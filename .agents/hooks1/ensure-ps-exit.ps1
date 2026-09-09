# PreToolUse hook: ensure PowerShell commands issued by the agent always exit
# explicitly so the terminal does not hang waiting for interactive input.
#
# Reads the hook JSON from stdin. If the tool input has a `command` string that
# looks like a PowerShell invocation (powershell/pwsh/.ps1) and does not already
# contain an `exit` statement, appends "; exit $LASTEXITCODE" and returns the
# modified input via `updatedInput`. Otherwise passes through unchanged.
#
# Non-blocking on any error: we never want a hook failure to stall the agent.

$ErrorActionPreference = 'Continue'

# Read stdin outside try: $input enumerator is only reliably available in the
# script's top-level process scope, not inside a try/catch block.
$raw = $input | Out-String
if ([string]::IsNullOrWhiteSpace($raw)) { $raw = [Console]::In.ReadToEnd() }
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try {

    $data = $raw | ConvertFrom-Json
    $toolInput = $data.tool_input
    if (-not $toolInput) { exit 0 }

    $cmd = $toolInput.command
    if (-not $cmd -or $cmd -isnot [string]) { exit 0 }

    # Detect PowerShell invocation: powershell.exe / pwsh / any .ps1 file.
    $isPowerShell = ($cmd -match '(?i)\b(?:powershell(?:\.exe)?|pwsh(?:\.exe)?)\b') -or
                    ($cmd -match '(?i)\.ps1\b')
    if (-not $isPowerShell) { exit 0 }

    # Already has an explicit exit statement -> leave alone.
    if ($cmd -match '(?i)\bexit\b') { exit 0 }

    # Append exit guard. Backtick-escape $ so the target shell resolves it.
    $newCmd = $cmd.TrimEnd() + "; exit `$LASTEXITCODE"

    # Rebuild tool_input preserving all original fields, overriding only command.
    $updated = [ordered]@{}
    foreach ($prop in $toolInput.PSObject.Properties) {
        $updated[$prop.Name] = $prop.Value
    }
    $updated['command'] = $newCmd

    $output = [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName          = 'PreToolUse'
            permissionDecision     = 'allow'
            permissionDecisionReason = 'Auto-appended "; exit $LASTEXITCODE" to prevent PowerShell terminal hang.'
            updatedInput           = $updated
            additionalContext       = 'PowerShell command was auto-patched to exit explicitly so the terminal does not hang waiting for input.'
        }
    }
    $output | ConvertTo-Json -Depth 20
}
catch {
    # Never block the agent on hook errors.
    exit 0
}

exit 0