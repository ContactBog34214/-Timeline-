using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Line.Framework;
using Line.Framework.Graphics;
using Line.Framework.UI.DefaultWidget;
using Timeline.Game.Config;

namespace Timeline.Game;

public partial class TimelineGame
{
#if DEBUG
    public string GameName { get; } = "-Timeline Dev-";
#else
    public string GameName { get; } = "-Timeline-";
#endif
    public string GameDir { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Timeline"
        );
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

    public Stopwatch GameStopwatch { get; } = new();

    public TimelineGame(string[] args)
    {
        if (Running != null)
            return;
        Running = this;
        GameStopwatch.Start();
        Directory.CreateDirectory(Path.Combine(GameDir, "Logs"));
        Log.SetLogFile(
            Path.Combine(GameDir, "Logs", $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}.log")
        );
        if (Debugger.IsAttached)
            Log.SetMinLevel(LogLevel.Debug);
        else
            Log.SetMinLevel(LogLevel.Info);

        LoadConfigFile(out GraphicsCfg graphicsCfg);
        GameGraphicsCfg = graphicsCfg;

        LoadConfigFile(out UserInterfaceCfg userInterfaceCfg);
        GameUserInterfaceCfg = userInterfaceCfg;

        @Host = new(Backend: GameGraphicsCfg?.GraphicBackend ?? GraphicBackend.Vulkan)
        {
            FullScreen = GameGraphicsCfg?.FullScreen ?? true,
            EnableMouseRelative = true,
            ParallelRender = GameGraphicsCfg?.ParallelRender ?? true,
            FramePerSecond = GameGraphicsCfg?.FPSLimit ?? 1000,
            UpdatePerSecond = 10000,
            RequestQuit = () =>
            {
                if (Screen.Screen.FocusScreen?.AllowExit ?? true)
                    Task.Run(()=>@Host.Dispose());
            },
        };
        Log.Debug($"[{GetType().Name}] Window created");

        ScreenSurface = new UIBox()
        {
            Name = "Screen",
            color = new(0, 0, 0, 0),
            Size = new(new(), new(1, 1)),
            Z = 1,
        };

        Log.Debug("Loading intro screen");
        Screen.Intro intro = new();
        Screen.Screen.LoadScreenASync(intro);
    }

    internal UIBox ScreenSurface { get; init; }

    public void LoadConfigFile<T>(out T cfg)
        where T : ConfigType, new()
    {
        string typeName = typeof(T).Name;
        string filePath = Path.Combine(GameDir, "Data", "Config", $"{typeName}.json");

        try
        {
            string json = File.ReadAllText(filePath);
            cfg = JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            Log.Info($"Config file not found, creating default for {typeName}...");
            cfg = new T();
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }
        catch (Exception ex)
        {
            Log.Error($"[{typeName}] Load error: {ex}");
            cfg = new T();
            throw;
        }
        File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(cfg));
        Log.Debug($"Config {typeName} loaded");
    }

    internal Window @Host { get; set; }
    internal GraphicsCfg GameGraphicsCfg { get; set; }
    internal UserInterfaceCfg GameUserInterfaceCfg { get; set; }
    public static TimelineGame Running { get; private set; }
}
