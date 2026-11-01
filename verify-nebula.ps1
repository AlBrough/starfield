$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$type=$assembly.GetType('Brough.Starfield.FlightPainter');$flags=[Reflection.BindingFlags]'Instance,NonPublic,Public'
$painter=[Activator]::CreateInstance($type,$true)
$frame=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightFrame'));$frame.Nebula=[single]0.8
$bitmap=[Drawing.Bitmap]::new(1920,1080);$graphics=[Drawing.Graphics]::FromImage($bitmap)
try{
    $method=$type.GetMethod('Background')
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(1920,1080),$frame,100.0))
    $cache=$type.GetField('nebulaCache',$flags).GetValue($painter)
    if($cache.Width -gt 640 -or $cache.Height -gt 640){throw 'Nebula cache exceeds size bound'}
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(1920,1080),$frame,100.01))
    if($type.GetField('cachedAt',$flags).GetValue($painter) -ne 100){throw 'Nebula recomposed unnecessarily'}
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(1920,1080),$frame,100.1))
    if($type.GetField('cachedAt',$flags).GetValue($painter) -ne 100.1){throw 'Nebula cache did not refresh'}
    $painter.BackgroundColour=[Drawing.Color]::FromArgb(20,40,60)
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(1920,1080),$frame,100.11))
    if($type.GetField('cachedColour',$flags).GetValue($painter) -ne $painter.BackgroundColour.ToArgb()){throw 'Background colour change ignored'}
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(1920,1080),$frame,2.0))
    if($type.GetField('cachedAt',$flags).GetValue($painter) -ne 2){throw 'Preview time reset ignored'}
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(600,600),$frame,3.0))
    $cache=$type.GetField('nebulaCache',$flags).GetValue($painter)
    if($cache.Size -ne [Drawing.Size]::new(600,600)){throw 'Resize did not rebuild nebula cache'}
    $frame.Nebula=[single]0
    $graphics.Clear([Drawing.Color]::Magenta)
    $method.Invoke($painter,@($graphics,[Drawing.Size]::new(1920,1080),$frame,4.0))
    if($bitmap.GetPixel(100,100).ToArgb() -ne [Drawing.Color]::Magenta.ToArgb()){throw 'Disabled nebula still paints'}
}finally{$graphics.Dispose();$bitmap.Dispose();$painter.Dispose()}
# Older memorial preferences are ignored without losing the rest of the saved settings.
$settingsType=$assembly.GetType('Brough.Starfield.Settings')
$serializer=[Xml.Serialization.XmlSerializer]::new($settingsType)
$reader=[IO.StringReader]::new('<Settings><PatrickTribute>true</PatrickTribute><BridgeDisplay>true</BridgeDisplay><Fps>30</Fps><DensityMinutes>5</DensityMinutes></Settings>')
try{$settings=$serializer.Deserialize($reader);if(!$settings.BridgeDisplay -or $settings.Fps -ne 30 -or $settings.DensityMinutes -ne 5){throw 'Old settings were lost'}}finally{$reader.Dispose()}
if($null -ne $settingsType.GetField('PatrickTribute')){throw 'Memorial option still present'}
$window=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'),@($settings))
function Find-Controls($control){foreach($child in $control.Controls){$child;Find-Controls $child}}
try{if(@(Find-Controls $window | Where-Object {$_.Text -eq 'Remember Sir Patrick Stewart'}).Count -ne 0){throw 'Memorial control still visible'}}finally{$window.Dispose()}
Write-Output 'Nebula cache bounds, reuse, refresh, resize, background colour, disable state and removed-option compatibility passed.'
