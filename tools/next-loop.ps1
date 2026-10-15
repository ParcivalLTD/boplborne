# Runs /next unattended, one fresh headless Claude session per step, until a step needs you.
#
#   powershell -File tools/next-loop.ps1 -MaxSteps 2
#
# Before running: Unity Editor open on this project and idle, working tree clean.
# Stops when: a step reports NEEDS-HUMAN, Claude exits with an error, a run leaves
# uncommitted changes, or a run makes no commit. Logs go to Logs/next/ (gitignored).
# Never pushes. Package installs, git push and history rewrites are blocked per run.

param(
    [int]$MaxSteps = 3,
    [string]$Model = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$claude = (Get-Command claude -ErrorAction SilentlyContinue).Source
if (-not $claude) { $claude = Join-Path $env:USERPROFILE ".local\bin\claude.exe" }
if (-not (Test-Path $claude)) { throw "claude CLI not found. Install Claude Code or put claude on PATH." }

if (git status --porcelain) { throw "Working tree is not clean. Commit or stash first." }

$logDir = Join-Path $root "Logs\next"
New-Item -ItemType Directory -Force $logDir | Out-Null

$prompt = @'
Unattended run of the /next cycle: read .claude/skills/next/SKILL.md and do exactly what it says,
including its "Unattended runs" section. Nobody is watching, so do not ask questions.
'@

$allowed = @(
    "Read", "Glob", "Grep", "Edit", "Write",
    "Bash", "PowerShell",
    "mcp__unity"
)
$blocked = @(
    "mcp__unity__package_add", "mcp__unity__package_remove",
    "Bash(git push:*)", "Bash(git reset:*)", "Bash(git clean:*)", "Bash(git checkout:*)",
    "Bash(git rebase:*)", "Bash(git commit --amend:*)",
    "PowerShell(git push:*)", "PowerShell(git reset:*)", "PowerShell(git clean:*)", "PowerShell(git checkout:*)"
)

for ($i = 1; $i -le $MaxSteps; $i++) {
    $head = git rev-parse HEAD
    $log = Join-Path $logDir ("next-{0:yyyyMMdd-HHmmss}.txt" -f (Get-Date))
    Write-Host "=== Step run $i of $MaxSteps (log: $log) ==="

    $cliArgs = @("-p", $prompt, "--permission-mode", "acceptEdits",
        "--allowedTools") + $allowed + @("--disallowedTools") + $blocked
    if ($Model) { $cliArgs += @("--model", $Model) }

    & $claude @cliArgs 2>&1 | Tee-Object -FilePath $log
    $exit = $LASTEXITCODE

    if ($exit -ne 0) { Write-Host "Stopped: claude exited with $exit. See $log"; break }
    if (Select-String -Path $log -Pattern "NEEDS-HUMAN" -SimpleMatch -Quiet) {
        Write-Host "Stopped: this step needs you. Read the end of $log"; break
    }
    if (git status --porcelain) { Write-Host "Stopped: uncommitted changes left. See $log"; break }
    if ((git rev-parse HEAD) -eq $head) { Write-Host "Stopped: no commit was made. See $log"; break }

    Write-Host ("Committed: " + (git log --oneline -1))
}
