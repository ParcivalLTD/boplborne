<#
.SYNOPSIS
    Runs a Boplborne dedicated server and starts it again whenever it exits.

.DESCRIPTION
    Starts Boplborne.exe headless (-batchmode -nographics -server), writes one log file per run
    and a status.json that a monitor can read, and restarts the server after a crash or a quit.
    A server that dies within seconds, over and over, backs off instead of spinning.
    Stop it with Ctrl+C (the running server is stopped too).
    See docs/SERVER.md for hosting, port forwarding and running it at startup.

.EXAMPLE
    .\tools\run-server.ps1 -Exe C:\Boplborne\Boplborne.exe -Port 7777
#>
param(
    [string]$Exe = (Join-Path $PSScriptRoot "..\Boplborne.exe"),
    [int]$Port = 7777,
    [string]$LogDir = (Join-Path $PSScriptRoot "..\server-logs"),
    [int]$RestartDelaySeconds = 5,
    [int]$CrashLoopSeconds = 20,
    [int]$CrashLoopLimit = 5,
    [int]$CrashLoopPauseSeconds = 120,
    [int]$KeepLogs = 20
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $Exe)) { throw "Boplborne.exe not found at '$Exe'. Pass -Exe <path>." }
$Exe = (Resolve-Path -LiteralPath $Exe).Path
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$LogDir = (Resolve-Path -LiteralPath $LogDir).Path
$supervisorLog = Join-Path $LogDir "supervisor.log"
$statusFile = Join-Path $LogDir "status.json"

function Write-Supervisor([string]$message) {
    $line = "{0:yyyy-MM-dd HH:mm:ss} {1}" -f (Get-Date), $message
    Write-Host $line
    Add-Content -LiteralPath $supervisorLog -Value $line
}

function Remove-OldLogs {
    Get-ChildItem -LiteralPath $LogDir -Filter "server-*.log" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -Skip $KeepLogs |
        Remove-Item -Force -ErrorAction SilentlyContinue
}

$fastExits = 0
$process = $null
try {
    while ($true) {
        $logFile = Join-Path $LogDir ("server-{0:yyyyMMdd-HHmmss}.log" -f (Get-Date))
        $serverArgs = @("-batchmode", "-nographics", "-server", "-port", $Port, "-logFile", "`"$logFile`"", "-statusfile", "`"$statusFile`"")
        $started = Get-Date
        $process = Start-Process -FilePath $Exe -ArgumentList $serverArgs -PassThru
        Write-Supervisor "Started server pid $($process.Id) on port $Port, log $logFile"

        $process.WaitForExit()
        $ranSeconds = [int]((Get-Date) - $started).TotalSeconds
        Write-Supervisor "Server exited with code $($process.ExitCode) after ${ranSeconds}s"
        $process = $null

        if ($ranSeconds -lt $CrashLoopSeconds) { $fastExits++ } else { $fastExits = 0 }
        Remove-OldLogs

        if ($fastExits -ge $CrashLoopLimit) {
            Write-Supervisor "Exited $fastExits times in a row within ${CrashLoopSeconds}s; waiting ${CrashLoopPauseSeconds}s before trying again."
            Start-Sleep -Seconds $CrashLoopPauseSeconds
            $fastExits = 0
        }
        else {
            Start-Sleep -Seconds $RestartDelaySeconds
        }
    }
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Write-Supervisor "Stopping server pid $($process.Id)"
        Stop-Process -Id $process.Id -Force
    }
}
