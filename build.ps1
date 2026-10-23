$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
Set-Location $root
$build = Join-Path $root 'build'
$stage = Join-Path $build 'runtime-stage'
$runtimeZip = Join-Path $build 'runtime.zip'
$csc = Join-Path $root '.cache\roslyn\tools\csc.exe'
$vlcSource = Join-Path $root '.cache\vlc\vlc-3.0.21'
$dwebp = Join-Path $root 'assets\tools\dwebp.exe'
$framework = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'

foreach ($required in @($csc, (Join-Path $vlcSource 'libvlc.dll'), $dwebp, (Join-Path $root 'assets\app.ico'))) {
  if (!(Test-Path $required)) { throw "Missing build dependency: $required" }
}

New-Item -ItemType Directory -Force $build | Out-Null
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force (Join-Path $stage 'vlc'), (Join-Path $stage 'tools') | Out-Null

Write-Host 'Staging VLC runtime...'
Copy-Item (Join-Path $vlcSource 'libvlc.dll') (Join-Path $stage 'vlc\libvlc.dll')
Copy-Item (Join-Path $vlcSource 'libvlccore.dll') (Join-Path $stage 'vlc\libvlccore.dll')
Copy-Item (Join-Path $vlcSource 'COPYING.txt') (Join-Path $stage 'vlc\COPYING.txt')
robocopy (Join-Path $vlcSource 'plugins') (Join-Path $stage 'vlc\plugins') /E /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy plugins failed: $LASTEXITCODE" }
Copy-Item $dwebp (Join-Path $stage 'tools\dwebp.exe')

Write-Host 'Creating embedded runtime zip...'
if (Test-Path $runtimeZip) { Remove-Item $runtimeZip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($stage, $runtimeZip, [IO.Compression.CompressionLevel]::Optimal, $false)
Write-Host ("Runtime zip: {0:N1} MB" -f ((Get-Item $runtimeZip).Length / 1MB))

$refs = @(
  'System.dll',
  'System.Core.dll',
  'System.Windows.Forms.dll',
  'System.Drawing.dll',
  'System.Web.Extensions.dll',
  'System.IO.Compression.dll',
  'System.IO.Compression.FileSystem.dll'
) | ForEach-Object { '/r:' + (Join-Path $framework $_) }
$sources = Get-ChildItem (Join-Path $root 'src\*.cs') | ForEach-Object { $_.FullName }

Write-Host 'Compiling DeskFlow.exe...'
& $csc /nologo /target:winexe /platform:x64 /optimize+ /langversion:7.3 /codepage:65001 `
  /out:build\DeskFlow.exe /win32manifest:src\app.manifest /win32icon:assets\app.ico `
  /resource:assets\app.ico,DeskFlow.AppIcon.ico `
  /resource:LICENSE,DeskFlow.LICENSE.txt `
  /resource:THIRD-PARTY-NOTICES.txt,DeskFlow.THIRD-PARTY-NOTICES.txt `
  /resource:build\runtime.zip,DeskFlow.Payload.runtime.zip `
  @refs @sources
if ($LASTEXITCODE -ne 0) { throw "csc failed: $LASTEXITCODE" }

$exe = Join-Path $build 'DeskFlow.exe'
Write-Host ("DeskFlow.exe: {0:N1} MB" -f ((Get-Item $exe).Length / 1MB))

Write-Host 'Creating portable release package...'
$release = Join-Path $build 'release'
if (Test-Path $release) { Remove-Item $release -Recurse -Force }
New-Item -ItemType Directory -Force $release | Out-Null
Copy-Item $exe (Join-Path $release 'DeskFlow.exe')
Copy-Item (Join-Path $root 'README.md') (Join-Path $release 'README.md')
Copy-Item (Join-Path $root 'LICENSE') (Join-Path $release 'LICENSE')
Copy-Item (Join-Path $root 'THIRD-PARTY-NOTICES.txt') (Join-Path $release 'THIRD-PARTY-NOTICES.txt')
New-Item -ItemType Directory -Force (Join-Path $release 'docs') | Out-Null
Copy-Item (Join-Path $root 'docs\screenshot.jpg') (Join-Path $release 'docs\screenshot.jpg')
Copy-Item (Join-Path $root 'docs\screenshot-video.jpg') (Join-Path $release 'docs\screenshot-video.jpg')
Copy-Item (Join-Path $root 'docs\screenshot-settings.jpg') (Join-Path $release 'docs\screenshot-settings.jpg')

$portableZip = Join-Path $build 'DeskFlow-1.0.5-win64.zip'
if (Test-Path $portableZip) { Remove-Item $portableZip -Force }
[IO.Compression.ZipFile]::CreateFromDirectory($release, $portableZip, [IO.Compression.CompressionLevel]::Optimal, $false)
Write-Host ("Portable zip: {0:N1} MB" -f ((Get-Item $portableZip).Length / 1MB))
Get-FileHash $exe -Algorithm SHA256 | Format-List
Get-FileHash $portableZip -Algorithm SHA256 | Format-List
