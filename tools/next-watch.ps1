# Follows the newest unattended /next run (started by tools/next-loop.ps1) live:
# prints each tool call and each piece of text Claude writes. Ctrl+C stops watching only.
#
#   powershell -ExecutionPolicy Bypass -File tools/next-watch.ps1
#
# -Session <file.jsonl> follows a specific transcript instead.

param([string]$Session = "")

$projectDir = Join-Path $env:USERPROFILE ".claude\projects\R--dev-boplborne"
$marker = "Unattended run of the /next cycle"

if (-not $Session) {
    $Session = Get-ChildItem $projectDir -Filter *.jsonl |
        Sort-Object LastWriteTime -Descending |
        Where-Object {
            # Only the run's first user message counts; other sessions may quote the marker later.
            $first = Get-Content $_.FullName -TotalCount 30 -Encoding UTF8 |
                Where-Object { $_ -like '*"type":"user"*' } | Select-Object -First 1
            $first -and $first.Contains("""content"":""$marker")
        } |
        Select-Object -First 1 -ExpandProperty FullName
}
if (-not $Session) { Write-Host "No unattended run found in $projectDir yet."; exit 1 }
Write-Host "Watching $Session`n"

function Short([string]$text, [int]$max = 160) {
    $line = ($text -replace "\s+", " ").Trim()
    if ($line.Length -gt $max) { $line.Substring(0, $max) + "..." } else { $line }
}

Get-Content $Session -Wait -Encoding UTF8 | ForEach-Object {
    try { $entry = $_ | ConvertFrom-Json } catch { return }
    if ($entry.type -ne "assistant" -or -not $entry.message.content) { return }
    $time = if ($entry.timestamp) { ([datetime]$entry.timestamp).ToLocalTime().ToString("HH:mm:ss") } else { "" }

    foreach ($part in $entry.message.content) {
        switch ($part.type) {
            "text" { if ($part.text.Trim()) { Write-Host "$time  $(Short $part.text 300)" -ForegroundColor White } }
            "tool_use" {
                $toolInput = $part.input
                $detail = if ($toolInput.description) { $toolInput.description }
                    elseif ($toolInput.file_path) { $toolInput.file_path }
                    elseif ($toolInput.command) { $toolInput.command }
                    elseif ($toolInput.code) { $toolInput.code }
                    elseif ($toolInput.path) { $toolInput.path }
                    else { "" }
                Write-Host "$time  > $($part.name)  $(Short $detail)" -ForegroundColor DarkCyan
            }
        }
    }
}
