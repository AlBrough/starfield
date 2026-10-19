using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Brough.Starfield {
static class Theme {
 public static readonly Color Input=Color.FromArgb(32,41,56), Border=Color.FromArgb(63,77,98), Accent=Color.FromArgb(104,164,255), Ink=Color.FromArgb(230,236,245);
 public static GraphicsPath Rounded(RectangleF r,float radius) {var p=new GraphicsPath(); float d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 public static void Surface(Graphics g,Rectangle bounds,bool focus) {g.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Rounded(new RectangleF(.5f,.5f,bounds.Width-1,bounds.Height-1),6))using(var b=new SolidBrush(Input))using(var pen=new Pen(focus?Accent:Border)){g.FillPath(b,p);g.DrawPath(pen,p);}}
}
public class NumberInput : Control {
 readonly TextBox editor=new TextBox(); decimal current; bool writing;
 public decimal Minimum {get;set;} public decimal Maximum {get;set;} public decimal Increment {get;set;} public int DecimalPlaces {get;set;}
 public event EventHandler ValueChanged;
 public decimal Value {get{return current;}set {decimal next=Math.Max(Minimum,Math.Min(Maximum,value));bool changed=next!=current;current=next;if(!writing)Format();if(changed&&ValueChanged!=null)ValueChanged(this,EventArgs.Empty);Invalidate();}}
 public NumberInput() {
  Minimum=0;Maximum=100;Increment=1;Height=30;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);TabStop=false;AccessibleRole=AccessibleRole.SpinButton;
  editor.BorderStyle=BorderStyle.None;editor.BackColor=Theme.Input;editor.ForeColor=Theme.Ink;editor.TextAlign=HorizontalAlignment.Center;Controls.Add(editor);
  editor.TextChanged+=delegate {decimal number;if(decimal.TryParse(editor.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out number)&&number>=Minimum&&number<=Maximum){writing=true;Value=number;writing=false;}};
  editor.Leave+=delegate {Format();Invalidate();};editor.Enter+=delegate {Invalidate();};
  editor.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Up||e.KeyCode==Keys.Down){Value+=e.KeyCode==Keys.Up?Increment:-Increment;e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Enter){Format();e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Escape){Format();e.SuppressKeyPress=true;}};
 }
 void Format(){writing=true;editor.Text=current.ToString("F"+DecimalPlaces,CultureInfo.CurrentCulture);writing=false;}
 protected override void OnLayout(LayoutEventArgs e){base.OnLayout(e);int unit=(int)(26*Font.Size/10);editor.SetBounds(unit,(Height-editor.PreferredHeight)/2,Math.Max(10,Width-unit*2),editor.PreferredHeight);editor.AccessibleName=AccessibleName;}
 protected override void OnFontChanged(EventArgs e){base.OnFontChanged(e);editor.Font=Font;PerformLayout();}
 protected override void OnPaint(PaintEventArgs e){Theme.Surface(e.Graphics,ClientRectangle,editor.Focused);int unit=(int)(26*Font.Size/10);TextRenderer.DrawText(e.Graphics,"-",Font,new Rectangle(0,0,unit,Height),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);TextRenderer.DrawText(e.Graphics,"+",Font,new Rectangle(Width-unit,0,unit,Height),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);editor.Focus();int unit=(int)(26*Font.Size/10);if(e.X<unit)Value-=Increment;else if(e.X>Width-unit)Value+=Increment;}
 protected override void OnMouseWheel(MouseEventArgs e){if(editor.Focused)Value+=e.Delta>0?Increment:-Increment;base.OnMouseWheel(e);}
}
public class SmoothSlider : Control {
 int current; public int Minimum {get;set;} public int Maximum {get;set;} public event EventHandler ValueChanged;
 public int Value {get{return current;}set {int next=Math.Max(Minimum,Math.Min(Maximum,value));if(next==current)return;current=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public SmoothSlider(){Maximum=1000;Height=28;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.Slider;SetStyle(ControlStyles.Selectable|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float pad=Math.Max(8,Font.Size),y=Height/2f,x=pad+(Width-pad*2)*(current-Minimum)/(float)Math.Max(1,Maximum-Minimum);using(var track=new Pen(Theme.Border,3))using(var fill=new Pen(Theme.Accent,3))using(var dot=new SolidBrush(Theme.Accent)){track.StartCap=track.EndCap=fill.StartCap=fill.EndCap=LineCap.Round;g.DrawLine(track,pad,y,Width-pad,y);g.DrawLine(fill,pad,y,x,y);float radius=Focused?6:5;g.FillEllipse(dot,x-radius,y-radius,radius*2,radius*2);}if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(2,2,Width-4,Height-4),Theme.Accent,BackColor);}
 void MoveTo(int x){float pad=Math.Max(8,Font.Size);Value=Minimum+(int)Math.Round(Math.Max(0,Math.Min(1,(x-pad)/Math.Max(1,Width-pad*2)))*(Maximum-Minimum));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){Focus();Capture=true;MoveTo(e.X);}}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture&&e.Button==MouseButtons.Left)MoveTo(e.X);}
 protected override void OnMouseUp(MouseEventArgs e){Capture=false;base.OnMouseUp(e);}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Home||key==Keys.End||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Left)Value-=10;if(e.KeyCode==Keys.Right)Value+=10;if(e.KeyCode==Keys.Home)Value=Minimum;if(e.KeyCode==Keys.End)Value=Maximum;}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();} protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
}
public class ColourInput : Control {
 Color colour=Color.White; public event EventHandler ValueChanged;
 public Color Value {get{return colour;}set{if(colour==value)return;colour=value;AccessibleDescription="Colour "+ColorTranslator.ToHtml(value);Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public ColourInput(){Height=32;MinimumSize=new Size(0,32);Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.PushButton;SetStyle(ControlStyles.Selectable|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
 protected override void OnPaint(PaintEventArgs e){Theme.Surface(e.Graphics,ClientRectangle,Focused);float radius=7*Font.Size/10;using(var fill=new SolidBrush(colour))using(var border=new Pen(Theme.Border)){e.Graphics.FillEllipse(fill,12,Height/2-radius,radius*2,radius*2);e.Graphics.DrawEllipse(border,12,Height/2-radius,radius*2,radius*2);}TextRenderer.DrawText(e.Graphics,"#"+colour.R.ToString("X2")+colour.G.ToString("X2")+colour.B.ToString("X2"),Font,new Rectangle((int)(20+radius*2),0,Width-(int)(26+radius*2),Height),Theme.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter);}
 void Choose(){using(var dialog=new ColorDialog {Color=Value,FullOpen=true})if(dialog.ShowDialog(this)==DialogResult.OK)Value=dialog.Color;}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){Focus();Choose();}}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Space){Choose();e.SuppressKeyPress=true;}}
 protected override bool ProcessDialogKey(Keys key){if(key==Keys.Enter){Choose();return true;}return base.ProcessDialogKey(key);}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
}
public class ChoiceInput : Control {
 readonly List<object> items=new List<object>(); int selected=-1;
 public List<object> Items {get{return items;}} public event EventHandler SelectedIndexChanged;
 public int SelectedIndex {get{return selected;}set{if(selected==value)return;selected=value>=0&&value<items.Count?value:-1;Text=selected<0?"":items[selected].ToString();Invalidate();if(SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty);}}
 public object SelectedItem {get{return selected<0?null:items[selected];}set{SelectedIndex=items.IndexOf(value);}}
 public ChoiceInput(){Height=32;MinimumSize=new Size(0,32);Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.ComboBox;SetStyle(ControlStyles.Selectable|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
 protected override void OnPaint(PaintEventArgs e){Theme.Surface(e.Graphics,ClientRectangle,Focused);TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(12,0,Math.Max(1,Width-42),Height),Theme.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);using(var pen=new Pen(Theme.Accent,1.6f)){float x=Width-20,y=Height/2f;e.Graphics.DrawLines(pen,new[]{new PointF(x-4,y-2),new PointF(x,y+2),new PointF(x+4,y-2)});}}
 void Open(){var menu=new ContextMenuStrip {BackColor=Theme.Input,ForeColor=Theme.Ink,ShowImageMargin=false,Font=Font};for(int i=0;i<items.Count;i++){int index=i;var item=new ToolStripMenuItem(items[i].ToString()){Checked=i==selected,AutoSize=false,Width=Math.Max(150,Width),Height=32};item.Click+=delegate{SelectedIndex=index;};menu.Items.Add(item);}menu.Closed+=delegate{menu.Dispose();};menu.Show(this,new Point(0,Height));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){Focus();Open();}}
 protected override bool IsInputKey(Keys key){return key==Keys.Up||key==Keys.Down||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Up){SelectedIndex=Math.Max(0,selected-1);e.Handled=true;}if(e.KeyCode==Keys.Down){SelectedIndex=Math.Min(items.Count-1,selected+1);e.Handled=true;}if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter||e.KeyCode==Keys.F4){Open();e.SuppressKeyPress=true;}}
 protected override bool ProcessDialogKey(Keys key){if(key==Keys.Enter){Open();return true;}return base.ProcessDialogKey(key);}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
}
}
