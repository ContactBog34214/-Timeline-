using System.Diagnostics;
using System.Reflection;

namespace Timeline.Game;

internal partial class TimelineGame
{
#if DEBUG
    public string GameName{get;} = "-Timeline Dev-";
#else
    public string GameName{get;} = "-Timeline-";
#endif
    public string GameDir { get; } = AppContext.BaseDirectory;
    public string VersionTag { get; } = "Origin";
    public string Version
    {
        get
        {
            var ret = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            if (!string.IsNullOrEmpty(ret))
                return ret.Split('+').First();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $@"{version.Major}.{version.Minor}.{version.Build}-{VersionTag}";
        }
    }
}
