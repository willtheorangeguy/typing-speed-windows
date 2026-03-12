<#
.SYNOPSIS
    Publishes WinTypingSpeed.App and compiles the Inno Setup installer.

.PARAMETER Version
    The version string embedded in the installer (e.g. "1.2.3").
    Defaults to the most recent git tag, or "1.0.0" if no tag is found.

.PARAMETER Configuration
    The .NET build configuration. Defaults to "Release".

.EXAMPLE
    .\build-installer.ps1
    .\build-installer.ps1 -Version 2.0.0
#>
[CmdletBinding()]
param(
    [string] $Version,
    [string] $Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Resolve version
# ---------------------------------------------------------------------------
if (-not $Version) {
    try {
        $tag = git describe --tags --abbrev=0 2>$null
        if ($tag -match '^v?(\d+\.\d+\.\d+)') {
            $Version = $Matches[1]
        }
    } catch { }
}
if (-not $Version) { $Version = '1.0.0' }

Write-Host "Building WinTypingSpeed installer v$Version ($Configuration)" -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------
$Root         = $PSScriptRoot
$AppProject   = Join-Path $Root 'WinTypingSpeed.App\WinTypingSpeed.App.csproj'
$InstallerIss = Join-Path $Root 'installer\WinTypingSpeed.iss'
$DistDir      = Join-Path $Root 'dist'

# ---------------------------------------------------------------------------
# 1. Publish self-contained win-x64
# ---------------------------------------------------------------------------
Write-Host "`n[1/2] Publishing .NET app..." -ForegroundColor Cyan

$PublishArgs = @(
    'publish', $AppProject,
    '-c', $Configuration,
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=false',   # keep as a directory so Inno Setup can include all files
    "-p:AssemblyVersion=$Version",
    "-p:FileVersion=$Version",
    "-p:InformationalVersion=$Version"
)

& dotnet @PublishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE."
}

Write-Host "Publish succeeded." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2. Compile Inno Setup installer
# ---------------------------------------------------------------------------
Write-Host "`n[2/2] Compiling Inno Setup installer..." -ForegroundColor Cyan

$IsccPaths = @(
    'ISCC.exe',                                          # already on PATH (CI)
    'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
    'C:\Program Files\Inno Setup 6\ISCC.exe'
)

$Iscc = $IsccPaths | Where-Object { Test-Path $_ -ErrorAction SilentlyContinue } | Select-Object -First 1
if (-not $Iscc) {
    # Fall back to PATH resolution
    $Iscc = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
}
if (-not $Iscc) {
    Write-Error "ISCC.exe not found. Install Inno Setup 6 from https://jrsoftware.org/isdl.php and ensure it is on PATH."
}

New-Item -ItemType Directory -Force -Path $DistDir | Out-Null

& $Iscc $InstallerIss "/DAppVersion=$Version"
if ($LASTEXITCODE -ne 0) {
    Write-Error "ISCC.exe failed with exit code $LASTEXITCODE."
}

$InstallerExe = Join-Path $DistDir "WinTypingSpeed-$Version-Setup.exe"
if (Test-Path $InstallerExe) {
    Write-Host "`nInstaller ready: $InstallerExe" -ForegroundColor Green
} else {
    Write-Warning "Expected installer not found at $InstallerExe - check ISCC output above."
}
