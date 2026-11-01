$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$settingsType=$assembly.GetType('Brough.Starfield.Settings')
$sample=$assembly.GetType('Brough.Starfield.FlightMath').GetMethod('Sample')
$options=[Activator]::CreateInstance($settingsType)
$names=@('Engagement','CruiseVariation','CourseCorrections','NebulaPassages','BridgeDisplay','RedAlert','AlertSound')
foreach($name in $names){if($options.$name){throw "Effect defaults on: $name"}}
foreach($time in @(0.0,3.0,10.0,50.0,124.0,400.0)){
    $frame=$sample.Invoke($null,@($options,$time,10.0,$false))
    if($frame.Warp -ne $options.WarpFactor -or $frame.ExtraStretch -ne 0 -or $frame.OffsetX -ne 0 -or $frame.OffsetY -ne 0 -or $frame.Nebula -ne 0 -or $frame.Alert -ne 0){throw 'Disabled effects alter the field'}
}
$options.Engagement=$true
$start=$sample.Invoke($null,@($options,0.0,0.0,$false));$engaged=$sample.Invoke($null,@($options,3.0,3.0,$false))
if($start.Warp -ne 1 -or $engaged.Warp -ne $options.WarpFactor -or $engaged.ExtraStretch -le 0){throw 'Engagement ramp or stretch failed'}
$options.Engagement=$false;$options.CruiseVariation=$true
$slow=$sample.Invoke($null,@($options,90.0,90.0,$false));$fast=$sample.Invoke($null,@($options,180.0,180.0,$false))
if($slow.Warp -ge $fast.Warp -or $fast.Warp -gt $options.WarpFactor){throw 'Cruise variation failed'}
$options.CruiseVariation=$false;$options.CourseCorrections=$true
if($sample.Invoke($null,@($options,50.0,50.0,$false)).OffsetX -eq 0){throw 'Course correction missing'}
$options.CourseCorrections=$false;$options.NebulaPassages=$true
if($sample.Invoke($null,@($options,125.0,125.0,$false)).Nebula -lt .99){throw 'Nebula passage missing'}
$options.NebulaPassages=$false;$options.RedAlert=$true
if($sample.Invoke($null,@($options,124.0,124.0,$false)).Alert -lt .99){throw 'Red alert missing'}
foreach($name in $names){$options.$name=$true};$options.AlertSound=$false
foreach($index in 0..1800){
    $time=$index/3.0;$frame=$sample.Invoke($null,@($options,$time,$time,$false))
    if($frame.Warp -lt 1 -or $frame.Warp -gt 9.99001 -or [single]::IsNaN($frame.Warp) -or [Math]::Abs($frame.OffsetX) -gt .061 -or [Math]::Abs($frame.OffsetY) -gt .026 -or $frame.Nebula -lt 0 -or $frame.Nebula -gt 1 -or $frame.Alert -lt 0 -or $frame.Alert -gt 1){throw 'Effect bounds failed'}
}
# Actual renderer: save all effects together and each independent visual effect.
$fieldType=$assembly.GetType('Brough.Starfield.Field')
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
foreach($effect in @('All','Engagement','CruiseVariation','CourseCorrections','NebulaPassages','BridgeDisplay','RedAlert')){
    $settings=[Activator]::CreateInstance($settingsType);$settings.Stars=1000
    foreach($name in $names){$settings.$name=($effect -eq 'All' -or $effect -eq $name)};$settings.AlertSound=$false
    $field=[Activator]::CreateInstance($fieldType,@($settings));$field.Size=[Drawing.Size]::new(960,600)
    $bitmap=[Drawing.Bitmap]::new(960,600);$graphics=[Drawing.Graphics]::FromImage($bitmap)
    try{
        $field.PreviewEffects()
        $render=$fieldType.GetMethod('RenderFrame',$flags)
        $render.Invoke($field,@($graphics,0.0));$render.Invoke($field,@($graphics,10.0))
        $bitmap.Save((Join-Path $PSScriptRoot "dist\effects-$effect.png"))
    }finally{$graphics.Dispose();$bitmap.Dispose();$field.Dispose()}
}
# UI bindings, preview action and settings persistence.
$window=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'),@($options))
function Find-Controls($control){foreach($child in $control.Controls){$child;Find-Controls $child}}
try{
    $window.Show()
    $controls=@(Find-Controls $window)
    $field=@($controls | Where-Object {$_.GetType().Name -eq 'Field'})[0]
    $fieldType.GetField('timer',$flags).GetValue($field).Stop()
    $labels=@('Warp engagement','Vary cruising speed','Course corrections','Nebula passages','Bridge display','Red alert','Alert sound (optional)')
    for($index=0;$index -lt $labels.Count;$index++){
        $box=@($controls | Where-Object {$_ -is [Windows.Forms.CheckBox] -and $_.Text -eq $labels[$index]})[0]
        if($null -eq $box){throw "Missing option: $($labels[$index])"};$box.Checked=$false
        if($options.($names[$index])){throw 'Checkbox did not update settings'}
    }
    $button=@($controls | Where-Object {$_ -is [Windows.Forms.Button] -and $_.Text -eq 'Preview selected effects'})[0]
    $button.PerformClick()
    $field=@($controls | Where-Object {$_.GetType().Name -eq 'Field'})[0]
    if($fieldType.GetField('demoStart',$flags).GetValue($field) -lt 0){throw 'Preview action not connected'}
}finally{$window.Dispose()}
foreach($name in $names){$options.$name=$true}
$serializer=[Xml.Serialization.XmlSerializer]::new($settingsType);$writer=[IO.StringWriter]::new();$serializer.Serialize($writer,$options)
$reader=[IO.StringReader]::new($writer.ToString())
try{$loaded=$serializer.Deserialize($reader);foreach($name in $names){if(!$loaded.$name){throw "Effect not persisted: $name"}}}finally{$writer.Dispose();$reader.Dispose()}
$reader=[IO.StringReader]::new('<Settings><Speed>30</Speed><Fps>50</Fps></Settings>')
try{$loaded=$serializer.Deserialize($reader);foreach($name in $names){if($loaded.$name){throw 'Old settings enabled an effect'}}}finally{$reader.Dispose()}
# Validate the original sound resource without playing it.
$painter=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightPainter'),$true)
try{
    $painter.GetType().GetMethod('CreateChime',$flags).Invoke($painter,@())
    $wave=$painter.GetType().GetField('wave',$flags).GetValue($painter).ToArray()
    if([Text.Encoding]::ASCII.GetString($wave,0,4) -ne 'RIFF' -or $wave.Length -ne 26504){throw 'Synthesised chime WAV invalid'}
}finally{$painter.Dispose()}
Write-Output 'Independent effects, bounds, actual rendering, preview, settings compatibility and chime format checks passed.'
