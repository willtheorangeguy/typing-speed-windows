# WinTypingSpeed — Quickstart

## Install

Download `WinTypingSpeed-x.y.z-Setup.exe` from
[Releases](https://github.com/willtheorangeguy/typing-speed-windows/releases) and run it. The
.NET runtime is bundled, so nothing else is needed.

The installer offers a startup entry, **enabled by default**. Read the note at the bottom of
this page before accepting it.

## Or run from source

```powershell
dotnet build .\WinTypingSpeed.sln
dotnet run --project .\WinTypingSpeed.App\WinTypingSpeed.App.csproj
```

Requires the .NET 8 SDK — check with `dotnet --version`.

## Use it

The app has no main window at startup; it lives in the notification area.

| Action | How |
|---|---|
| Open the window | Double-click the tray icon |
| See live metrics | Tray menu, or the window |
| Pause or resume | Tray menu, or the window |
| Reset the session | Tray menu, or the window |

## The numbers

| Figure | Meaning |
|---|---|
| Current WPM | Words ÷ active minutes |
| Typed characters | Every counted keystroke this session |
| Estimated words | Completed words — a word ends at space or enter |
| Active time | Elapsed time since the session started or last resumed |
| Last input | When a key was last counted |

A word is whitespace-delimited. The word you are part-way through counts toward WPM but not
toward the displayed word total.

## Automatic pausing

Tracking stops on **workstation lock** and on **system suspend**, and resumes afterwards — but
only if it was running before, so a manual pause is not undone by locking your machine.

It does **not** pause when you simply stop typing.

## Why WPM falls the longer it runs

"Active time" is elapsed time, not time spent typing. Leave the app running for an hour and
type for five minutes of it, and WPM is computed over the full hour.

Use **Reset** before a stretch of work you actually want measured. Recorded in
[`internal/known-issues.md`](./internal/known-issues.md).

## About the startup entry

Enabling it means the app runs from sign-in — which is also the configuration in which the
figure above is least meaningful, since the session clock starts hours before you look at it.

Worth leaving off unless you plan to reset regularly.
