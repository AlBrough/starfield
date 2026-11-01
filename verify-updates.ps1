param([switch]$Live)
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$output=Join-Path $PSScriptRoot 'dist\UpdateChecks.exe'
$application=Join-Path $PSScriptRoot 'dist\Starfield.scr'
Copy-Item -LiteralPath $application -Destination (Join-Path $PSScriptRoot 'dist\Starfield.dll') -Force
& $compiler /nologo /target:exe /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Net.Http.dll /reference:System.Runtime.Serialization.dll /reference:$application /out:$output (Join-Path $PSScriptRoot 'tests\UpdateChecks.cs')
if($LASTEXITCODE -ne 0){throw 'Update checks did not compile'}
if($Live){& $output --live}else{& $output}
if($LASTEXITCODE -ne 0){throw 'Update checks failed'}
