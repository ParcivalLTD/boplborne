# Renders tools/audio/voice_lines.tsv (name <TAB> text) to Assets/_Project/Audio/Voice/<name>.wav with the
# Windows offline voice "Microsoft David". Edit the lines here AND in Resources/StoryData.asset, then run
# this and Grace Run -> Build Voice Over. Usage: powershell -File tools/audio/make_voice.ps1
param([string]$Voice = 'Microsoft David Desktop', [int]$Rate = -2)

Add-Type -AssemblyName System.Speech
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out = Join-Path $root 'Assets/_Project/Audio/Voice'
New-Item -ItemType Directory -Force $out | Out-Null

$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.SelectVoice($Voice)
$synth.Rate = $Rate
$format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)

foreach ($line in Get-Content (Join-Path $PSScriptRoot 'voice_lines.tsv') -Encoding UTF8) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $name, $text = $line -split "`t", 2
    $path = Join-Path $out "$name.wav"
    $synth.SetOutputToWaveFile($path, $format)
    $synth.Speak($text)
    $synth.SetOutputToNull()
    Write-Host "$name -> $path"
}
