$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$settingsType=$assembly.GetType('Brough.Starfield.Settings')
$settings=[Activator]::CreateInstance($settingsType)
$memorial=$assembly.GetType('Brough.Starfield.NavigationMath').GetMethod('Stardate').Invoke($null,@([DateTime]::new(2026,10,28)))
if($memorial.ToString('00000.000',[Globalization.CultureInfo]::InvariantCulture) -ne '41821.918'){throw 'Remembrance date mapping failed'}
$window=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'),@($settings))
function Find-Controls($control){foreach($child in $control.Controls){$child;Find-Controls $child}}
try{
    $controls=@(Find-Controls $window)
    $bridge=@($controls | Where-Object {$_ -is [Windows.Forms.CheckBox] -and $_.Text -eq 'Bridge display'})[0]
    $tribute=@($controls | Where-Object {$_ -is [Windows.Forms.CheckBox] -and $_.Text -eq 'Remember Sir Patrick Stewart'})[0]
    if($tribute.Checked -or $tribute.Enabled){throw 'Tribute should be opt-in and require bridge display'}
    $bridge.Checked=$true
    if(!$tribute.Enabled){throw 'Tribute option did not enable'}
    $tribute.Checked=$true
    if(!$settings.PatrickTribute){throw 'Tribute binding failed'}
}finally{$window.Dispose()}
$serializer=[Xml.Serialization.XmlSerializer]::new($settingsType);$writer=[IO.StringWriter]::new();$serializer.Serialize($writer,$settings)
$reader=[IO.StringReader]::new($writer.ToString())
try{$loaded=$serializer.Deserialize($reader);if(!$loaded.PatrickTribute){throw 'Tribute preference not persisted'}}finally{$reader.Dispose();$writer.Dispose()}
$reader=[IO.StringReader]::new('<Settings><BridgeDisplay>true</BridgeDisplay></Settings>')
try{if($serializer.Deserialize($reader).PatrickTribute){throw 'Old settings opted into tribute'}}finally{$reader.Dispose()}
$painter=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightPainter'),$true)
$bitmap=[Drawing.Bitmap]::new(960,600);$graphics=[Drawing.Graphics]::FromImage($bitmap)
$frame=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightFrame'));$frame.Warp=[single]9
try{
    $graphics.Clear([Drawing.Color]::Black)
    $painter.GetType().GetMethod('Foreground').Invoke($painter,@($graphics,[Drawing.Size]::new(960,600),$settings,$frame,10.0))
    $bitmap.Save((Join-Path $PSScriptRoot 'dist\tribute-preview.png'))
}finally{$graphics.Dispose();$bitmap.Dispose();$painter.Dispose()}
Write-Output 'Remembrance date, opt-in UI, bridge dependency, XML compatibility and tribute rendering passed.'
