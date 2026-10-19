using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Brough.Starfield {
public class Settings {
 public int Stars=10000, Speed=70, Fps=60, StarColour=Color.White.ToArgb(), Background=Color.Black.ToArgb();
 public float Size=1, Stretch=2;
 public bool Motion=true, Shift=false;
 public string Centre="Desktop";
 static string PathName { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Brough","Starfield","settings.xml"); } }
 public static Settings Load() { try { using(var f=File.OpenRead(PathName)) { var s=(Settings)new XmlSerializer(typeof(Settings)).Deserialize(f); s.Stars=Math.Max(100,Math.Min(30000,s.Stars)); s.Speed=Math.Max(1,Math.Min(200,s.Speed)); s.Fps=Math.Max(15,Math.Min(144,s.Fps)); s.Size=Math.Max(.25f,Math.Min(5,s.Size)); s.Stretch=Math.Max(0,Math.Min(10,s.Stretch)); return s; } } catch { return new Settings(); } }
 public void Save() { Directory.CreateDirectory(Path.GetDirectoryName(PathName)); string temp=PathName+".tmp"; using(var f=File.Create(temp)) new XmlSerializer(typeof(Settings)).Serialize(f,this); if(File.Exists(PathName)) File.Replace(temp,PathName,null); else File.Move(temp,PathName); }
}
public class Field : Control {
 struct Star { public float X,Y,Z; }
 Star[] stars; readonly Random random=new Random(); readonly Stopwatch watch=Stopwatch.StartNew(); readonly Timer timer=new Timer(); double last;
 public Settings Options; public PointF Origin=new PointF(.5f,.5f);
 public Field(Settings options) { Options=options; SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true); Dock=DockStyle.Fill; timer.Tick+=delegate { Invalidate(); }; timer.Interval=Math.Max(1,1000/options.Fps); timer.Start(); }
 Star NewStar(bool anywhere) { return new Star {X=(float)(random.NextDouble()*2-1),Y=(float)(random.NextDouble()*2-1),Z=anywhere?(float)(.02+random.NextDouble()*.98):1}; }
 protected override void Dispose(bool disposing) { if(disposing) timer.Dispose(); base.Dispose(disposing); }
 protected override void OnPaint(PaintEventArgs e) {
  var o=Options; timer.Interval=Math.Max(1,1000/o.Fps); e.Graphics.Clear(Color.FromArgb(o.Background)); if(Width<2||Height<2)return;
  if(stars==null||stars.Length!=o.Stars) { stars=new Star[o.Stars]; for(int i=0;i<stars.Length;i++)stars[i]=NewStar(true); }
  double now=watch.Elapsed.TotalSeconds,dt=last==0?0:Math.Min(.05,now-last); last=now;
  float scale=Math.Max(Width,Height)*.65f,cx=Width*Origin.X,cy=Height*Origin.Y;
  Color colour=Color.FromArgb(o.StarColour);
  if(o.Shift) { double h=now*.15; colour=Color.FromArgb((int)(155+100*Math.Sin(h)),(int)(155+100*Math.Sin(h+2.094)),(int)(155+100*Math.Sin(h+4.188))); }
  e.Graphics.SmoothingMode=SmoothingMode.None;
  using(var pen=new Pen(colour)) using(var brush=new SolidBrush(colour)) {
   for(int i=0;i<stars.Length;i++) {
    Star s=stars[i]; float previous=s.Z; s.Z-=(float)(dt*o.Speed*.0025);
    if(s.Z<.015f) { s=NewStar(false); previous=s.Z; }
    float x=cx+s.X/s.Z*scale,y=cy+s.Y/s.Z*scale;
    if(x< -30||x>Width+30||y< -30||y>Height+30) { stars[i]=NewStar(false); continue; }
    int alpha=(int)(40+215*(1-s.Z)); var c=Color.FromArgb(alpha,colour); pen.Color=c; brush.Color=c;
    float size=Math.Max(.6f,o.Size*(.35f+1.8f*(1-s.Z))); pen.Width=size;
    if(o.Motion&&o.Stretch>0) { float tail=s.Z+Math.Max(0,previous-s.Z)*o.Stretch; e.Graphics.DrawLine(pen,cx+s.X/tail*scale,cy+s.Y/tail*scale,x,y); }
    else e.Graphics.FillRectangle(brush,x,y,size,size);
    stars[i]=s;
   }
  }
 }
}
public class Saver : Form {
 [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr child,IntPtr parent);
 [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hwnd,int index);
 [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hwnd,int index,int value);
 [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr hwnd,out RECT rect);
 [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left,Top,Right,Bottom; }
 readonly bool embedded; Point start; bool armed; readonly Timer arm=new Timer(); readonly Timer resize=new Timer();
 public Saver(Settings options,Rectangle bounds,IntPtr parent) {
  embedded=parent!=IntPtr.Zero; FormBorderStyle=FormBorderStyle.None; StartPosition=FormStartPosition.Manual; Bounds=bounds; ShowInTaskbar=false; TopMost=!embedded; KeyPreview=true;
  var field=new Field(options); Controls.Add(field);
  if(options.Centre=="Mouse position") { Point p=Cursor.Position; field.Origin=new PointF((float)(p.X-bounds.X)/bounds.Width,(float)(p.Y-bounds.Y)/bounds.Height); }
  Action quit=delegate { if(!embedded) Application.Exit(); };
  KeyDown+=delegate { quit(); }; field.MouseDown+=delegate { quit(); };
  field.MouseMove+=delegate { if(armed&&(Math.Abs(Cursor.Position.X-start.X)>5||Math.Abs(Cursor.Position.Y-start.Y)>5))quit(); };
  arm.Interval=750; arm.Tick+=delegate { start=Cursor.Position; armed=true; arm.Stop(); }; arm.Start();
  Shown+=delegate { if(embedded) { SetParent(Handle,parent); SetWindowLong(Handle,-16,(GetWindowLong(Handle,-16)|0x40000000)&~unchecked((int)0x80000000)); resize.Interval=250; resize.Tick+=delegate { RECT r; if(GetClientRect(parent,out r)) Bounds=new Rectangle(0,0,r.Right,r.Bottom); else Close(); }; resize.Start(); } else Cursor.Hide(); };
  FormClosed+=delegate { arm.Dispose(); resize.Dispose(); if(!embedded)Cursor.Show(); };
 }
}
public class Configuration : Form {
 readonly Settings settings; readonly Field preview; readonly TableLayoutPanel grid=new TableLayoutPanel(); int row;
 public Configuration(Settings options) {
  settings=options; Text="Brough Starfield - Settings"; Font=new Font("Segoe UI",10); AutoScaleMode=AutoScaleMode.Dpi; MinimumSize=new Size(820,660); Size=new Size(1000,740); StartPosition=FormStartPosition.CenterScreen;
  var split=new SplitContainer {Dock=DockStyle.Fill,SplitterDistance=450,FixedPanel=FixedPanel.Panel1}; Controls.Add(split);
  grid.Dock=DockStyle.Fill; grid.Padding=new Padding(20); grid.ColumnCount=2; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55)); grid.AutoScroll=true; split.Panel1.Controls.Add(grid);
  preview=new Field(settings); split.Panel2.Controls.Add(preview);
  Add(new Label {Text="STARFIELD",Font=new Font("Segoe UI",20,FontStyle.Bold),AutoSize=true},null);
  Number("Number of stars",100,30000,100,settings.Stars,0,delegate(decimal v){settings.Stars=(int)v;});
  Number("Speed",1,200,1,settings.Speed,0,delegate(decimal v){settings.Speed=(int)v;});
  Number("Star size",.25m,5,.25m,(decimal)settings.Size,2,delegate(decimal v){settings.Size=(float)v;});
  Check("Motion stretch",settings.Motion,delegate(bool v){settings.Motion=v;});
  Number("Stretch amount",0,10,.1m,(decimal)settings.Stretch,1,delegate(decimal v){settings.Stretch=(float)v;});
  Colour("Stars",settings.StarColour,delegate(int v){settings.StarColour=v;}); Colour("Background",settings.Background,delegate(int v){settings.Background=v;});
  Check("Chromatic shift",settings.Shift,delegate(bool v){settings.Shift=v;});
  var fps=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill}; foreach(int n in new[]{30,60,90,120,144})fps.Items.Add(n); fps.SelectedItem=settings.Fps; if(fps.SelectedIndex<0)fps.SelectedIndex=1; fps.SelectedIndexChanged+=delegate {settings.Fps=(int)fps.SelectedItem;}; AddLabel("FPS limit",fps);
  var centre=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill}; centre.Items.AddRange(new object[]{"Desktop","Each monitor","Mouse position"}); centre.SelectedItem=settings.Centre; if(centre.SelectedIndex<0)centre.SelectedIndex=0; centre.SelectedIndexChanged+=delegate {settings.Centre=centre.Text;}; AddLabel("Centred on",centre);
  var buttons=new FlowLayoutPanel {AutoSize=true,Dock=DockStyle.Fill};
  var save=new Button {Text="Save",AutoSize=true}; save.Click+=delegate {try {settings.Save(); Close();} catch(Exception ex){MessageBox.Show(this,"Could not save settings: "+ex.Message);} }; buttons.Controls.Add(save);
  var cancel=new Button {Text="Cancel",AutoSize=true}; cancel.Click+=delegate {Close();}; buttons.Controls.Add(cancel);
  var full=new Button {Text="Full screen",AutoSize=true}; full.Click+=delegate {var f=new Form {FormBorderStyle=FormBorderStyle.None,Bounds=Screen.FromControl(this).Bounds,StartPosition=FormStartPosition.Manual,KeyPreview=true}; f.Controls.Add(new Field(settings)); f.KeyDown+=delegate {f.Close();}; f.Controls[0].MouseDown+=delegate {f.Close();}; f.ShowDialog(this);}; buttons.Controls.Add(full); Add(buttons,null);
  Add(new Label {Text="Full screen preview: press any key or click to return.\nSave applies settings to the Windows screensaver.",AutoSize=true,MaximumSize=new Size(380,0)},null);
 }
 void Add(Control a,Control b) {grid.RowStyles.Add(new RowStyle(SizeType.AutoSize)); a.Margin=new Padding(3,9,3,9);grid.Controls.Add(a,0,row);if(b==null)grid.SetColumnSpan(a,2);else {b.Margin=new Padding(3,9,3,9);grid.Controls.Add(b,1,row);}row++;}
 void AddLabel(string text,Control c) {Add(new Label {Text=text,AutoSize=true,Anchor=AnchorStyles.Left},c);}
 void Check(string text,bool value,Action<bool> change) {var c=new CheckBox {Text=text,Checked=value,AutoSize=true};c.CheckedChanged+=delegate {change(c.Checked);};Add(c,null);}
 void Number(string text,decimal min,decimal max,decimal step,decimal value,int places,Action<decimal> change) {
  var panel=new FlowLayoutPanel {AutoSize=true,Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false};
  var n=new NumericUpDown {Minimum=min,Maximum=max,Increment=step,DecimalPlaces=places,Value=value,Width=185};
  var slider=new TrackBar {Minimum=0,Maximum=1000,TickStyle=TickStyle.None,Width=185,Height=30,Value=(int)((value-min)/(max-min)*1000)};
  bool updating=false; n.ValueChanged+=delegate {change(n.Value);if(!updating){updating=true;slider.Value=(int)((n.Value-min)/(max-min)*1000);updating=false;}};
  slider.ValueChanged+=delegate {if(!updating){updating=true;n.Value=Math.Round(min+(max-min)*slider.Value/1000,places);updating=false;}};
  panel.Controls.Add(slider);panel.Controls.Add(n);AddLabel(text,panel);
 }
 void Colour(string text,int value,Action<int> change) {var b=new Button {Text="Choose colour...",BackColor=Color.FromArgb(value),ForeColor=Color.FromArgb(value).GetBrightness()>.5?Color.Black:Color.White,AutoSize=true};b.Click+=delegate {using(var d=new ColorDialog {Color=b.BackColor,FullOpen=true}) if(d.ShowDialog(this)==DialogResult.OK){b.BackColor=d.Color;b.ForeColor=d.Color.GetBrightness()>.5?Color.Black:Color.White;change(d.Color.ToArgb());}};AddLabel(text,b);}
}
static class Program {
 [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
 [STAThread] static void Main(string[] args) {
  SetProcessDPIAware(); Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false); var settings=Settings.Load(); string arg=args.Length==0?"/c":args[0].ToLowerInvariant();
  if(arg.StartsWith("/p")) {string raw=arg.Contains(":")?arg.Substring(arg.IndexOf(':')+1):(args.Length>1?args[1]:"");long handle;if(long.TryParse(raw,out handle)&&handle!=0)Application.Run(new Saver(settings,new Rectangle(0,0,320,240),new IntPtr(handle)));return;}
  if(arg.StartsWith("/s")) { if(settings.Centre=="Each monitor") {var context=new ApplicationContext();foreach(var screen in Screen.AllScreens){var f=new Saver(settings,screen.Bounds,IntPtr.Zero);f.Show();}Application.Run(context);}else Application.Run(new Saver(settings,SystemInformation.VirtualScreen,IntPtr.Zero)); }
  else Application.Run(new Configuration(settings));
 }
}
}
