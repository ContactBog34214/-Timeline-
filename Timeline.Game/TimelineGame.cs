using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Line.Framework;
using Line.Framework.Graphics;
using Line.Framework.IO;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;
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

    internal async Task Game(string[] args)
    {
        if (Running != null)
            return;
        Running = this;
        GameStopwatch.Start();
        Directory.CreateDirectory(Path.Combine(GameDir, "Logs"));
        Directory.CreateDirectory(Path.Combine(GameDir, "Files"));
        File = new(Path.Combine(GameDir, "Files"));
        Log.SetLogFile(
            Path.Combine(GameDir, "Logs", $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}.log")
        );
        if (Debugger.IsAttached)
            Log.SetMinLevel(LogLevel.Debug);
        else
            Log.SetMinLevel(LogLevel.Info);

        GameGraphicsCfg = await LoadConfigFile<GraphicsCfg>();

        GameUserInterfaceCfg = await LoadConfigFile<UserInterfaceCfg>();

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
                    Task.Run(() => @Host.Dispose());
            },
        };
        Log.Debug($"[{GetType().Name}] Window created");

        var fontTask = LoadAllFont();

        ScreenSurface = new UIBox()
        {
            Name = "Screen",
            color = new(0, 0, 0, 0),
            Size = new Coord2(new(), new(1, 1)),
            Z = 1,
            TouchMode = TouchModes.All,
            Parent = @Host.Root,
        };
        Overlay = new UIBox()
        {
            Name = "Overlay",
            color = new(0, 0, 0, 0),
            Size = new Coord2(new(), new(1, 1)),
            Z = 2,
            TouchMode = TouchModes.Children,
            Parent = @Host.Root,
        };
        Background = new UIBox()
        {
            Name = "Background",
            color = new(0, 0, 0, 1),
            Size = new Coord2(new(), new(1, 1)),
            Z = 0,
            TouchMode = TouchModes.None,
            Parent = @Host.Root,
        };

        fontTask?.Wait();

        Log.Debug("Loading intro screen");
        Screen.Intro intro = new();
        await Screen.Screen.LoadScreenASync(intro);
    }

    public TimelineGame(string[] args)
    {
        Task.Run(() => Game(args)).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    async Task LoadAllFont()
    {
        var rm = Host.Resource;
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();
        foreach (var name in names)
        {
            var sp = name.Split('.');
            if (
                sp.Length > 5
                && sp[0] == "Timeline"
                && sp[1] == "Game"
                && sp[2] == "Assets"
                && sp[3] == "Fonts"
                && sp.Last() == "ttf"
            )
            {
                try
                {
                    rm.Create("Font", name, assembly.GetManifestResourceStream(name));
                    ((Font)rm.GetResource(name)).Size = (uint)Host.Size.Y;
                    Log.Info($"Font {name} loaded");
                }
                catch (Exception ex)
                {
                    Log.Error($"Cannot load font {name}:{ex}");
                }
            }
        }
    }

    internal UIBox ScreenSurface { get; private set; }
    internal UIBox Overlay { get; private set; }
    internal UIBox Background { get; private set; }
    public FileManager File { get; private set; }

    public async Task<T> LoadConfigFile<T>()
        where T : ConfigType, new()
    {
        string typeName = typeof(T).Name;
        string filePath = Path.Combine("Config", $"{typeName}.json");
        T cfg;
        void CreateIt()
        {
            cfg = new T();
            File.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }
        if (File.FileExists(filePath))
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                cfg = JsonSerializer.Deserialize<T>(json) ?? new T();
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                Log.Info($"Config file not found, creating default for {typeName}...");
                CreateIt();
            }
            catch (Exception ex)
            {
                Log.Error($"[{typeName}] Load error: {ex}");
                cfg = new T();
                throw;
            }
        else
        {
            Log.Info($"Config file not found, creating default for {typeName}...");
            CreateIt();
        }
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(cfg));
        Log.Debug($"Config {typeName} loaded");
        return cfg;
    }

    internal Window @Host { get; set; }
    internal GraphicsCfg GameGraphicsCfg { get; set; }
    internal UserInterfaceCfg GameUserInterfaceCfg { get; set; }
    public static TimelineGame Running { get; private set; }
}
