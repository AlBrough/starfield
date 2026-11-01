using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Brough.Starfield {
public static class NavigationMath {
 public static double RandomHeading(){var bytes=new byte[4];using(var generator=RandomNumberGenerator.Create())generator.GetBytes(bytes);return BitConverter.ToUInt32(bytes,0)/4294967296.0*360;}
 public static double Heading(double initial,float correction){return Math.Round(((initial+correction*180)%360+360)%360,1)%360;}
 // App convention: local year +338; 2364 starts at 41000, 1000 units/local year.
 // This is an explicit TNG-era approximation, not an official canon conversion.
 public static double Stardate(DateTime localDate){
  DateTime start=new DateTime(localDate.Year,1,1);
  double secondsInYear=(DateTime.IsLeapYear(localDate.Year)?366:365)*86400.0;
  return (localDate.Year+338-2323)*1000.0+(localDate-start).TotalSeconds/secondsInYear*1000;
 }
}
static class NavigationSession {
 public static readonly double Heading=NavigationMath.RandomHeading();
}
public struct FlightFrame {
 public float Warp, ExtraStretch, OffsetX, OffsetY, Nebula, Alert;
}
public static class FlightMath {
 static double Smooth(double t){t=Math.Max(0,Math.Min(1,t));return t*t*(3-2*t);}
 static double Passage(double time,double period,double start,double duration){double phase=time%period-start;return phase<0||phase>duration?0:Math.Pow(Math.Sin(Math.PI*phase/duration),2);}
 public static FlightFrame Sample(Settings options,double time,double engagementAge,bool demo){
  time=Math.Max(0,time);var frame=new FlightFrame {Warp=options.WarpFactor};
  if(options.CruiseVariation)frame.Warp=(float)(1+(frame.Warp-1)*(.82+.18*Math.Cos(2*Math.PI*time/180)));
  if(options.RedAlert){frame.Alert=(float)Passage(time,demo?20:300,demo?6:120,8);frame.Warp+=(9.99f-frame.Warp)*.35f*frame.Alert;}
  if(options.Engagement){frame.Warp=(float)(1+(frame.Warp-1)*Smooth(engagementAge/3));frame.ExtraStretch=engagementAge<0||engagementAge>7?0:(float)(4*Math.Sin(Math.PI*engagementAge/7));}
  if(options.CourseCorrections){double envelope=demo?Math.Pow(Math.Sin(Math.PI*time/20),2):Passage(time,100,35,30);double direction=((int)(time/100)%2==0)?1:-1;frame.OffsetX=(float)(.06*envelope*direction);frame.OffsetY=(float)(.025*envelope*Math.Sin(time*.07));}
  if(options.NebulaPassages)frame.Nebula=(float)(demo?Math.Pow(Math.Sin(Math.PI*time/20),2):Passage(time,420,90,70));
  frame.Warp=WarpMath.Clamp(frame.Warp);return frame;
 }
}
sealed class FlightPainter : IDisposable {
 Bitmap nebula; Font bridgeFont; float fontSize; SoundPlayer chime; MemoryStream wave; bool sounded;
 Bitmap nebulaCache; double cachedAt=double.NegativeInfinity; float cachedOpacity; int cachedColour;
 byte[] nebulaPixels;
 [StructLayout(LayoutKind.Sequential)] struct BitmapHeader {public uint Size;public int Width,Height;public ushort Planes,Bits;public uint Compression,ImageSize;public int XPels,YPels;public uint Used,Important;}
 [DllImport("gdi32.dll")] static extern int StretchDIBits(IntPtr dc,int x,int y,int width,int height,int sourceX,int sourceY,int sourceWidth,int sourceHeight,byte[] pixels,ref BitmapHeader header,uint usage,uint operation);
 [DllImport("gdi32.dll")] static extern int SetStretchBltMode(IntPtr dc,int mode);
 [DllImport("gdi32.dll")] static extern bool SetBrushOrgEx(IntPtr dc,int x,int y,IntPtr previous);
 public Color BackgroundColour=Color.Black;
 void CreateNebula(){
  if(nebula!=null)return;nebula=new Bitmap(256,144,PixelFormat.Format32bppPArgb);
  for(int y=0;y<144;y++)for(int x=0;x<256;x++){
   double nx=x/255.0,ny=y/143.0;
   double clouds=Math.Exp(-((nx-.3)*(nx-.3)*15+(ny-.5)*(ny-.5)*8))+.8*Math.Exp(-((nx-.72)*(nx-.72)*25+(ny-.35)*(ny-.35)*12));
   double filament=.55+.45*Math.Sin(nx*22+Math.Sin(ny*12)*2)*Math.Sin(ny*17+nx*8);
   int alpha=(int)Math.Min(42,clouds*filament*34);
   nebula.SetPixel(x,y,Color.FromArgb(alpha,(int)(75+80*nx),95,(int)(160+70*ny)));
  }
 }
 public void Background(Graphics g,Size size,FlightFrame frame,double time){
  if(frame.Nebula<=.001||size.Width<1||size.Height<1)return;CreateNebula();
  // Composite translucency on a small opaque surface, not millions of screen pixels.
  double scale=Math.Min(1,640.0/Math.Max(size.Width,size.Height));
  var cacheSize=new Size(Math.Max(1,(int)(size.Width*scale)),Math.Max(1,(int)(size.Height*scale)));
  if(nebulaCache==null||nebulaCache.Size!=cacheSize){if(nebulaCache!=null)nebulaCache.Dispose();nebulaCache=new Bitmap(cacheSize.Width,cacheSize.Height,PixelFormat.Format32bppPArgb);cachedAt=double.NegativeInfinity;}
  if(time<cachedAt||time-cachedAt>=1.0/12||cachedColour!=BackgroundColour.ToArgb()||Math.Abs(cachedOpacity-frame.Nebula)>.05){
   using(var layer=Graphics.FromImage(nebulaCache))using(var attributes=new ImageAttributes()){
    layer.Clear(BackgroundColour);layer.InterpolationMode=InterpolationMode.Bilinear;
    var matrix=new ColorMatrix();matrix.Matrix33=frame.Nebula;attributes.SetColorMatrix(matrix);
    int width=(int)(cacheSize.Width*1.45),height=(int)(cacheSize.Height*1.4);
    var target=new Rectangle((int)(-cacheSize.Width*.25+Math.Sin(time*.009)*cacheSize.Width*.10),(int)(-cacheSize.Height*.2+Math.Cos(time*.007)*cacheSize.Height*.05),width,height);
    layer.DrawImage(nebula,target,0,0,nebula.Width,nebula.Height,GraphicsUnit.Pixel,attributes);
   }
   cachedAt=time;cachedOpacity=frame.Nebula;cachedColour=BackgroundColour.ToArgb();
   var data=nebulaCache.LockBits(new Rectangle(Point.Empty,cacheSize),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
   try{int length=data.Stride*data.Height;if(nebulaPixels==null||nebulaPixels.Length!=length)nebulaPixels=new byte[length];Marshal.Copy(data.Scan0,nebulaPixels,0,length);}finally{nebulaCache.UnlockBits(data);}
  }
  // Opaque native stretch avoids GDI+'s per-pixel alpha/resampling path at 4K.
  var header=new BitmapHeader {Size=40,Width=cacheSize.Width,Height=-cacheSize.Height,Planes=1,Bits=32,ImageSize=(uint)nebulaPixels.Length};
  IntPtr dc=g.GetHdc();bool copied=false;
  try{int previous=SetStretchBltMode(dc,3);SetBrushOrgEx(dc,0,0,IntPtr.Zero);copied=StretchDIBits(dc,0,0,size.Width,size.Height,0,0,cacheSize.Width,cacheSize.Height,nebulaPixels,ref header,0,0x00CC0020)>0;SetStretchBltMode(dc,previous);}finally{g.ReleaseHdc(dc);}
  if(!copied)g.DrawImage(nebulaCache,new Rectangle(Point.Empty,size));
 }
 public void Foreground(Graphics g,Size size,Settings options,FlightFrame frame,double time){
  if(frame.Alert>.001){
   float pulse=(float)(.8+.2*Math.Sin(time*Math.PI));
   using(var pen=new Pen(Color.FromArgb((int)(150*frame.Alert*pulse),235,65,65),Math.Max(2,size.Width/180f)))g.DrawRectangle(pen,3,3,Math.Max(1,size.Width-6),Math.Max(1,size.Height-6));
  }
  if(options.BridgeDisplay&&size.Width>=200&&size.Height>=120){
   float nextSize=Math.Max(8,Math.Min(14,size.Width/65f));
   if(bridgeFont==null||Math.Abs(fontSize-nextSize)>.1){if(bridgeFont!=null)bridgeFont.Dispose();fontSize=nextSize;bridgeFont=new Font("Consolas",fontSize,FontStyle.Regular);}
   double heading=NavigationMath.Heading(NavigationSession.Heading,frame.OffsetX);
   string label=string.Format(System.Globalization.CultureInfo.InvariantCulture,"WARP {0:0.00}   HEADING {1:000.0}\nSTARDATE {2:00000.000}{3}",frame.Warp,heading,NavigationMath.Stardate(DateTime.Now),frame.Alert>.1?"   RED ALERT":"");
   // Reserve alert text space so both the position and panel dimensions stay fixed.
   SizeF measured=g.MeasureString("WARP 9.99   HEADING 359.9\nSTARDATE 99999.999   RED ALERT",bridgeFont);float width=Math.Min(size.Width-20,measured.Width+24),height=measured.Height+20;
   float x=10,y=Math.Max(10,size.Height-height-10);
   using(var path=Theme.Rounded(new RectangleF(x,y,width,height),8))using(var fill=new SolidBrush(Color.FromArgb(155,12,19,29)))using(var ink=new SolidBrush(Color.FromArgb(180,180,210,240))){g.FillPath(fill,path);g.DrawString(label,bridgeFont,ink,new RectangleF(x+12,y+10,width-24,height-16));}
  }
  // Sound is independently opt-in. One original synthesised chime per alert passage.
  if(!options.AlertSound||!options.RedAlert){if(chime!=null)chime.Stop();sounded=false;return;}
  if(frame.Alert<=.01){sounded=false;return;}
  if(!sounded){sounded=true;try{if(chime==null)CreateChime();chime.Play();}catch(InvalidOperationException){} }
 }
 void CreateChime(){
  const int rate=22050,samples=13230;wave=new MemoryStream();var writer=new BinaryWriter(wave);
  writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples*2);
  for(int i=0;i<samples;i++){double t=i/(double)rate,frequency=t<.3?660:440,envelope=Math.Min(1,Math.Min(t/.04,(.6-t)/.08));writer.Write((short)(3500*Math.Max(0,envelope)*Math.Sin(2*Math.PI*frequency*t)));}
  writer.Flush();wave.Position=0;chime=new SoundPlayer(wave);chime.Load();
 }
 public void Dispose(){if(chime!=null){chime.Stop();chime.Dispose();}if(wave!=null)wave.Dispose();if(nebula!=null)nebula.Dispose();if(nebulaCache!=null)nebulaCache.Dispose();if(bridgeFont!=null)bridgeFont.Dispose();}
}
}
