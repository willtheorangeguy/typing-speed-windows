# WinTypingSpeed — Configuration

There is nothing to configure. No settings file, no registry keys of the app's own, no
command-line options, and no options dialog.

This page documents the constants and the state instead.

## Runtime state

Everything is in memory in a single `TypingSessionTracker`:

| Field | Meaning |
|---|---|
| `sessionStartedAt` | When the session began, or was last reset |
| `activeSegmentStartedAt` | When the current unpaused segment began; `null` while paused |
| `accumulatedActiveTime` | Time from completed segments |
| `lastInputAt` | Last counted keystroke — **displayed, but not used in any calculation** |
| `typedCharacterCount` | Counted keystrokes |
| `estimatedWordCount` | Completed words |
| `currentWordCharacterCount` | Characters in the word in progress |
| `isPaused` | Manual or system pause |

**None of it is persisted.** Closing the app discards the session; there is no file, no
registry value, and nothing to clear.

## Fixed values

| Value | Where | Effect |
|---|---|---|
| 1 second | `App.xaml.cs` → `refreshTimer` | Tray and window refresh rate |
| `WH_KEYBOARD_LL` (13) | `GlobalKeyboardHook` | The hook type |
| `WM_KEYDOWN`, `WM_SYSKEYDOWN` | `GlobalKeyboardHook` | Messages counted; **`WM_SYSKEYDOWN` means Alt combinations count too** |
| Word boundary | `VK_SPACE`, `VK_RETURN` | Tab does not end a word |
| Typing keys | 0–9, A–Z, numpad, OEM ranges | See `IsTypingKey` |

The virtual-key ranges are the definition of "a character was typed". Modifier state is not
consulted, so shortcuts count — recorded in
[`internal/known-issues.md`](./internal/known-issues.md).

## What triggers a pause

| Event | Source |
|---|---|
| Tray or window control | The user |
| `SessionSwitchReason.SessionLock` | `SystemEvents` |
| `PowerModes.Suspend` | `SystemEvents` |

Resume happens on unlock and on power resume, guarded by `resumeAfterSystemPause` so a manual
pause survives a lock.

Idleness triggers nothing.

## Installer options

Set at install time, in `installer/WinTypingSpeed.iss`:

| Option | Default |
|---|---|
| Install path | `C:\Program Files\WinTypingSpeed` |
| Start Menu shortcut | Yes |
| Desktop shortcut | Optional |
| Run at sign-in | **Yes** |

## Versioning

`build-installer.ps1` resolves the version in priority order:

1. An explicit `-Version` argument
2. The release tag, from `$env:GITHUB_REF`
3. `git describe --tags`
4. `1.0.0`

So a build from a shallow clone with no tags produces `1.0.0` — see [Deployment](./deployment.md).
