$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly = [Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$settings = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Settings'))
function Get-Descendants($control) {
    foreach ($child in $control.Controls) { $child; Get-Descendants $child }
}
foreach ($scale in @(1.0, 1.25, 1.5, 2.0)) {
    $window = [Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Configuration'), @($settings))
    try {
        $window.Show()
        # Exercise layout without continuously repainting the starfield in DoEvents.
        $field = @(Get-Descendants $window | Where-Object { $_.GetType().Name -eq 'Field' })[0]
        $field.GetType().GetField('timer',[Reflection.BindingFlags]'Instance,NonPublic').GetValue($field).Stop()
        if ($scale -ne 1.0) { $window.Scale([Drawing.SizeF]::new($scale, $scale)) }
        foreach ($size in @([Drawing.Size]::new(1000,740), [Drawing.Size]::new(1400,850), [Drawing.Size]::new(900,680))) {
            $window.Size = [Drawing.Size]::new([int]($size.Width*$scale), [int]($size.Height*$scale))
            [Windows.Forms.Application]::DoEvents()
            $controls = @(Get-Descendants $window)
            $number = @($controls | Where-Object { $_.GetType().Name -eq 'NumberInput' })[0]
            $panel = $number.Parent
            while ($panel.Parent -ne $window -and $panel -isnot [Windows.Forms.SplitterPanel]) { $panel = $panel.Parent }
            if ($panel.ClientSize.Width -lt 360*$scale) { throw "Settings panel collapsed: $($panel.ClientSize.Width) px at scale $scale" }
            foreach ($label in @($controls | Where-Object { $_ -is [Windows.Forms.Label] -and $_.Text -in @('STARFIELD','Number of stars','Speed','Star size','Stretch amount','Centred on') })) {
                if ($label.Width -lt 100*$scale -and $label.Height -gt 32*$scale) { throw "Label wraps vertically: $($label.Text)" }
            }
            foreach ($input in @($controls | Where-Object { $_.GetType().Name -eq 'NumberInput' -or $_.GetType().Name -eq 'ChoiceInput' -or $_.GetType().Name -eq 'SmoothSlider' })) {
                if ($input.Right -gt $input.Parent.ClientSize.Width + 2) { throw "Input clipped: $($input.GetType().Name)" }
            }
            foreach ($action in @($controls | Where-Object { $_ -is [Windows.Forms.Button] -and $_.Text -in @('Save settings','Cancel','Full screen preview') })) {
                $location = $window.PointToClient($action.PointToScreen([Drawing.Point]::Empty))
                if ($location.X -lt 0 -or $location.Y -lt 0 -or $location.X+$action.Width -gt $window.ClientSize.Width -or $location.Y+$action.Height -gt $window.ClientSize.Height) { throw "Action outside window: $($action.Text)" }
            }
            $number.Value = 12000
            if ($settings.Stars -ne 12000) { throw 'Star count does not update preview settings' }
            $starSlider = @($controls | Where-Object { $_.GetType().Name -eq 'SmoothSlider' -and $_.AccessibleName -eq 'Number of stars' })[0]
            $starSlider.Value = 500
            if ($number.Value -ne 15050 -or $settings.Stars -ne 15050) { throw 'Slider and numeric value are not synchronised' }
        }
        if ($scale -eq 1.0) {
            $window.Size = [Drawing.Size]::new(1100,760)
            [Windows.Forms.Application]::DoEvents()
            $image = [Drawing.Bitmap]::new($window.Width,$window.Height)
            try { $window.DrawToBitmap($image,[Drawing.Rectangle]::new(0,0,$window.Width,$window.Height)); $image.Save((Join-Path $PSScriptRoot 'dist\settings-preview.png')) } finally { $image.Dispose() }
        }
        Write-Output "Layout passed at simulated scale $scale"
    } finally { $window.Close(); $window.Dispose() }
}
