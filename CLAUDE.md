# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WinTypingSpeed is a Windows tray application that tracks live typing speed (WPM) globally across all applications. It uses a low-level keyboard hook (`WH_KEYBOARD_LL`) to intercept keystrokes system-wide and computes words-per-minute based on whitespace-delimited word boundaries. The app pauses automatically on system lock or power suspend.

## Commands

```powershell
# Build
dotnet build .\WinTypingSpeed.sln

# Run the app
dotnet run --project .\WinTypingSpeed.App\WinTypingSpeed.App.csproj

# Run all tests
dotnet test .\WinTypingSpeed.sln

# Run a single test by name
dotnet test .\WinTypingSpeed.sln --filter "FullyQualifiedName~<TestMethodName>"

# Build the Inno Setup installer (requires Inno Setup 6 installed)
.\build-installer.ps1
.\build-installer.ps1 -Version 2.0.0   # explicit version override
```

## Architecture

The solution has three projects with a strict layering:

**WinTypingSpeed.Core** — no UI dependencies, no NuGet packages. Contains:
- `TypingSessionTracker`: the session state machine. All public methods are thread-safe via a single `lock(syncRoot)`. Tracks character count, word count, and active time. WPM = `words / activeMinutes`. Words increment on whitespace; the in-progress (unfinished) word is counted fractionally in snapshots.
- `TypingSessionSnapshot`: an immutable `record` capturing a point-in-time view of the session. The App layer reads snapshots; it never touches tracker internals directly.

**WinTypingSpeed.App** — orchestrates everything. Key relationships:
- `App.xaml.cs` owns the `TypingSessionTracker`, `GlobalKeyboardHook`, `TrayIconHost`, and `MainWindow`. It wires them together, runs a 1-second `DispatcherTimer` to push snapshots to the UI, and listens to `SystemEvents` for power mode and session lock/unlock to call `Pause()`/`Resume()` on the tracker.
- `GlobalKeyboardHook` installs the `WH_KEYBOARD_LL` hook and maps virtual key codes to characters manually (avoids `ToUnicodeEx` to prevent dead-key state corruption). It raises events that `App.xaml.cs` converts into `RecordCharacter` calls on the tracker.
- `TrayIconHost` hosts the system-tray icon and context menu (showing live metrics). Controls (Pause/Resume/Reset) call back into the tracker.
- `MainWindow.xaml.cs` is a thin display layer; it only reads snapshots provided by `App.xaml.cs` via a `UpdateDisplay(TypingSessionSnapshot)` call on the UI thread.

**WinTypingSpeed.Core.Tests** — xUnit tests covering only Core logic. No UI or hook code is tested directly. Tests construct a `TypingSessionTracker` with an injected `DateTime` to control time.

## Key Conventions

- **Nullable reference types are enabled** in all three projects. Do not introduce `#nullable disable` or suppress nullable warnings without a clear reason.
- **No external NuGet packages** in Core or App — keep the dependency surface minimal. Tests use xUnit + coverlet only.
- **UI thread safety**: Any update to WPF controls must happen on the Dispatcher. The existing pattern is to call `Dispatcher.Invoke` or update only from within the timer callback (which already fires on the UI thread).
- **Installer versioning**: The build script resolves version in priority order — explicit `-Version` argument → release tag (`$env:GITHUB_REF`) → `git describe --tags` → `1.0.0`. Tag the repo before publishing a release.

## CI/CD

GitHub Actions workflow (`.github/workflows/build-installer.yml`) runs on `workflow_dispatch` (manual, with optional version input) and on published releases. It checks out full history for git-tag resolution, runs `dotnet test` in Release mode, then calls `build-installer.ps1`. The installer artifact is attached to the release or uploaded as a workflow artifact (90-day retention).
