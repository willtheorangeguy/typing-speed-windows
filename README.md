# WinTypingSpeed

`WinTypingSpeed` is a Windows tray app that tracks your live typing speed across Windows applications and shows session metrics for:

- current WPM
- total typed characters
- estimated words
- active session time
- pause/resume state
- reset of the current session

The app is built as a native `.NET 8` WPF desktop application with a separate core library for session logic and an xUnit test project for validation.

## Project structure

- `WinTypingSpeed.App` - WPF desktop app, tray icon host, global keyboard hook, and session window
- `WinTypingSpeed.Core` - framework-agnostic session tracking logic
- `WinTypingSpeed.Core.Tests` - unit tests for session math and lifecycle behavior
- `installer/WinTypingSpeed.iss` - Inno Setup 6 installer script
- `build-installer.ps1` - PowerShell script to publish the app and compile the installer
- `.github/workflows/build-installer.yml` - GitHub Actions workflow for automated installer builds
- `WinTypingSpeed.sln` - solution file

## Current behavior

- Tracks printable keyboard input globally across Windows while the app is running
- Estimates words using whitespace-delimited word boundaries
- Shows current WPM and session stats from the tray menu and the main app window
- Automatically pauses tracking during Windows lock/suspend and resumes after unlock/resume if the app was previously active
- Does not persist or display raw typed text

## Requirements

- Windows 10 or Windows 11
- `.NET 8 SDK` to build from source

You can verify your SDK with:

```powershell
dotnet --version
```

## Build

From the repository root:

```powershell
dotnet build .\WinTypingSpeed.sln
```

## Run

To run the app directly from source:

```powershell
dotnet run --project .\WinTypingSpeed.App\WinTypingSpeed.App.csproj
```

When the app starts, it lives in the Windows notification area/system tray. Double-click the tray icon or use the tray menu to open the main window.

## Test

To run the unit tests:

```powershell
dotnet test .\WinTypingSpeed.sln
```

## Install / publish

### Download the installer

Download the latest `WinTypingSpeed-x.y.z-Setup.exe` from the [Releases](../../releases) page and run it. The installer:

- Bundles the .NET runtime — no separate .NET installation required
- Installs to `C:\Program Files\WinTypingSpeed`
- Creates a Start Menu shortcut
- Optionally creates a desktop shortcut
- Optionally adds a Windows startup entry so the app launches at sign-in (enabled by default)
- Includes an uninstaller (available from Add/Remove Programs or the Start Menu)

### Build the installer locally

**Prerequisites:** [Inno Setup 6](https://jrsoftware.org/isdl.php) installed (add `ISCC.exe` to PATH, or the script will find it at the default install location).

From the repository root:

```powershell
.\build-installer.ps1
```

To specify a version explicitly:

```powershell
.\build-installer.ps1 -Version 1.2.3
```

The installer will be written to `dist\WinTypingSpeed-<version>-Setup.exe`. The script automatically resolves the version from the most recent git tag if `-Version` is not supplied.

### Automated builds (GitHub Actions)

The `Build Installer` workflow (`.github/workflows/build-installer.yml`) runs on:

| Trigger | Behaviour |
|---|---|
| `workflow_dispatch` | Builds the installer and uploads it as a workflow artifact (90-day retention) |
| Release published | Builds the installer, uploads it as an artifact, and attaches it to the GitHub release |

To trigger a manual build: go to **Actions → Build Installer → Run workflow** and optionally supply a version string.

## Notes

- Global keyboard tracking is used only to derive aggregate typing metrics.
- The app currently focuses on session tracking only; it does not save historical sessions.
