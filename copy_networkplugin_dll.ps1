# Copies NetworkPlugin.dll to the target folder (overwrite if different).
# Usage: powershell -ExecutionPolicy Bypass -File .\copy_networkplugin_dll.ps1

[CmdletBinding()]
param(
  # Build the NetworkPlugin project before copying.
  [switch]$Build = $true,

  # Clean the NetworkPlugin project before building.
  [switch]$Clean = $false,

  # Skip restore when building (useful for offline/dev loops).
  [switch]$NoRestore = $false,

  # dotnet build configuration (only used when -Build is specified).
  [ValidateSet('Debug', 'Release')]
  [string]$BuildConfiguration = 'Debug',

  # dotnet verbosity (only used when -Build is specified).
  [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
  [string]$BuildVerbosity = 'minimal',

  # Source DLL path. If omitted, defaults to a path relative to this script.
  [string]$SourceDll,

  # Also copy the adjacent PDB if present. Needed for some loaders (e.g. ScriptEngine)
  # that call Mono.Cecil with ReadSymbols=true.
  [switch]$CopyPdb = $true,

  # Also copy the adjacent deps.json if present (harmless for BepInEx, useful for some tooling).
  [switch]$CopyDepsJson = $false,

  # Default destination: the folder you specified.
  [string]$DestDir = "D:\steam\steamapps\common\LBoL\Mods\1"
)

$ErrorActionPreference = 'Stop'

$scriptRoot = $null
if ($PSScriptRoot) {
  $scriptRoot = $PSScriptRoot
} elseif ($MyInvocation.MyCommand.Path) {
  # Covers cases where only part of the script is executed in an interactive session.
  $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
} else {
  $scriptRoot = (Get-Location).Path
}

if ($Build) {
  $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
  if (-not $dotnet) {
    throw "dotnet SDK not found on PATH. Install .NET SDK or add dotnet to PATH."
  }

  $projectDir = Join-Path -Path $scriptRoot -ChildPath "networkplugin"
  $projectPath = Join-Path -Path $projectDir -ChildPath "NetWorkPlugin.csproj"
  if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    # Fallback: if the csproj name changes, build the folder.
    $projectPath = $projectDir
  }

  if ($Clean) {
    Write-Host "Cleaning NetworkPlugin ($BuildConfiguration)..." -ForegroundColor Cyan
    & $dotnet.Source clean $projectPath -c $BuildConfiguration -v $BuildVerbosity
    if ($LASTEXITCODE -ne 0) {
      throw "dotnet clean failed (exit code $LASTEXITCODE)."
    }
  }

  Write-Host "Building NetworkPlugin ($BuildConfiguration)..." -ForegroundColor Cyan
  $buildArgs = @(
    'build',
    $projectPath,
    '-c', $BuildConfiguration,
    '-v', $BuildVerbosity
  )
  if ($NoRestore) {
    $buildArgs += '--no-restore'
  }

  & $dotnet.Source @buildArgs
  if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed (exit code $LASTEXITCODE)."
  }

  Write-Host "Build complete: NetworkPlugin ($BuildConfiguration)" -ForegroundColor Green
}

if ([string]::IsNullOrWhiteSpace($SourceDll)) {
  $SourceDll = Join-Path -Path $scriptRoot -ChildPath "networkplugin\bin\$BuildConfiguration\netstandard2.1\NetworkPlugin.dll"
} elseif (-not [System.IO.Path]::IsPathRooted($SourceDll)) {
  $SourceDll = Join-Path -Path $scriptRoot -ChildPath $SourceDll
}

if (-not (Test-Path -LiteralPath $SourceDll -PathType Leaf)) {
  throw "Source DLL not found: $SourceDll"
}

$sourceFull = (Resolve-Path -LiteralPath $SourceDll).Path

if (-not (Test-Path -LiteralPath $DestDir -PathType Container)) {
  New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
}

$destFile = Join-Path -Path $DestDir -ChildPath "NetworkPlugin.dll"
$destFull = $destFile
if (Test-Path -LiteralPath $destFile -PathType Leaf) {
  try { $destFull = (Resolve-Path -LiteralPath $destFile).Path } catch { $destFull = $destFile }
}

# If source and destination are the same file, copying will fail and is unnecessary.
if ($sourceFull -ieq $destFull) {
  Write-Host "Source and destination are the same file; nothing to copy:" -ForegroundColor Yellow
  Write-Host "  $sourceFull"
  return
}

Copy-Item -LiteralPath $sourceFull -Destination $destFile -Force

# Copy Assets folder if present
$sourceAssets = Join-Path -Path $scriptRoot -ChildPath "networkplugin\Assets"
if (Test-Path -LiteralPath $sourceAssets -PathType Container) {
  $destAssets = Join-Path -Path $DestDir -ChildPath "Assets"
  if (-not (Test-Path -LiteralPath $destAssets -PathType Container)) {
    New-Item -ItemType Directory -Path $destAssets -Force | Out-Null
  }
  Copy-Item -Path (Join-Path $sourceAssets "*") -Destination $destAssets -Force -Recurse
}

if ($CopyPdb) {
  $sourcePdb = [System.IO.Path]::ChangeExtension($sourceFull, '.pdb')
  if (Test-Path -LiteralPath $sourcePdb -PathType Leaf) {
    $destPdb = [System.IO.Path]::ChangeExtension($destFile, '.pdb')
    Copy-Item -LiteralPath $sourcePdb -Destination $destPdb -Force
  }
}

if ($CopyDepsJson) {
  $sourceDeps = [System.IO.Path]::ChangeExtension($sourceFull, '.deps.json')
  if (Test-Path -LiteralPath $sourceDeps -PathType Leaf) {
    $destDeps = [System.IO.Path]::ChangeExtension($destFile, '.deps.json')
    Copy-Item -LiteralPath $sourceDeps -Destination $destDeps -Force
  }
}

Write-Host "Copied:" -ForegroundColor Green
Write-Host "  From: $sourceFull"
Write-Host "  To:   $destFile"
