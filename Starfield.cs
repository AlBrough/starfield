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
 readonly Color muted=Color.FromArgb(155,168,187), ink=Color.FromArgb(230,236,245), surface=Color.FromArgb(24,30,41);
 public Configuration(Settings options) {
  SuspendLayout(); settings=options; Text="Starfield - Settings"; AutoScaleDimensions=new SizeF(96,96); AutoScaleMode=AutoScaleMode.Dpi; Font=new Font("Segoe UI",10);
  BackColor=Color.FromArgb(15,19,27); ForeColor=ink; ClientSize=new Size(1080,720); MinimumSize=new Size(900,680); StartPosition=FormStartPosition.CenterScreen;
  var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=Padding.Empty,Padding=Padding.Empty};
  layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,400)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); layout.RowStyles.Add(new RowStyle(SizeType.Percent,100)); Controls.Add(layout);
  var sidebar=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,BackColor=surface,Margin=Padding.Empty,Padding=new Padding(24,20,24,16)};
  sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute,80)); sidebar.RowStyles.Add(new RowStyle(SizeType.Percent,100)); sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute,62)); layout.Controls.Add(sidebar,0,0);
  var header=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty};
  header.Controls.Add(new Label {Text="STARFIELD",Font=new Font("Segoe UI",21,FontStyle.Bold),AutoSize=true,Location=new Point(0,0)});
  header.Controls.Add(new Label {Text="Make space your own.",ForeColor=muted,AutoSize=true,Location=new Point(2,43)}); sidebar.Controls.Add(header,0,0);
  var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true,Margin=Padding.Empty}; sidebar.Controls.Add(scroll,0,1);
  grid.Dock=DockStyle.Top; grid.AutoSize=true; grid.AutoSizeMode=AutoSizeMode.GrowAndShrink; grid.ColumnCount=1; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); grid.Padding=new Padding(0,0,12,12); scroll.Controls.Add(grid);
  Section("MOTION");
  Number("Number of stars",100,30000,100,settings.Stars,0,delegate(decimal v){settings.Stars=(int)v;});
  Number("Speed",1,200,1,settings.Speed,0,delegate(decimal v){settings.Speed=(int)v;});
  Number("Star size",.25m,5,.25m,(decimal)settings.Size,2,delegate(decimal v){settings.Size=(float)v;});
  Check("Motion stretch",settings.Motion,delegate(bool v){settings.Motion=v;});
  Number("Stretch amount",0,10,.1m,(decimal)settings.Stretch,1,delegate(decimal v){settings.Stretch=(float)v;});
  Section("APPEARANCE");
  Colour("Star colour",settings.StarColour,delegate(int v){settings.StarColour=v;}); Colour("Background",settings.Background,delegate(int v){settings.Background=v;});
  Check("Chromatic shift",settings.Shift,delegate(bool v){settings.Shift=v;});
  Section("DISPLAY");
  var fps=Combo(); foreach(int n in new[]{30,60,90,120,144})fps.Items.Add(n); if(!fps.Items.Contains(settings.Fps))fps.Items.Add(settings.Fps); fps.SelectedItem=settings.Fps; fps.SelectedIndexChanged+=delegate {settings.Fps=(int)fps.SelectedItem;}; AddLabel("FPS limit",fps);
  var centre=Combo(); centre.Items.AddRange(new object[]{"Desktop","Each monitor","Mouse position"}); centre.SelectedItem=settings.Centre; if(centre.SelectedIndex<0)centre.SelectedIndex=0; centre.SelectedIndexChanged+=delegate {settings.Centre=centre.Text;}; AddLabel("Centred on",centre);
  var buttons=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0,14,0,0)}; buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
  var save=Button("Save settings"); save.BackColor=Color.FromArgb(63,115,222); save.Click+=delegate {try {settings.Save(); Close();} catch(Exception ex){MessageBox.Show(this,"Could not save settings: "+ex.Message);} }; buttons.Controls.Add(save,0,0);
  var cancel=Button("Cancel"); cancel.Click+=delegate {Close();}; buttons.Controls.Add(cancel,1,0); sidebar.Controls.Add(buttons,0,2); AcceptButton=save; CancelButton=cancel;
  var right=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(20),Margin=Padding.Empty}; right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));right.RowStyles.Add(new RowStyle(SizeType.Absolute,34));right.RowStyles.Add(new RowStyle(SizeType.Percent,100));right.RowStyles.Add(new RowStyle(SizeType.Absolute,64));layout.Controls.Add(right,1,0);
  right.Controls.Add(new Label {Text="LIVE PREVIEW",ForeColor=muted,AutoSize=true,Margin=new Padding(0,2,0,0)},0,0);
  preview=new Field(settings); preview.Margin=Padding.Empty; right.Controls.Add(preview,0,1);
  var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0,16,0,0)}; footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,172));
  footer.Controls.Add(new Label {Text="Changes appear here as you adjust.\nSave to apply them to your screensaver.",ForeColor=muted,AutoSize=true,Dock=DockStyle.Fill,Margin=new Padding(0,2,8,0)},0,0);
  var full=Button("Full screen preview"); full.Click+=delegate {using(var f=new Form {FormBorderStyle=FormBorderStyle.None,Bounds=Screen.FromControl(this).Bounds,StartPosition=FormStartPosition.Manual,KeyPreview=true}) {f.Controls.Add(new Field(settings)); f.KeyDown+=delegate {f.Close();}; f.Controls[0].MouseDown+=delegate {f.Close();}; f.ShowDialog(this);} }; footer.Controls.Add(full,1,0);right.Controls.Add(footer,0,2);
  ResumeLayout(true);
 }
 Button Button(string text) {return new Button {Text=text,Dock=DockStyle.Fill,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(40,49,65),ForeColor=ink,Margin=new Padding(0,0,8,0),Cursor=Cursors.Hand};}
 ComboBox Combo() {return new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(38,46,61),ForeColor=ink};}
 void Add(Control control) {grid.RowCount=row+1;grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));control.Margin=new Padding(0,0,0,10);grid.Controls.Add(control,0,row++);}
 void Section(string text) {Add(new Label {Text=text,ForeColor=muted,AutoSize=true,Margin=new Padding(0,14,0,10),Font=new Font("Segoe UI",9,FontStyle.Bold)});}
 void AddLabel(string text,Control control) {var line=new TableLayoutPanel {Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,RowCount=1,Margin=Padding.Empty};line.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,43));line.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,57));line.Controls.Add(new Label {Text=text,AutoSize=true,Anchor=AnchorStyles.Left,Margin=new Padding(0,4,6,4)},0,0);line.Controls.Add(control,1,0);Add(line);}
 void Check(string text,bool value,Action<bool> change) {var c=new CheckBox {Text=text,Checked=value,AutoSize=true,Dock=DockStyle.Top};c.CheckedChanged+=delegate {change(c.Checked);};Add(c);}
 void Number(string text,decimal min,decimal max,decimal step,decimal value,int places,Action<decimal> change) {
  var panel=new TableLayoutPanel {AutoSize=true,Dock=DockStyle.Top,ColumnCount=2,RowCount=2,Margin=Padding.Empty}; panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,100));
  panel.RowStyles.Add(new RowStyle(SizeType.Absolute,28)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute,32));
  panel.Controls.Add(new Label {Text=text,AutoSize=true,Anchor=AnchorStyles.Left,Margin=new Padding(0,0,6,0)},0,0);
  var n=new NumericUpDown {Minimum=min,Maximum=max,Increment=step,DecimalPlaces=places,Value=value,Dock=DockStyle.Fill,BackColor=Color.FromArgb(38,46,61),ForeColor=ink,BorderStyle=BorderStyle.FixedSingle,TextAlign=HorizontalAlignment.Right,Margin=Padding.Empty,AccessibleName=text};
  var slider=new TrackBar {Minimum=0,Maximum=1000,TickStyle=TickStyle.None,Dock=DockStyle.Fill,Height=30,AutoSize=false,Margin=new Padding(0,4,0,0),Value=(int)((value-min)/(max-min)*1000),AccessibleName=text};
  bool updating=false; n.ValueChanged+=delegate {change(n.Value);if(!updating){updating=true;slider.Value=(int)((n.Value-min)/(max-min)*1000);updating=false;}};
  slider.ValueChanged+=delegate {if(!updating){updating=true;n.Value=Math.Round(min+(max-min)*slider.Value/1000,places);updating=false;}};
  panel.Controls.Add(n,1,0);panel.Controls.Add(slider,0,1);panel.SetColumnSpan(slider,2);Add(panel);
 }
 void Colour(string text,int value,Action<int> change) {var b=Button("Choose...");b.BackColor=Color.FromArgb(value);b.ForeColor=b.BackColor.GetBrightness()>.5?Color.Black:Color.White;b.Height=30;b.MinimumSize=new Size(0,30);b.AccessibleName=text;b.Click+=delegate {using(var d=new ColorDialog {Color=b.BackColor,FullOpen=true}) if(d.ShowDialog(this)==DialogResult.OK){b.BackColor=d.Color;b.ForeColor=d.Color.GetBrightness()>.5?Color.Black:Color.White;change(d.Color.ToArgb());}};AddLabel(text,b);}
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
