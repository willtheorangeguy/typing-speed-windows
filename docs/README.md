# WinTypingSpeed — Documentation

A Windows tray application that measures typing speed across every application, using a
low-level keyboard hook that deliberately never learns which key you pressed.

```
typing-speed-windows/
├── WinTypingSpeed.Core/          session state machine, no UI, no packages
│   ├── TypingSessionTracker.cs   counting, timing, pause/resume — thread-safe
│   └── TypingSessionSnapshot.cs  immutable point-in-time view
├── WinTypingSpeed.App/           WPF host
│   ├── App.xaml.cs               wiring, 1s refresh timer, system events
│   ├── Services/GlobalKeyboardHook.cs
│   ├── Services/TrayIconHost.cs
│   └── MainWindow.xaml.cs        thin display layer
├── WinTypingSpeed.Core.Tests/    xUnit, Core only
├── installer/WinTypingSpeed.iss  Inno Setup 6
└── docs/                         this documentation
```

## Pages

- [Quickstart](./quickstart.md) — install, run, read the numbers
- [Installation](./installation.md) — installer or from source
- [Configuration](./configuration.md) — what there is (very little) and where state lives
- [Architecture](./architecture.md) — the hook, the tracker, and what is never captured
- [Development](./development.md) — build, test, layering
- [Deployment](./deployment.md) — the installer and its versioning
- [FAQ](./faq.md) — privacy, antivirus, shortcuts, why WPM falls
- [Troubleshooting](./troubleshooting.md) — no counting, hook removed, installer problems
- [Roadmap](./roadmap.md) — direction and non-goals
- [Known issues](./internal/known-issues.md) — recorded defects

## What it can and cannot see

A global keyboard hook is a keylogger's mechanism, so this deserves a plain answer.

`GlobalKeyboardHook.TryClassifyKey` maps every keystroke to one of three outcomes:

| Key | Becomes |
|---|---|
| Space or Enter | `' '` — a word boundary |
| Any other typing key | `'a'` — one character, identity discarded |
| Anything else | Ignored |

The real character is **never resolved**. Nothing downstream could reconstruct your typing even
if it wanted to, because the information is destroyed at the hook, not filtered later.

There is also no network code anywhere in the solution, and no file is written except by the
installer.

(There is a second reason for that design, in [Architecture](./architecture.md): calling
`ToUnicodeEx` inside a low-level hook corrupts dead-key state for every application on the
system.)

## Read this about the numbers

"Active time" is elapsed time since the session started or resumed — it does **not** exclude
periods when you were not typing. Since the installer offers to launch the app at sign-in, the
default configuration is one where WPM decays all day. See
[`internal/known-issues.md`](./internal/known-issues.md).
