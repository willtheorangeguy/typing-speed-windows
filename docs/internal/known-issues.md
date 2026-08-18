# Known Issues — typing-speed-windows

Concrete defects and gaps found while writing this repository's documentation in
August 2026. **Nothing here was changed** — each one needs a code, configuration, or
licensing decision rather than a documentation one.

Ordered by severity. See [`docs/roadmap.md`](../roadmap.md) for the narrative version,
which also covers deliberate non-goals.


**5 open:** 1 high, 3 medium, 1 low.

## 1. "Active time" is elapsed time, so WPM decays for as long as the app runs

**Severity:** High  
**Where:** `WinTypingSpeed.Core/TypingSessionTracker.cs` -> `GetCurrentSegmentDuration`, `GetSnapshot`

**What:** `GetCurrentSegmentDuration` returns `now - activeSegmentStartedAt` -- wall-clock time since the session started or was last resumed -- and `GetSnapshot` divides the word count by it. `lastInputAt` is recorded on every keystroke, carried in the snapshot, and displayed, but is never used in any calculation. The only things that stop the clock are a manual pause, workstation lock, and system suspend. Idleness does not.

**Why it matters:** The headline number is words divided by how long the program has been open. Type 300 words in the first ten minutes and leave the app running for eight hours, and it reports about 0.6 WPM -- a plausible-looking figure that measures nothing. The installer offers a startup entry and enables it by default, so the shipped configuration is precisely the one where the session clock starts at sign-in and never stops. Both the README and CLAUDE.md call this 'active time', and the tests assert the elapsed-time behaviour, so nothing anywhere flags the discrepancy. The sibling project `typing-speed-vscode` excludes idle gaps explicitly, which makes this look like an omission rather than a considered difference.

**Suggested fix:** Track active time the way the VS Code sibling does: accumulate only the gaps between consecutive keystrokes that are shorter than an idle threshold, using the `lastInputAt` already being recorded. If elapsed time is genuinely wanted, rename the field and the display to 'session time' and compute WPM over something else -- a rolling window would answer the question users are actually asking.

## 2. Keyboard shortcuts are counted as typed characters

**Severity:** Medium  
**Where:** `WinTypingSpeed.App/Services/GlobalKeyboardHook.cs` -> `HandleHook`, `TryClassifyKey`

**What:** The hook handles `WM_KEYDOWN` **and** `WM_SYSKEYDOWN` (the Alt-combination message), and `TryClassifyKey` consults only the virtual-key code. Modifier state is never checked -- not via `GetKeyState`, and not via the `Flags` field of `KBDLLHOOKSTRUCT`, which the struct declares and the code reads but never uses. `Ctrl+S`, `Ctrl+C`, `Ctrl+Shift+P`, and `Alt+F` all count their letter as a typed character.

**Why it matters:** Editing work is dense with shortcuts, so the inflation is neither small nor evenly spread: somebody who saves compulsively scores higher than somebody who does not, at identical actual typing speed. Because the counter only rises, the error is invisible -- there is no way to tell a session's figure from a correct one. Including `WM_SYSKEYDOWN` explicitly suggests Alt combinations were considered and deliberately admitted, which is the opposite of what a typing-speed measure wants.

**Suggested fix:** Check the modifier state in `HandleHook` and ignore the event when Ctrl or Alt is down -- `GetKeyState(VK_CONTROL)` and the `LLKHF_ALTDOWN` bit of the `Flags` field already available in the struct. Shift should still count, since shifted characters are typing.

## 3. Key auto-repeat is counted, so holding a key inflates the count

**Severity:** Medium  
**Where:** `WinTypingSpeed.App/Services/GlobalKeyboardHook.cs` -> `HandleHook`

**What:** Holding a key produces repeated `WM_KEYDOWN` messages at the system repeat rate, and each one is classified and counted. There is no de-duplication: no tracking of which keys are currently down, and no use of the timestamp available in `KBDLLHOOKSTRUCT.Time`.

**Why it matters:** A held arrow key does not fire (it is not a typing key), but a held letter, digit, or punctuation key does -- and so does a held Enter or Space, which manufactures words as well as characters. At a typical repeat rate that is roughly thirty characters per second held, far above any real typing speed, and it takes only a stuck key or a leaned-on keyboard to make a session's figures meaningless. Combined with the elapsed-time issue above, the two errors push in opposite directions and neither is visible.

**Suggested fix:** Track the set of currently-pressed virtual keys and ignore a `WM_KEYDOWN` for a key already held, clearing it on `WM_KEYUP`. That requires handling the key-up messages the hook currently discards.

## 4. A hook removed by Windows for exceeding its timeout is never detected

**Severity:** Medium  
**Where:** `WinTypingSpeed.App/Services/GlobalKeyboardHook.cs`, `WinTypingSpeed.App/App.xaml.cs`

**What:** Windows silently uninstalls a `WH_KEYBOARD_LL` hook whose callback exceeds `LowLevelHooksTimeout` (300 ms by default) -- no callback, no error, no notification. `Start()` throws if `SetWindowsHookEx` fails, but nothing afterwards verifies the hook is still installed, and `hookHandle` remains non-zero, so `Start()` would decline to reinstall even if something called it.

**Why it matters:** The app carries on looking entirely healthy: the tray icon is there, the window opens, the timer ticks, and the counters simply stop moving. That is indistinguishable from not having typed, which is exactly the state a typing tracker is often in, so it can go unnoticed for a whole session. A machine under heavy load is both when this is most likely and when the user is least likely to question it.

**Suggested fix:** Re-arm periodically: on a timer, or on the existing 1-second refresh tick, reinstall the hook when no input has been seen for a while. Windows raises `WM_TIMER`-free approaches too -- simply calling `SetWindowsHookEx` again after a failure window is cheap and idempotent enough for this.

## 5. build-installer.ps1 falls back to version 1.0.0 silently

**Severity:** Low  
**Where:** `build-installer.ps1`

**What:** The version is resolved as `-Version`, then `$env:GITHUB_REF`, then `git describe --tags`, then the literal `1.0.0`. The last step is a plain fallback -- no warning and no failure.

**Why it matters:** A build from a repository with no tags, or a shallow clone where `git describe` cannot reach one, produces `WinTypingSpeed-1.0.0-Setup.exe`: a normal-looking installer with a wrong version, which then collides with any genuine 1.0.0 already released. Nothing downstream notices, because the filename is well-formed. CI checks out full history specifically to make the third step work, which shows the fragility was understood without being surfaced.

**Suggested fix:** Warn loudly when the fallback is used, or fail the build unless an explicit `-Version` is supplied. A version derived from nothing is worse than a build that refuses to run.


---

## Also, across every repository

**`.bandit` is present on disk but untracked in git.** Verified in PyWorkout, treklogger,
skyscanner-cli, booking-cli, piggy, and aibot — the config file exists locally in each but
`git ls-files` does not know about it, so none of it reached GitHub.

The August 2026 security sweep therefore looks complete locally and landed nowhere. Worth
checking across all 44 repositories it covered.
