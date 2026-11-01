using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Brough.Starfield {
public sealed class ReleaseVersion : IComparable<ReleaseVersion> {
 public readonly int Major,Minor,Patch;public readonly string Suffix;
 ReleaseVersion(int major,int minor,int patch,string suffix){Major=major;Minor=minor;Patch=patch;Suffix=suffix;}
 public static ReleaseVersion Parse(string tag){
  if(tag==null||tag.Length>100)return null;
  var match=Regex.Match(tag,@"\A[vV]?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z",RegexOptions.CultureInvariant);
  int major,minor,patch;if(!match.Success||!int.TryParse(match.Groups[1].Value,out major)||!int.TryParse(match.Groups[2].Value,out minor)||!int.TryParse(match.Groups[3].Value,out patch))return null;
  string suffix=match.Groups[4].Value;foreach(string part in suffix.Split('.'))if(part.Length>1&&part[0]=='0'&&Regex.IsMatch(part,@"^[0-9]+$"))return null;
  return new ReleaseVersion(major,minor,patch,suffix);
 }
 public int CompareTo(ReleaseVersion other){
  if(other==null)return 1;int result=Major.CompareTo(other.Major);if(result!=0)return result;result=Minor.CompareTo(other.Minor);if(result!=0)return result;result=Patch.CompareTo(other.Patch);if(result!=0)return result;
  if(Suffix==other.Suffix)return 0;if(Suffix.Length==0)return 1;if(other.Suffix.Length==0)return -1;
  string[] left=Suffix.Split('.'),right=other.Suffix.Split('.');
  for(int i=0;i<Math.Min(left.Length,right.Length);i++){
   bool ln=Regex.IsMatch(left[i],@"^[0-9]+$"),rn=Regex.IsMatch(right[i],@"^[0-9]+$");
   if(ln&&rn){result=left[i].Length.CompareTo(right[i].Length);if(result==0)result=string.CompareOrdinal(left[i],right[i]);}
   else if(ln!=rn)result=ln?-1:1;else result=string.CompareOrdinal(left[i],right[i]);
   if(result!=0)return result;
  }
  return left.Length.CompareTo(right.Length);
 }
 public override string ToString(){return Major+"."+Minor+"."+Patch+(Suffix.Length==0?"":"-"+Suffix);}
}
[DataContract] public sealed class GitHubRelease {
 [DataMember(Name="tag_name",IsRequired=true)] public string Tag;
 [DataMember(Name="draft",IsRequired=true)] public bool Draft;
 [DataMember(Name="prerelease",IsRequired=true)] public bool Preview;
}
public sealed class UpdateRelease {
 public readonly ReleaseVersion Version;public readonly bool Preview;public readonly string Url;
 public UpdateRelease(ReleaseVersion version,bool preview,string tag){Version=version;Preview=preview;Url=UpdateChecker.ReleasesUrl+"/tag/"+Uri.EscapeDataString(tag);}
 public bool IsNewerThan(ReleaseVersion current,bool currentPreview){int order=Version.CompareTo(current);return order>0||(order==0&&currentPreview&&!Preview);}
 public string DisplayVersion {get{string value=Version.ToString();if(value.Length>40)value=value.Substring(0,37)+"...";return value+(Preview?" (preview)":"");}}
}
public sealed class UpdateCheckException : Exception {public UpdateCheckException(string message):base(message){} }
public static class UpdateChecker {
 public const string ReleasesUrl="https://github.com/AlBrough/starfield/releases";
 public static UpdateRelease Select(IEnumerable<GitHubRelease> releases,bool includePreviews){
  UpdateRelease best=null;
  foreach(var item in releases){
   if(item==null||item.Draft)continue;var version=ReleaseVersion.Parse(item.Tag);if(version==null)continue;
   bool preview=item.Preview||version.Suffix.Length!=0;if(preview&&!includePreviews)continue;
   var candidate=new UpdateRelease(version,preview,item.Tag);
   if(best==null||version.CompareTo(best.Version)>0||(version.CompareTo(best.Version)==0&&best.Preview&&!preview))best=candidate;
  }
  return best;
 }
 public static async Task<UpdateRelease> CheckAsync(bool includePreviews,CancellationToken token){
  // GitHub requires TLS. Never weaken certificate validation or supply credentials.
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  using(var handler=new HttpClientHandler {AllowAutoRedirect=false,UseDefaultCredentials=false,AutomaticDecompression=DecompressionMethods.GZip|DecompressionMethods.Deflate})
  using(var client=new HttpClient(handler)){return await CheckAsync(client,includePreviews,token).ConfigureAwait(false);}
 }
 public static async Task<UpdateRelease> CheckAsync(HttpClient client,bool includePreviews,CancellationToken token){
  using(var deadline=CancellationTokenSource.CreateLinkedTokenSource(token)){
   deadline.CancelAfter(TimeSpan.FromSeconds(15));var releases=new List<GitHubRelease>();
   try{for(int page=1;page<=5;page++){
    using(var request=new HttpRequestMessage(HttpMethod.Get,"https://api.github.com/repos/AlBrough/starfield/releases?per_page=100&page="+page)){
     request.Headers.UserAgent.ParseAdd("Starfield/"+AppInfo.Version);request.Headers.Accept.ParseAdd("application/vnd.github+json");request.Headers.Add("X-GitHub-Api-Version","2026-03-10");
     using(var response=await client.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,deadline.Token).ConfigureAwait(false)){
      using(var abort=deadline.Token.Register(delegate {response.Dispose();})){
      if((int)response.StatusCode==403||(int)response.StatusCode==429)throw new UpdateCheckException("GitHub is limiting requests. Please try again later.");
      if(response.StatusCode!=HttpStatusCode.OK)throw new UpdateCheckException("Couldn't read GitHub releases. Please try again later.");
      if(response.Content.Headers.ContentLength>2*1024*1024)throw new UpdateCheckException("GitHub returned too much data. Please view releases directly.");
      using(var input=await response.Content.ReadAsStreamAsync().ConfigureAwait(false))using(var buffer=new MemoryStream()){
       var block=new byte[8192];int read;
       while((read=await input.ReadAsync(block,0,block.Length,deadline.Token).ConfigureAwait(false))>0){if(buffer.Length+read>2*1024*1024)throw new UpdateCheckException("GitHub returned too much data. Please view releases directly.");buffer.Write(block,0,read);}
       buffer.Position=0;GitHubRelease[] items;
       try{items=(GitHubRelease[])new DataContractJsonSerializer(typeof(GitHubRelease[])).ReadObject(buffer);}catch(SerializationException){throw new UpdateCheckException("GitHub returned an unexpected response. Please try again later.");}
       if(items==null)throw new UpdateCheckException("GitHub returned an unexpected response. Please try again later.");
       releases.AddRange(items);deadline.Token.ThrowIfCancellationRequested();if(items.Length<100)return Select(releases,includePreviews);
      }
      }
     }
    }
   }}catch(Exception){deadline.Token.ThrowIfCancellationRequested();throw;}
   throw new UpdateCheckException("There are too many releases to check completely. Please view releases directly.");
  }
 }
}
}
