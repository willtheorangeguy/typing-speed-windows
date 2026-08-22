# WinTypingSpeed — Architecture

Three projects with strict layering: Core knows nothing about UI, App knows nothing about
counting.

```text
keystroke (anywhere in Windows)
   └── WH_KEYBOARD_LL hook
          └── TryClassifyKey  → ' ' | 'a' | ignored
                 └── App.xaml.cs → tracker.RecordCharacter(c)
                        └── 1s DispatcherTimer → GetSnapshot()
                               ├── TrayIconHost   (menu text)
                               └── MainWindow     (UpdateDisplay)
```

## The hook, and what it refuses to know

`GlobalKeyboardHook` installs `WH_KEYBOARD_LL` and handles `WM_KEYDOWN` and `WM_SYSKEYDOWN`.

`TryClassifyKey` reduces every keystroke to one of three outcomes:

| Virtual key | Result |
|---|---|
| `VK_SPACE`, `VK_RETURN` | `' '` — a word boundary |
| 0–9, A–Z, numpad, the OEM punctuation ranges | `'a'` — one character |
| Everything else | Ignored, `false` |

**The actual character is never resolved.** Only the whitespace distinction affects counting, so
the identity is discarded at the point of capture rather than filtered downstream. That is the
difference between a program that chooses not to record your typing and one that structurally
cannot.

### Why not `ToUnicodeEx`

The obvious way to get a real character is `ToUnicodeEx` with `GetKeyboardState`. The code
refuses, and says why:

> calling ToUnicodeEx inside a WH_KEYBOARD_LL hook mutates the system dead-key state and
> corrupts keyboard input in every running application.

A dead key (`^` then `e` → `ê`) is held in per-thread state that `ToUnicodeEx` consumes. Calling
it from a hook consumes it out from under the application the user is actually typing into. So
the privacy property here is also the correctness property.

### Constraints of a low-level hook

The callback runs on the thread that installed it and **blocks all keyboard input** until it
returns. Windows silently removes a hook that exceeds `LowLevelHooksTimeout` (300 ms by
default).

So the callback does the minimum: read the struct, classify, raise an event, chain on. Anything
slow added here would degrade typing across the whole machine, and then the hook would be
removed with no notification at all — see
[`internal/known-issues.md`](./internal/known-issues.md).

## `TypingSessionTracker`

The state machine, in Core, with no UI or platform dependencies. Every public method takes an
optional `DateTimeOffset`, defaulting to now — which is what makes the tests deterministic
without a clock abstraction.

All public methods lock a single `syncRoot`, because `RecordCharacter` arrives on the hook thread
while `GetSnapshot` is called from the UI timer.

**Counting.** Any non-whitespace character increments `typedCharacterCount` and
`currentWordCharacterCount`. Whitespace increments the character count and completes the word —
and `CompleteCurrentWord` returns early when the in-progress word is empty, so consecutive
spaces do not manufacture words.

**Timing.** `accumulatedActiveTime` holds completed segments; `GetCurrentSegmentDuration`
returns `now - activeSegmentStartedAt` for the open one. Pausing folds the open segment in and
clears the marker; resuming opens a new one.

Note what that means: **the clock runs on wall time from the moment the session starts or
resumes, regardless of whether anything is typed.** `lastInputAt` is recorded and displayed but
never consulted. See [`internal/known-issues.md`](./internal/known-issues.md).

**WPM.**

```text
effectiveWordCount = estimatedWordCount + (currentWordCharacterCount > 0 ? 1 : 0)
CurrentWpm = effectiveWordCount / activeTime.TotalMinutes
```

The in-progress word counts toward WPM but not toward the displayed word total, so the two
differ by at most one.

## `TypingSessionSnapshot`

An immutable `record`. App reads snapshots and never touches tracker internals, so the display
can never disagree with the state — and the tracker's locking does not have to extend into UI
code.

## The App layer

`App.xaml.cs` owns everything and wires it together:

- A one-second `DispatcherTimer` pulls a snapshot and pushes it to the tray and window.
- `SystemEvents.SessionSwitch` and `SystemEvents.PowerModeChanged` drive automatic pause and
  resume, guarded by `resumeAfterSystemPause` so a manual pause survives a lock.
- `TrayIconHost` owns the notification-area icon and menu; `MainWindow` only renders what it is
  given.

There is no persistence layer, because there is nothing persisted.

## What is not tested

The tests cover Core alone. The hook, the tray, the window, and the system-event wiring have no
automated coverage — which is a reasonable line to draw, since testing a global keyboard hook
means driving real input, but it does mean the classification table in `IsTypingKey` is verified
only by hand.
