$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly = [Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$math = $assembly.GetType('Brough.Starfield.MotionMath')
$tail = $math.GetMethod('TailDepth')
$advance = $math.GetMethod('Advance')
foreach ($fps in @(30,50,60,90,120,144)) {
    $depth = [single]0.8
    foreach ($frame in 1..$fps) { $depth = $advance.Invoke($null,@($depth,30,[double](1.0/$fps))) }
    if ([Math]::Abs($depth-0.725) -gt 0.00001) { throw "Motion speed differs at $fps FPS" }
    $trail = $tail.Invoke($null,@([single]0.5,30,[single]1.5)) - 0.5
    if ([Math]::Abs($trail-0.001875) -gt 0.000001) { throw "Trail exposure differs at $fps FPS" }
}
# Uneven frames still cover the correct distance without controlling trail length.
$depth = [single]0.8
foreach ($interval in @(0.012,0.020,0.014,0.024,0.016)) { $depth = $advance.Invoke($null,@($depth,30,[double]$interval)) }
if ([Math]::Abs($depth-(0.8-0.086*30*0.0025)) -gt 0.000001) { throw 'Uneven frame integration failed' }
$settings = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Settings'))
$window = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'),@($settings))
function Find-Controls($control) { foreach($child in $control.Controls) { $child; Find-Controls $child } }
try {
    $controls = @(Find-Controls $window)
    $number = @($controls | Where-Object { $_.GetType().Name -eq 'NumberInput' })[0]
    $editor = $number.Controls[0]
    $editor.Text = '12345'
    if ($number.Value -ne 12345 -or $settings.Stars -ne 12345) { throw 'Direct typing failed' }
    $editor.Text = 'invalid'
    $editor.GetType().GetMethod('OnLeave',[Reflection.BindingFlags]'Instance,NonPublic').Invoke($editor,@([EventArgs]::Empty))
    if ($editor.Text -ne '12345') { throw 'Invalid input was not restored' }
    $editor.GetType().GetMethod('OnKeyDown',[Reflection.BindingFlags]'Instance,NonPublic').Invoke($editor,@([Windows.Forms.KeyEventArgs]::new([Windows.Forms.Keys]::Up)))
    if ($number.Value -ne 12445) { throw 'Keyboard increment failed' }
    $fpsChoice = @($controls | Where-Object { $_.GetType().Name -eq 'ChoiceInput' -and $_.Items.Contains(50) })[0]
    $fpsChoice.SelectedItem=50
    if ($settings.Fps -ne 50) { throw '50 FPS selection failed' }
    $natural = @($controls | Where-Object { $_ -is [Windows.Forms.CheckBox] -and $_.Text -eq 'Natural star colours' })[0]
    $natural.Checked=$false
    if ($settings.NaturalColours) { throw 'Natural colour toggle failed' }
} finally { $window.Dispose() }
# New colour preference survives XML, and older XML without it gets the default.
$serializer = [Xml.Serialization.XmlSerializer]::new($settings.GetType())
$writer = [IO.StringWriter]::new()
$serializer.Serialize($writer,$settings)
$reader = [IO.StringReader]::new($writer.ToString())
$loaded = $serializer.Deserialize($reader)
if ($loaded.NaturalColours -or $loaded.Fps -ne 50) { throw 'Settings XML round-trip failed' }
$reader.Dispose(); $writer.Dispose()
$oldReader = [IO.StringReader]::new('<Settings><Stars>10000</Stars><Fps>30</Fps></Settings>')
try { $old = $serializer.Deserialize($oldReader); if (!$old.NaturalColours -or $old.Fps -ne 30) { throw 'Old settings compatibility failed' } } finally { $oldReader.Dispose() }
$palette = $assembly.GetType('Brough.Starfield.StarPalette').GetMethod('Sample')
$colours = @(0.1,0.7,0.85,0.95,0.99 | ForEach-Object { $palette.Invoke($null,@([double]$_)).ToArgb() } | Sort-Object -Unique)
if ($colours.Count -ne 5) { throw 'Natural colour palette missing a class' }
Write-Output 'Motion, modern inputs, 50 FPS, natural colours and settings compatibility checks passed.'
