param(
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Version)) {
    $assetPath = "Assets/_Project/Data/Config/GameConfig.asset"
    $Version = "0.1.0"
    if (Test-Path $assetPath) {
        $match = Select-String -Path $assetPath -Pattern "_gameVersion:\s*(.+)"
        if ($match) {
            $Version = $match.Matches[0].Groups[1].Value.Trim().Trim('"').Trim("'")
        }
    }
}

$buildDir = "Builds/Windows/GraceRun_v$Version"
if (-not (Test-Path "$buildDir/GraceRun.exe")) {
    $found = Get-ChildItem -Path "Builds" -Filter "GraceRun.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $found) {
        throw "Build not found at '$buildDir/GraceRun.exe'. Run 'Grace Run -> Build Windows' in Unity first."
    }
    $buildDir = $found.Directory.FullName
}

$fullBuildDir = (Resolve-Path $buildDir).Path
New-Item -ItemType Directory -Force -Path "Builds/Release" | Out-Null
$releaseDir = (Resolve-Path "Builds/Release").Path
$singleExe = Join-Path $releaseDir "GraceRun_v${Version}_SingleFile.exe"
$portableZip = Join-Path $releaseDir "GraceRun_v${Version}_Portable.zip"
$payloadZip = Join-Path $env:TEMP "GraceRun_payload_${Version}.zip"

Write-Host "Compressing Unity build '$fullBuildDir'..."
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
if (Test-Path $portableZip) { Remove-Item $portableZip -Force }

Compress-Archive -Path "$fullBuildDir\*" -DestinationPath $payloadZip -CompressionLevel Optimal -Force
Copy-Item -Path $payloadZip -Destination $portableZip -Force

$launcherDir = Join-Path $env:TEMP "GraceRunSingleExeStub"
if (Test-Path $launcherDir) { Remove-Item $launcherDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $launcherDir | Out-Null
Copy-Item -Path $payloadZip -Destination (Join-Path $launcherDir "payload.zip") -Force

$csproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>false</SelfContained>
    <AssemblyName>GraceRun_SingleFile</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <EmbeddedResource Include="payload.zip" LogicalName="payload.zip" />
  </ItemGroup>
</Project>
"@

$programCs = @"
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

class Program
{
    static int Main(string[] args)
    {
        string version = "$Version";
        string extractRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GraceRun",
            "Runtime_" + version);
        string exePath = Path.Combine(extractRoot, "GraceRun.exe");
        string stampPath = Path.Combine(extractRoot, ".build_stamp");

        var asm = Assembly.GetExecutingAssembly();
        string currentStamp = asm.ManifestModule.ModuleVersionId.ToString();

        bool needsExtract = !File.Exists(exePath) ||
                            !File.Exists(stampPath) ||
                            File.ReadAllText(stampPath).Trim() != currentStamp;

        if (needsExtract)
        {
            try
            {
                if (Directory.Exists(extractRoot))
                    Directory.Delete(extractRoot, true);
            }
            catch { }

            Directory.CreateDirectory(extractRoot);
            using Stream stream = asm.GetManifestResourceStream("payload.zip");
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            archive.ExtractToDirectory(extractRoot, true);
            File.WriteAllText(stampPath, currentStamp);
        }

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = extractRoot,
            UseShellExecute = false
        };
        foreach (var a in args)
            psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi);
        return 0;
    }
}
"@

Set-Content -Path (Join-Path $launcherDir "GraceRunLauncher.csproj") -Value $csproj -Encoding UTF8
Set-Content -Path (Join-Path $launcherDir "Program.cs") -Value $programCs -Encoding UTF8

Write-Host "Building single-file .exe wrapper..."
$pubOut = Join-Path $launcherDir "out"
dotnet publish (Join-Path $launcherDir "GraceRunLauncher.csproj") -c Release -r win-x64 -o $pubOut --nologo -v quiet
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Copy-Item -Path (Join-Path $pubOut "GraceRun_SingleFile.exe") -Destination $singleExe -Force
Remove-Item $payloadZip -Force -ErrorAction SilentlyContinue
Remove-Item $launcherDir -Recurse -Force -ErrorAction SilentlyContinue

$sizeMB = [math]::Round((Get-Item $singleExe).Length / 1MB, 2)
Write-Host "Created single-file executable: $singleExe ($sizeMB MB)"
Write-Host "Created portable archive:       $portableZip"
