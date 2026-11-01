using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Media;

namespace Brough.Starfield {
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
 void CreateNebula(){
  if(nebula!=null)return;nebula=new Bitmap(256,144);
  for(int y=0;y<144;y++)for(int x=0;x<256;x++){
   double nx=x/255.0,ny=y/143.0;
   double clouds=Math.Exp(-((nx-.3)*(nx-.3)*15+(ny-.5)*(ny-.5)*8))+.8*Math.Exp(-((nx-.72)*(nx-.72)*25+(ny-.35)*(ny-.35)*12));
   double filament=.55+.45*Math.Sin(nx*22+Math.Sin(ny*12)*2)*Math.Sin(ny*17+nx*8);
   int alpha=(int)Math.Min(42,clouds*filament*34);
   nebula.SetPixel(x,y,Color.FromArgb(alpha,(int)(75+80*nx),95,(int)(160+70*ny)));
  }
 }
 public void Background(Graphics g,Size size,FlightFrame frame,double time){
  if(frame.Nebula<=.001)return;CreateNebula();
  using(var attributes=new ImageAttributes()){
   var matrix=new ColorMatrix();matrix.Matrix33=frame.Nebula;attributes.SetColorMatrix(matrix);
   int width=(int)(size.Width*1.45),height=(int)(size.Height*1.4);
   var target=new Rectangle((int)(-size.Width*.25+Math.Sin(time*.009)*size.Width*.10),(int)(-size.Height*.2+Math.Cos(time*.007)*size.Height*.05),width,height);
   g.DrawImage(nebula,target,0,0,nebula.Width,nebula.Height,GraphicsUnit.Pixel,attributes);
  }
 }
 public void Foreground(Graphics g,Size size,Settings options,FlightFrame frame,double time){
  if(frame.Alert>.001){
   float pulse=(float)(.8+.2*Math.Sin(time*Math.PI));
   using(var pen=new Pen(Color.FromArgb((int)(150*frame.Alert*pulse),235,65,65),Math.Max(2,size.Width/180f)))g.DrawRectangle(pen,3,3,Math.Max(1,size.Width-6),Math.Max(1,size.Height-6));
  }
  if(options.BridgeDisplay&&size.Width>=200&&size.Height>=120){
   float nextSize=Math.Max(8,Math.Min(14,size.Width/65f));
   if(bridgeFont==null||Math.Abs(fontSize-nextSize)>.1){if(bridgeFont!=null)bridgeFont.Dispose();fontSize=nextSize;bridgeFont=new Font("Consolas",fontSize,FontStyle.Regular);}
   double heading=(90+frame.OffsetX*180+360)%360;
   string label=string.Format(System.Globalization.CultureInfo.InvariantCulture,"WARP {0:0.00}   HDG {1:000.0}\nSIM STARDATE {2:00000.00}{3}",frame.Warp,heading,60200+time/86400,frame.Alert>.1?"   RED ALERT":"");
   SizeF measured=g.MeasureString(label,bridgeFont);float width=Math.Min(size.Width-20,measured.Width+24),height=measured.Height+20;
   float x=10+(float)((Math.Sin(time*.013)+1)*.5)*Math.Max(0,size.Width-width-20);
   float y=10+(float)((Math.Cos(time*.011)+1)*.5)*Math.Max(0,size.Height-height-20);
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
 public void Dispose(){if(chime!=null){chime.Stop();chime.Dispose();}if(wave!=null)wave.Dispose();if(nebula!=null)nebula.Dispose();if(bridgeFont!=null)bridgeFont.Dispose();}
}
}
