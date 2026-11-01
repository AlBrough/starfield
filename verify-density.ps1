$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly = [Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$density = $assembly.GetType('Brough.Starfield.DensityMath')
$fraction = $density.GetMethod('Fraction')
$opacity = $density.GetMethod('Opacity')
foreach ($minutes in @(10,12,15)) {
    $period = $minutes*60
    foreach ($sample in @(@(0,1.0),@(($period/2),0.3),@($period,1.0),@(($period*100),1.0))) {
        $actual = $fraction.Invoke($null,@([double]$sample[0],$minutes,$true))
        if ([Math]::Abs($actual-$sample[1]) -gt 0.000001) { throw "Cycle endpoint failed at $minutes minutes" }
    }
    $previous = 1.0
    foreach ($second in 1..$period) {
        $value = $fraction.Invoke($null,@([double]$second,$minutes,$true))
        if ($value -lt 0.299999 -or $value -gt 1.000001 -or [Math]::Abs($value-$previous) -gt 0.004) { throw 'Cycle bounds or continuity failed' }
        $previous = $value
    }
}
if ($fraction.Invoke($null,@(360.0,12,$false)) -ne 1) { throw 'Disabled cycle changes density' }
foreach ($level in @(0.3,0.65,1.0)) {
    $sum = 0.0
    foreach ($index in 0..999) {
        $value = $opacity.Invoke($null,@($index,1000,$level))
        if ($value -lt 0 -or $value -gt 1) { throw 'Invalid star opacity' }
        $sum += $value
    }
    if ([Math]::Abs($sum/1000-$level) -gt 0.02) { throw 'Visible density does not track the cycle' }
}
$settings = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Settings'))
$settings.Stars=1000
$field = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Field'),@($settings))
$field.Size = [Drawing.Size]::new(800,600)
$bitmap = [Drawing.Bitmap]::new(800,600)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$render = $field.GetType().GetMethod('RenderFrame',$flags)
$storage = $field.GetType().GetField('stars',$flags)
$clock = $field.GetType().GetField('last',$flags)
try {
    $render.Invoke($field,@($graphics,0.0))
    $stars = $storage.GetValue($field)
    # Freeze motion and use a synthetic clock to exercise the real renderer at both extremes.
    $settings.Speed=0
    $settings.Motion=$false
    $energies = @{}
    foreach ($sample in @(@(0.0,'dense'),@(360.0,'sparse'),@(720.0,'returned'))) {
        $clock.SetValue($field,0.0)
        $render.Invoke($field,@($graphics,[double]$sample[0]))
        if (![Object]::ReferenceEquals($stars,$storage.GetValue($field))) { throw 'Cycle replaced the star array' }
        $bitmap.Save((Join-Path $PSScriptRoot "dist\density-$($sample[1]).png"))
        $energy = 0L
        for($y=0;$y -lt 600;$y+=2){for($x=0;$x -lt 800;$x+=2){$pixel=$bitmap.GetPixel($x,$y);$energy+=$pixel.R+$pixel.G+$pixel.B}}
        $energies[$sample[1]]=$energy
    }
    if ($energies['dense'] -le 0 -or $energies['sparse'] -ge $energies['dense']*0.6 -or $energies['returned'] -lt $energies['dense']*0.9) {throw 'Rendered image did not thin and return'}
} finally { $graphics.Dispose();$bitmap.Dispose();$field.Dispose() }
$window = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'),@($settings))
function Find-Controls($control) {foreach($child in $control.Controls){$child;Find-Controls $child}}
try {
    $controls = @(Find-Controls $window)
    $toggle = @($controls | Where-Object { $_ -is [Windows.Forms.CheckBox] -and $_.Text -eq 'Vary star density' })[0]
    $toggle.Checked=$false
    if ($settings.DensityCycle) {throw 'Cycle toggle binding failed'}
    $length = @($controls | Where-Object { $_.GetType().Name -eq 'NumberInput' -and $_.AccessibleName -eq 'Cycle length (minutes)' })[0]
    $length.Value=15
    if ($settings.DensityMinutes -ne 15) {throw 'Cycle length binding failed'}
} finally {$window.Dispose()}
$serializer = [Xml.Serialization.XmlSerializer]::new($settings.GetType())
$writer = [IO.StringWriter]::new(); $serializer.Serialize($writer,$settings)
$reader = [IO.StringReader]::new($writer.ToString())
try {$loaded=$serializer.Deserialize($reader);if($loaded.DensityCycle -or $loaded.DensityMinutes -ne 15){throw 'Cycle settings round-trip failed'}}finally{$reader.Dispose();$writer.Dispose()}
$reader = [IO.StringReader]::new('<Settings><Stars>10000</Stars><Fps>30</Fps></Settings>')
try {$loaded=$serializer.Deserialize($reader);if(!$loaded.DensityCycle -or $loaded.DensityMinutes -ne 12 -or $loaded.Fps -ne 30){throw 'Old settings compatibility failed'}}finally{$reader.Dispose()}
Write-Output 'Density cycle, fade bounds, real renderer, controls and settings compatibility checks passed.'
