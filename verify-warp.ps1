$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$warp=$assembly.GetType('Brough.Starfield.WarpMath')
$speed=$warp.GetMethod('VisualSpeed'); $convert=$warp.GetMethod('FromLegacySpeed')
$previous=0.0
foreach($step in 100..999){
    $value=$speed.Invoke($null,@([single]($step/100.0)))
    if([double]::IsInfinity($value) -or [double]::IsNaN($value) -or $value -le $previous){throw 'Warp speed is not finite and increasing'}
    $previous=$value
}
foreach($legacy in @(1,30,70,90,150,200)){
    $factor=$convert.Invoke($null,@($legacy));$actual=$speed.Invoke($null,@($factor))
    if([Math]::Abs($actual-$legacy) -gt 0.01){throw "Legacy speed $legacy not preserved"}
}
$type=$assembly.GetType('Brough.Starfield.Settings')
$serializer=[Xml.Serialization.XmlSerializer]::new($type)
$reader=[IO.StringReader]::new('<Settings><Speed>30</Speed><Fps>50</Fps></Settings>')
try{$settings=$serializer.Deserialize($reader)}finally{$reader.Dispose()}
if([Math]::Abs($speed.Invoke($null,@($settings.WarpFactor))-30) -gt 0.01 -or $settings.Fps -ne 50){throw 'Old XML migration failed'}
$window=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'),@($settings))
function Find-Controls($control){foreach($child in $control.Controls){$child;Find-Controls $child}}
try{
    $input=@(Find-Controls $window | Where-Object {$_.GetType().Name -eq 'NumberInput' -and $_.AccessibleName -eq 'Warp factor'})[0]
    $input.Value=9.99
    if([Math]::Abs($settings.WarpFactor-9.99) -gt .00001){throw 'Warp control binding failed'}
    $input.Value=10
    if($input.Value -ne 9.99){throw 'Warp 10 is selectable'}
}finally{$window.Dispose()}
$writer=[IO.StringWriter]::new();$serializer.Serialize($writer,$settings)
$reader=[IO.StringReader]::new($writer.ToString())
try{$loaded=$serializer.Deserialize($reader);if([Math]::Abs($loaded.WarpFactor-9.99) -gt .00001){throw 'Warp round-trip failed'}}finally{$reader.Dispose();$writer.Dispose()}
$settings.Stars=100
$field=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Field'),@($settings))
$field.Size=[Drawing.Size]::new(800,600)
$bitmap=[Drawing.Bitmap]::new(800,600);$graphics=[Drawing.Graphics]::FromImage($bitmap)
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$render=$field.GetType().GetMethod('RenderFrame',$flags)
$storage=$field.GetType().GetField('stars',$flags)
try{
    $render.Invoke($field,@($graphics,0.0))
    $stars=$storage.GetValue($field)
    foreach($factor in @(1.0,9.0,9.99)){
        $settings.WarpFactor=[single]$factor
        $star=$stars.GetValue(0);$star.X=[single]0.01;$star.Y=[single]0.01;$star.Z=[single]0.8;$stars.SetValue($star,0)
        $field.GetType().GetField('last',$flags).SetValue($field,1.0)
        $render.Invoke($field,@($graphics,1.02))
        $expected=0.8-0.02*0.0025*$speed.Invoke($null,@([single]$factor))
        if([Math]::Abs($stars.GetValue(0).Z-$expected) -gt .000001){throw 'Renderer does not use selected warp factor'}
    }
}finally{$graphics.Dispose();$bitmap.Dispose();$field.Dispose()}
Write-Output 'Warp controls, monotonic finite speed, old speed migration and settings round-trip passed.'
