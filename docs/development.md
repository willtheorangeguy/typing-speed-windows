# WinTypingSpeed — Development

## Commands

```powershell
dotnet build .\WinTypingSpeed.sln
dotnet run --project .\WinTypingSpeed.App\WinTypingSpeed.App.csproj
dotnet test .\WinTypingSpeed.sln
dotnet test .\WinTypingSpeed.sln --filter "FullyQualifiedName~Pause_StopsActiveTime"
.\build-installer.ps1
.\build-installer.ps1 -Version 1.2.3
```

## Layering

| Project | May depend on |
|---|---|
| `WinTypingSpeed.Core` | The BCL. No UI, no NuGet packages |
| `WinTypingSpeed.App` | Core, WPF, Win32 P/Invoke. No NuGet packages |
| `WinTypingSpeed.Core.Tests` | Core, xUnit, coverlet |

The Core boundary is what makes the session logic testable at all — everything time-dependent
takes an injected `DateTimeOffset`, so tests assert exact durations without waiting or mocking a
clock.

**Keep Core free of `System.Windows` and P/Invoke.** The moment counting logic needs a hook
handle, none of it can be tested.

## Tests

`WinTypingSpeed.Core.Tests` covers counting, word boundaries, WPM, pause, resume, and reset.
Each test constructs a tracker with an explicit start time and passes explicit timestamps.

The hook, tray, window, and system-event wiring are **not** covered. Adding coverage there means
driving real keyboard input, which is why the line is drawn where it is — but it also means the
virtual-key table in `IsTypingKey` is only ever verified by hand.

Worth checking manually after touching the hook:

- Letters, digits, and punctuation all count.
- Space and Enter end a word; two spaces do not make two words.
- Function keys, arrows, and modifiers alone do not count.
- Typing stays responsive in other applications — a slow callback degrades the whole system.
- Dead keys still compose correctly (type `^` then `e` in a French layout).

That last one is the regression the hook's design exists to prevent; see
[Architecture](./architecture.md).

## Conventions

- **Nullable reference types are on** in all three projects. Do not add `#nullable disable`.
- **No NuGet packages** in Core or App.
- **UI updates on the Dispatcher.** The existing pattern updates from the timer callback, which
  already fires on the UI thread.
- **The tracker's public methods take an optional timestamp.** Keep that; it is the whole test
  strategy.
- **The hook callback stays minimal.** It blocks keyboard input system-wide while it runs.

## Releasing

Tag the repository, then publish a GitHub release. The workflow builds the installer and
attaches it.

`build-installer.ps1` resolves the version from `-Version`, then `$env:GITHUB_REF`, then
`git describe --tags`, then `1.0.0`. Tag before publishing — see [Deployment](./deployment.md).

## CI

`.github/workflows/build-installer.yml` runs on `workflow_dispatch` and on published releases. It
checks out full history (needed for tag resolution), runs `dotnet test` in Release, then builds
the installer.

Note there is no workflow on push or pull request, so tests run only when an installer is being
built.

## Recording defects

Bugs found while working here go in [`internal/known-issues.md`](./internal/known-issues.md)
rather than being fixed in passing, unless fixing them is the job you are on.
