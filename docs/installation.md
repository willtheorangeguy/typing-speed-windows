# WinTypingSpeed — Installation

## Requirements

| | |
|---|---|
| OS | Windows 10 or Windows 11 |
| To run the installer | Nothing — the .NET runtime is bundled |
| To build from source | .NET 8 SDK |
| To build the installer | [Inno Setup 6](https://jrsoftware.org/isdl.php) |

```powershell
dotnet --version
```

## From the installer

Download `WinTypingSpeed-x.y.z-Setup.exe` from
[Releases](https://github.com/willtheorangeguy/typing-speed-windows/releases) and run it.

| It does | Notes |
|---|---|
| Bundles the .NET runtime | Self-contained; no separate install |
| Installs to `C:\Program Files\WinTypingSpeed` | Requires elevation |
| Creates a Start Menu shortcut | |
| Offers a desktop shortcut | Optional |
| Offers a startup entry | **Enabled by default** |
| Registers an uninstaller | Add/Remove Programs, or the Start Menu |

The startup entry is worth a moment's thought. It is convenient, and it is also the setup in
which the WPM figure means least — see [Quickstart](./quickstart.md).

### Windows SmartScreen

The installer is not signed with a code-signing certificate, so SmartScreen will warn on first
run: **More info → Run anyway**.

### Antivirus

A global keyboard hook is what a keylogger uses, so some antivirus products flag this app on
heuristics. What the hook actually does is in [Architecture](./architecture.md) — it discards
key identity at the hook itself — and the source is here to check.

## From source

```powershell
git clone https://github.com/willtheorangeguy/typing-speed-windows.git
cd typing-speed-windows
dotnet build .\WinTypingSpeed.sln
dotnet run --project .\WinTypingSpeed.App\WinTypingSpeed.App.csproj
```

No NuGet restore is needed for the app: Core and App take no external packages. Only the test
project pulls anything (xUnit and coverlet).

## Verify

```powershell
dotnet test .\WinTypingSpeed.sln
```

Then run the app, type, and watch the tray menu. If the counters stay at zero the hook did not
install — see [Troubleshooting](./troubleshooting.md).

## Uninstall

Add/Remove Programs, or the Start Menu uninstaller. The app writes no configuration and no
session file, so nothing is left behind beyond the startup entry the uninstaller removes.
