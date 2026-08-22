<!-- Logo -->
<h1 align="center">WinTypingSpeed</h1>

<!-- Copy -->
<h4 align="center">A Windows tray app that tracks your typing speed across every application — without ever knowing what you typed.</h4>

<!-- Badges -->
<div align="center">
  <img alt="Build Installer" src="https://img.shields.io/github/actions/workflow/status/willtheorangeguy/typing-speed-windows/build-installer.yml?label=installer">
  <img alt="GitHub Issues" src="https://img.shields.io/github/issues/willtheorangeguy/typing-speed-windows">
  <img alt="GitHub Pull Requests" src="https://img.shields.io/github/issues-pr/willtheorangeguy/typing-speed-windows">
  <img alt="License" src="https://img.shields.io/github/license/willtheorangeguy/typing-speed-windows">
</div>

<!-- Navigation -->
<p align="center">
  <a href="#key-features">Key Features</a> •
  <a href="#installation">Installation</a> •
  <a href="#usage">Usage</a> •
  <a href="#documentation">Documentation</a> •
  <a href="#support">Support</a> •
  <a href="#contributing">Contributing</a> •
  <a href="#credits">Credits</a> •
  <a href="#license">License</a>
</p>

## Key Features

- Live WPM, characters, words, and session time, from the tray or the main window.
- Works across every Windows application, not just an editor.
- **Cannot record what you type** — the hook never resolves a keystroke to its actual character. Every typing key becomes the letter `a`; only space and enter are distinguished, because only word boundaries matter.
- Auto-pauses on lock or sleep, and resumes afterwards if it was running.
- Native .NET 8 WPF, no NuGet packages outside the test project, and no network code at all.
- Ships as a self-contained Inno Setup installer with the runtime bundled.

## Installation

Download the latest `WinTypingSpeed-x.y.z-Setup.exe` from [Releases](https://github.com/willtheorangeguy/typing-speed-windows/releases) and run it.

Or build from source with the .NET 8 SDK:

```powershell
dotnet build .\WinTypingSpeed.sln
dotnet run --project .\WinTypingSpeed.App\WinTypingSpeed.App.csproj
```

## Usage

The app lives in the notification area. Double-click the tray icon for the main window, or use the tray menu to pause, resume, or reset.

## Documentation

Full documentation lives in [`docs/`](docs/index.md):
[Quickstart](docs/quickstart.md) · [Installation](docs/installation.md) · [Configuration](docs/configuration.md) · [Architecture](docs/architecture.md) · [Development](docs/development.md) · [Deployment](docs/deployment.md) · [FAQ](docs/faq.md) · [Troubleshooting](docs/troubleshooting.md) · [Roadmap](docs/roadmap.md)

## Support

Open a [GitHub Discussion](https://github.com/willtheorangeguy/typing-speed-windows/discussions/new) or file an [issue](https://github.com/willtheorangeguy/typing-speed-windows/issues/new/choose).

## Contributing

Contributions welcome. See the org-wide [Contributing Guide](https://github.com/willtheorangeguy/.github/blob/main/CONTRIBUTING.md) and [Code of Conduct](https://github.com/willtheorangeguy/.github/blob/main/CODE_OF_CONDUCT.md).

## Credits

Built with [.NET 8](https://dotnet.microsoft.com/) and WPF. Installer by [Inno Setup 6](https://jrsoftware.org/isinfo.php). Tested with [xUnit](https://xunit.net/).

## License

MIT — see [`LICENSE.md`](LICENSE.md).

> A global keyboard hook is a serious thing to install. [`docs/architecture.md`](docs/architecture.md) explains exactly what this one does and does not see.
