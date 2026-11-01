$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
Add-Type -ReferencedAssemblies ([Drawing.Bitmap].Assembly.Location),([Drawing.Rectangle].Assembly.Location),System.Runtime -TypeDefinition @'
using System.Drawing;
public static class BridgePixels {
 public static Rectangle Bounds(Bitmap image){int left=image.Width,top=image.Height,right=-1,bottom=-1;for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++){Color p=image.GetPixel(x,y);if(p.R!=0||p.G!=0||p.B!=0){left=System.Math.Min(left,x);top=System.Math.Min(top,y);right=System.Math.Max(right,x);bottom=System.Math.Max(bottom,y);}}return right<0?Rectangle.Empty:Rectangle.FromLTRB(left,top,right+1,bottom+1);}
}
'@
$settings=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.Settings'));$settings.BridgeDisplay=$true
$frameType=$assembly.GetType('Brough.Starfield.FlightFrame')
$flags=[Reflection.BindingFlags]'Instance,NonPublic,Public'
foreach($size in @([Drawing.Size]::new(960,600),[Drawing.Size]::new(1920,1080))){
    $painter=[Activator]::CreateInstance($assembly.GetType('Brough.Starfield.FlightPainter'),$true)
    $bitmap=[Drawing.Bitmap]::new($size.Width,$size.Height);$graphics=[Drawing.Graphics]::FromImage($bitmap)
    $baseline=$null
    try{
        foreach($time in @(0.0,60.0,300.0,900.0)){
            $graphics.Clear([Drawing.Color]::Black)
            $frame=[Activator]::CreateInstance($frameType);$frame.Warp=[single]9;$frame.Alert=if($time -eq 300){[single]1}else{[single]0}
            $painter.GetType().GetMethod('Foreground',$flags).Invoke($painter,@($graphics,$size,$settings,$frame,$time))
            # Remove only the outer alert border; the bridge panel starts at the 10 px inset.
            $graphics.FillRectangle([Drawing.Brushes]::Black,0,0,$size.Width,10)
            $graphics.FillRectangle([Drawing.Brushes]::Black,0,$size.Height-10,$size.Width,10)
            $graphics.FillRectangle([Drawing.Brushes]::Black,0,0,10,$size.Height)
            $graphics.FillRectangle([Drawing.Brushes]::Black,$size.Width-10,0,10,$size.Height)
            $bounds=[BridgePixels]::Bounds($bitmap)
            if($null -eq $baseline){$baseline=$bounds}else{if($bounds -ne $baseline){throw "Bridge panel moved at $time seconds"}}
            if($bounds.Left -gt 12 -or $bounds.Bottom -lt $size.Height-12){throw 'Bridge panel is not anchored bottom left'}
        }
        if($size.Width -eq 960){$bitmap.Save((Join-Path $PSScriptRoot 'dist\bridge-stationary.png'))}
    }finally{$graphics.Dispose();$bitmap.Dispose();$painter.Dispose()}
}
Write-Output 'Bridge panel remains anchored across time at preview and full HD sizes.'
