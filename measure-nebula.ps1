param([string]$Label='current')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$frame=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightFrame'));$frame.Nebula=[single]0.8
$results=@()
foreach($size in @([Drawing.Size]::new(1920,1080),[Drawing.Size]::new(3840,2160))){
    $painter=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightPainter'),$true)
    $method=$painter.GetType().GetMethod('Background')
    $bitmap=[Drawing.Bitmap]::new($size.Width,$size.Height);$graphics=[Drawing.Graphics]::FromImage($bitmap)
    try{
        $times=@()
        foreach($pass in 0..3){
            $watch=[Diagnostics.Stopwatch]::StartNew()
            foreach($index in 0..29){
                $graphics.Clear([Drawing.Color]::Black)
                $method.Invoke($painter,@($graphics,$size,$frame,[double](100+$pass+$index/60.0)))
            }
            $watch.Stop()
            if($pass -gt 0){$times+=$watch.Elapsed.TotalMilliseconds/30}
        }
        $median=($times | Sort-Object)[1]
        $results+=[pscustomobject]@{Width=$size.Width;Height=$size.Height;MedianMsPerFrame=[Math]::Round($median,3)}
        if($size.Width -eq 1920){$bitmap.Save((Join-Path $PSScriptRoot "dist\nebula-$Label.png"))}
    }finally{$graphics.Dispose();$bitmap.Dispose();$painter.Dispose()}
}
$results | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $PSScriptRoot "dist\nebula-$Label.json")
$results | Format-Table
