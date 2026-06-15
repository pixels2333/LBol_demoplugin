# Auto-deploy hook for NetworkPlugin
# Runs at session end to copy the built DLL to the mod directory

param(
    [string]$WorkspacePath = "d:\programme\LBol_demoplugin"
)

$ErrorActionPreference = "Continue"

try {
    $scriptPath = Join-Path $WorkspacePath "copy_networkplugin_dll.ps1"
    
    if (Test-Path $scriptPath) {
        Write-Host "🔄 Hook: Running copy_networkplugin_dll.ps1..." -ForegroundColor Cyan
        
        # Invoke the script with default parameters
        & $scriptPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Hook: NetworkPlugin deployed successfully" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Hook: Deployment completed with exit code $LASTEXITCODE" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️  Hook: Script not found at $scriptPath" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️  Hook: Error during deployment - $($_.Exception.Message)" -ForegroundColor Yellow
}

exit 0  # Non-blocking
