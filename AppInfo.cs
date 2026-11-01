using System.Reflection;

[assembly: AssemblyTitle("Starfield")]
[assembly: AssemblyProduct("Starfield")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Al Brough")]
[assembly: AssemblyVersion(Brough.Starfield.AppInfo.Version + ".0")]
[assembly: AssemblyFileVersion(Brough.Starfield.AppInfo.Version + ".0")]
[assembly: AssemblyInformationalVersion(Brough.Starfield.AppInfo.Information)]

namespace Brough.Starfield {
public static class AppInfo {
 public const string Version="0.1.11";
 public const bool IsPreview=true;
 public const string Information=Version+(IsPreview?"-preview":"");
 public static string DisplayVersion {get{return Version+(IsPreview?" (preview)":"");}}
}
}
