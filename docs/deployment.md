# WinTypingSpeed — Deployment

## Building the installer

```powershell
.\build-installer.ps1
.\build-installer.ps1 -Version 1.2.3
```

Requires [Inno Setup 6](https://jrsoftware.org/isdl.php). The script finds `ISCC.exe` on PATH or
at the default install location.

Output: `dist\WinTypingSpeed-<version>-Setup.exe`, self-contained with the .NET runtime bundled.

## Versioning

Resolved in priority order:

1. `-Version`
2. `$env:GITHUB_REF` — the release tag
3. `git describe --tags`
4. `1.0.0`

The last one is a silent fallback, not an error. A build from a shallow clone, or from a
repository with no tags, produces an installer labelled `1.0.0` that looks entirely normal.
**Tag before publishing**, and check the filename the script reports.

## Automated builds

`.github/workflows/build-installer.yml`:

| Trigger | Behaviour |
|---|---|
| `workflow_dispatch` | Builds and uploads as a workflow artifact (90-day retention), with an optional version input |
| Release published | Builds, uploads the artifact, and attaches the installer to the release |

The workflow checks out full history so `git describe --tags` can resolve, and runs
`dotnet test` in Release before building.

There is **no** push or pull-request workflow, so a change only meets the test suite when
someone builds an installer.

## Manual release

**Actions → Build Installer → Run workflow**, optionally supplying a version.

## Code signing

The installer is unsigned, so Windows SmartScreen warns on first run and users must choose
**More info → Run anyway**.

For an app that installs a global keyboard hook, that is a worse first impression than usual —
the warning arrives at exactly the moment a cautious user is already weighing whether to trust
it. A code-signing certificate would remove it.

## What the installer places

| | |
|---|---|
| Program files | `C:\Program Files\WinTypingSpeed` |
| Start Menu shortcut | Always |
| Desktop shortcut | Optional |
| Startup entry | **Enabled by default** |
| Uninstaller | Add/Remove Programs and Start Menu |

The default startup entry deserves attention: it is also the configuration in which the WPM
figure is least meaningful, since the session clock starts at sign-in and never pauses for
idleness. See [`internal/known-issues.md`](./internal/known-issues.md).

## Nothing else is written

No configuration file, no session file, no registry values beyond what Inno Setup creates for
the uninstaller and the optional startup entry. Uninstalling leaves nothing behind.
