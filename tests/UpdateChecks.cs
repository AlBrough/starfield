using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brough.Starfield;

static class UpdateChecks {
 static void Assert(bool value,string message){if(!value)throw new Exception(message);}
 static string Json(string tag,bool preview=false,bool draft=false){return "{\"tag_name\":\""+tag+"\",\"draft\":"+draft.ToString().ToLowerInvariant()+",\"prerelease\":"+preview.ToString().ToLowerInvariant()+"}";}
 sealed class FakeHandler:HttpMessageHandler {
  public Func<int,HttpResponseMessage> Response;public int Calls;
  public FakeHandler(Func<int,HttpResponseMessage> response){Response=response;}
  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){
   Assert(request.RequestUri.Scheme=="https"&&request.RequestUri.Host=="api.github.com"&&request.RequestUri.AbsolutePath=="/repos/AlBrough/starfield/releases","Unexpected API destination");
   Assert(request.Headers.Authorization==null&&request.Headers.UserAgent.ToString().StartsWith("Starfield/"),"Credentials or user-agent wrong");
   Assert(request.RequestUri.Query.Contains("page="+(Calls+1)),"Pagination skipped a page");
   Calls++;return Task.FromResult(Response(Calls));
  }
 }
 static HttpResponseMessage Response(string json,HttpStatusCode status=HttpStatusCode.OK){return new HttpResponseMessage(status){Content=new StringContent(json)};}
 static UpdateRelease Check(FakeHandler handler,bool previews=true,CancellationToken token=default(CancellationToken)){using(var client=new HttpClient(handler))return UpdateChecker.CheckAsync(client,previews,token).GetAwaiter().GetResult();}
 static void Fails(Action run,string description){try{run();}catch(UpdateCheckException){return;}throw new Exception("Expected check failure: "+description);}
 sealed class BlockedStream:Stream {
  readonly TaskCompletionSource<int> pending=new TaskCompletionSource<int>();public bool Closed;
  public override bool CanRead{get{return true;}} public override bool CanSeek{get{return false;}} public override bool CanWrite{get{return false;}}
  public override long Length{get{throw new NotSupportedException();}} public override long Position{get{throw new NotSupportedException();}set{throw new NotSupportedException();}}
  public override Task<int> ReadAsync(byte[] data,int offset,int count,CancellationToken ignored){return pending.Task;}
  protected override void Dispose(bool disposing){Closed=true;pending.TrySetException(new ObjectDisposedException("blocked body"));base.Dispose(disposing);}
  public override int Read(byte[] b,int o,int c){throw new NotSupportedException();}public override void Flush(){}public override long Seek(long o,SeekOrigin s){throw new NotSupportedException();}public override void SetLength(long n){throw new NotSupportedException();}public override void Write(byte[] b,int o,int c){throw new NotSupportedException();}
 }
 sealed class UnknownLengthStream:MemoryStream {public UnknownLengthStream(byte[] data):base(data){}public override bool CanSeek{get{return false;}}}
 static IEnumerable<Control> Controls(Control control){foreach(Control child in control.Controls){yield return child;foreach(var nested in Controls(child))yield return nested;}}
 static T Find<T>(Control window,string name) where T:Control{foreach(var child in Controls(window))if(child is T&&child.Name==name)return (T)child;throw new Exception("Missing control: "+name);}
 static void StopPreview(Configuration window){foreach(var child in Controls(window))if(child is Field)typeof(Field).GetField("timer",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(child).GetType().GetMethod("Stop").Invoke(typeof(Field).GetField("timer",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(child),null);}
 static void Pump(Func<bool> condition){var end=DateTime.UtcNow.AddSeconds(3);while(!condition()&&DateTime.UtcNow<end){Application.DoEvents();Thread.Sleep(1);}Assert(condition(),"UI did not finish");}
 static void UiChecks(){
  var installed=ReleaseVersion.Parse(AppInfo.Version);string next=installed.Major+"."+installed.Minor+"."+(installed.Patch+1);
  int calls=0;var completion=new TaskCompletionSource<UpdateRelease>();
  using(var form=new Configuration(new Settings(),delegate(bool previews,CancellationToken token){calls++;return completion.Task;})){
   Assert(calls==0,"Update check runs automatically");form.Show();StopPreview(form);
   var button=Find<Button>(form,"CheckUpdates");var status=Find<LinkLabel>(form,"UpdateStatus");
   button.PerformClick();button.PerformClick();Assert(calls==1&&!button.Enabled,"Concurrent checks allowed");
   completion.SetResult(UpdateChecker.Select(new[]{new GitHubRelease {Tag="v"+next,Preview=true}},true));Pump(()=>button.Enabled);
   Assert(status.Text.Contains(next)&&status.Links.Count==1&&status.Links[0].LinkData.ToString()==UpdateChecker.ReleasesUrl+"/tag/v"+next,"New release link wrong");
  }
  foreach(float scale in new[]{1f,1.25f,1.5f,2f})using(var form=new Configuration(new Settings(),delegate(bool previews,CancellationToken token){var failed=new TaskCompletionSource<UpdateRelease>();failed.SetException(new UpdateCheckException("There are too many releases to check completely. Please view releases directly."));return failed.Task;})){
   form.Show();StopPreview(form);Find<Button>(form,"CheckUpdates").PerformClick();Pump(()=>Find<Button>(form,"CheckUpdates").Enabled);
   var status=Find<LinkLabel>(form,"UpdateStatus");Assert(status.Text.Contains("too many")&&status.Links.Count==1,"Failure state missing");
    if(scale!=1)form.Scale(new SizeF(scale,scale));Application.DoEvents();
    int height=TextRenderer.MeasureText(status.Text,status.Font,new Size(status.Width,1000),TextFormatFlags.WordBreak|TextFormatFlags.TextBoxControl).Height;
    Assert(height<=status.Height,"Update status/link clipped");
  }
  bool cancelled=false;
  using(var form=new Configuration(new Settings(),delegate(bool previews,CancellationToken token){var pending=new TaskCompletionSource<UpdateRelease>();token.Register(delegate{cancelled=true;pending.TrySetCanceled();});return pending.Task;})){
   form.Show();StopPreview(form);Find<Button>(form,"CheckUpdates").PerformClick();form.Close();form.Dispose();Application.DoEvents();Assert(cancelled,"Closing Settings did not cancel the check");
  }
 }
 [STAThread] static int Main(string[] args){try{
  if(args.Length>0&&args[0]=="--live"){
   var result=UpdateChecker.CheckAsync(true,CancellationToken.None).GetAwaiter().GetResult();Assert(result!=null,"Live repository has no release");Console.WriteLine("Live GitHub check: "+result.DisplayVersion+" "+result.Url);return 0;
  }
  Assert(Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Starfield.scr")).GetName().Version.ToString()==AppInfo.Version+".0","Installed metadata mismatch");
  Assert(ReleaseVersion.Parse("v0.1.10").CompareTo(ReleaseVersion.Parse("v0.1.9"))>0,"Lexical version comparison");
  Assert(ReleaseVersion.Parse("1.2.3-rc.10").CompareTo(ReleaseVersion.Parse("1.2.3-rc.2"))>0,"Prerelease number comparison");
  Assert(ReleaseVersion.Parse("1.2.3").CompareTo(ReleaseVersion.Parse("1.2.3-rc.10"))>0,"Stable ordering");
  foreach(string bad in new[]{"v0.1.11\n","https://evil.test","../v1.2.3","01.2.3","1.2.3-01","1.2","1.2.3#x","1.2.3/evil","999999999999999999999.0.0"})Assert(ReleaseVersion.Parse(bad)==null,"Unsafe/invalid tag accepted");
  var current=ReleaseVersion.Parse(AppInfo.Version);
  var samePreview=UpdateChecker.Select(new[]{new GitHubRelease {Tag="v"+AppInfo.Version,Preview=true}},true);
  Assert(!samePreview.IsNewerThan(current,true),"Same preview is an update");
  var sameStable=UpdateChecker.Select(new[]{new GitHubRelease {Tag="v"+AppInfo.Version}},true);
  Assert(sameStable.IsNewerThan(current,true)&&!sameStable.IsNewerThan(current,false),"Same-version promotion handling");
  var selected=Check(new FakeHandler(i=>Response("["+Json("v0.1.9")+","+Json("v0.1.10",true)+","+Json("v9.0.0",false,true)+"]")));
  Assert(selected.Version.ToString()=="0.1.10"&&selected.Url==UpdateChecker.ReleasesUrl+"/tag/v0.1.10","Release selection wrong");
  selected=Check(new FakeHandler(i=>Response("["+Json("v0.1.9")+","+Json("v0.1.10",true)+","+Json("v0.2.0-rc.1")+"]")),false);
  Assert(selected.Version.ToString()=="0.1.9","Preview filtering wrong");
  var hundred=new List<string>();for(int i=0;i<100;i++)hundred.Add(Json("v0.0."+i));string page="["+string.Join(",",hundred)+"]";
  var paginated=new FakeHandler(i=>Response(i==1?page:"["+Json("v0.2.0")+"]"));Assert(Check(paginated).Version.ToString()=="0.2.0"&&paginated.Calls==2,"Incomplete pagination");
  Fails(()=>Check(new FakeHandler(i=>Response(page))),"Truncated history");
  foreach(HttpStatusCode code in new[]{HttpStatusCode.Forbidden,(HttpStatusCode)429,HttpStatusCode.Found,HttpStatusCode.NotFound})Fails(()=>Check(new FakeHandler(i=>Response("[]",code))),"HTTP error or redirect");
  Fails(()=>Check(new FakeHandler(i=>Response("not json"))),"Malformed JSON");Fails(()=>Check(new FakeHandler(i=>Response("null"))),"Null JSON");
  Fails(()=>Check(new FakeHandler(i=>Response("[{\"tag_name\":\"v1.2.3\"}]"))),"Missing schema fields");
  Fails(()=>Check(new FakeHandler(i=>Response(new string('x',2*1024*1024+1)))),"Oversized response");
  Fails(()=>Check(new FakeHandler(i=>new HttpResponseMessage(HttpStatusCode.OK){Content=new StreamContent(new UnknownLengthStream(new byte[2*1024*1024+1]))})),"Unknown-length oversized stream");
  Assert(Check(new FakeHandler(i=>Response("[]")))==null,"Empty listing");
  var blocked=new BlockedStream();var timer=System.Diagnostics.Stopwatch.StartNew();
  try{Check(new FakeHandler(i=>new HttpResponseMessage(HttpStatusCode.OK){Content=new StreamContent(blocked)}));throw new Exception("Stalled body did not time out");}catch(OperationCanceledException){}
  Assert(blocked.Closed&&timer.Elapsed.TotalSeconds>=14&&timer.Elapsed.TotalSeconds<20,"End-to-end timeout did not abort body");
  UiChecks();Console.WriteLine("Version, preview, URL, pagination, HTTP/error, 15-second body timeout, metadata and UI checks passed.");return 0;
 }catch(Exception error){Console.Error.WriteLine(error);return 1;}}
}
