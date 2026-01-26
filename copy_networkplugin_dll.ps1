# Copies NetworkPlugin.dll to the target folder (overwrite if different).
# Usage: powershell -ExecutionPolicy Bypass -File .\copy_networkplugin_dll.ps1

[CmdletBinding()]
param(
  # Source DLL path. If omitted, defaults to a path relative to this script.
  [string]$SourceDll,

  # Default destination: the folder you specified.
  [string]$DestDir = "D:\steam\steamapps\workshop\content\1140150\3483706823\BepInEx\plugins"
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

if ([string]::IsNullOrWhiteSpace($SourceDll)) {
  $SourceDll = Join-Path -Path $scriptRoot -ChildPath "networkplugin\bin\Debug\netstandard2.1\NetworkPlugin.dll"
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
  exit 0
}

Copy-Item -LiteralPath $sourceFull -Destination $destFile -Force
Write-Host "Copied:" -ForegroundColor Green
Write-Host "  From: $sourceFull"
Write-Host "  To:   $destFile"
