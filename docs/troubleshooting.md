# WinTypingSpeed — Troubleshooting

## Counters stay at zero while I type

**1. Is tracking paused?** Check the tray menu. It may have auto-paused on a lock and not
resumed — resume happens only if it was running when the lock began.

**2. Did the hook install?** `GlobalKeyboardHook.Start` throws a `Win32Exception` when
`SetWindowsHookEx` fails, so a failure at startup is loud. Silence plus zero counting points at
the next item.

**3. Was the hook removed by Windows?** A low-level hook that takes longer than
`LowLevelHooksTimeout` (300 ms by default) is silently uninstalled. Nothing here detects that, so
the symptom is counting that simply stops. Restart the app. Recorded in
[`internal/known-issues.md`](./internal/known-issues.md).

**4. Are you typing into something the hook cannot see?**

| Case | Why |
|---|---|
| An elevated application | A non-elevated hook does not receive its input |
| UAC prompt, sign-in screen, Ctrl+Alt+Del | Secure desktops exclude hooks entirely |
| Some games | Anti-cheat systems block hooks |

Running the app as administrator covers the first.

## WPM looks far too low

Almost certainly the elapsed-time definition of "active time": your words divided by however
long the app has been running, not by how long you were typing.

Reset the session and type — the figure is meaningful over a short measured stretch and not much
else. See [`internal/known-issues.md`](./internal/known-issues.md).

## WPM looks too high

Shortcuts and key repeat both count. A session of `Ctrl+S`, `Ctrl+C`, and held arrow keys will
be flattering.

## Character count rises when I am not typing letters

Same cause. Modifier state is not consulted, so the letter in a shortcut is counted.

## Tracking stopped after locking my machine

It should resume on unlock, but only if it was running before the lock — a manual pause is
deliberately preserved across a lock. Resume from the tray menu.

## Tracking stopped after sleep

`PowerModes.Resume` should restart it. If a resume event is missed, resume manually. Restarting
the app also reinstalls the hook, which is worth doing after a sleep that ended oddly.

## Antivirus quarantined it

Expected on heuristics — see [FAQ](./faq.md). Add an exclusion if you are satisfied with what
the hook does; [Architecture](./architecture.md) sets it out and the source is four files.

## SmartScreen blocks the installer

Unsigned installer: **More info → Run anyway**.

## The window closed but the app is still running

By design. It is a tray app — closing the window hides it. Exit from the tray menu.

## `dotnet build` fails

| Check | Detail |
|---|---|
| SDK version | `dotnet --version` must report 8.x |
| Right solution | Build `WinTypingSpeed.sln` from the repository root |
| Platform | The App project is Windows-only; Core builds anywhere |

## The installer script cannot find ISCC

`build-installer.ps1` looks on PATH and at Inno Setup's default install location. Install
[Inno Setup 6](https://jrsoftware.org/isdl.php), or add `ISCC.exe` to PATH.

## The installer came out as version 1.0.0

The version fell through to the last fallback — no `-Version`, no release tag, and no tag
reachable from `git describe`. Tag the repository, or pass `-Version` explicitly. See
[Deployment](./deployment.md).

## Still stuck

[Open an issue](https://github.com/willtheorangeguy/typing-speed-windows/issues/new/choose) with
your Windows version, whether the app is elevated, and what you were typing into.
