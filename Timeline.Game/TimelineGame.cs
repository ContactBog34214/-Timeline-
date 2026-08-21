using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Line.Framework;
using Line.Framework.Default.Graphics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Graphics;
using Line.Framework.IO;
using Line.Framework.Resource;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Config;
using Timeline.Game.Maths;
using Timeline.Game.ResourceTypes;
using Timeline.Game.Screen;
using Timeline.Game.Sprites;

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
        //设置日志
        Directory.CreateDirectory(Path.Combine(GameDir, "Logs"));
        Directory.CreateDirectory(Path.Combine(GameDir, "Files"));
        Log.SetLogFile(
            Path.Combine(GameDir, "Logs", $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}.log")
        );
        if (Debugger.IsAttached)
            Log.SetMinLevel(LogLevel.Debug);
        else
            Log.SetMinLevel(LogLevel.Info);

        //设置虚拟文件系统
        File = new(Path.Combine(GameDir, "Files"));

        //加载配置文件
        GameGraphicsCfg = await LoadConfigFile<GraphicsCfg>();
        GameStorageCfg = await LoadConfigFile<StorageCfg>();
        GameUserInterfaceCfg = await LoadConfigFile<UserInterfaceCfg>();
        GameDebugToolCfg = await LoadConfigFile<DebugToolCfg>();

        //重载虚拟文件系统
        File = new(Path.Combine(GameDir, "Files"))
        {
            AllowCache = GameStorageCfg.EnableCache,
            CompressFile = GameStorageCfg.EnableCompress,
            MaximumCacheSize = GameStorageCfg.MaximumCacheSize
        };

        //创建窗口
        @Host = new(Backend: GameGraphicsCfg?.GraphicBackend ?? GraphicBackend.Vulkan)
        {
            FullScreen = GameGraphicsCfg?.FullScreen ?? true,
            EnableMouseRelative = true,
            FramePerSecond = GameGraphicsCfg?.FPSLimit ?? 1000,
            UpdatePerSecond = 5000,
            RequestQuit = () =>
            {
                if (Screen.Screen.FocusScreen?.AllowExit ?? true)
                    Task.Run(() => @Host.Dispose());
            },
            VSync = GameGraphicsCfg?.VSync ?? false,
        };
        Log.Debug($"[{GetType().Name}] Window created");
        Host.Resource.AddType("StreamFile", new TStreamFile());

        Host.OnUpdate += (_) =>
        {
            if (Host.VSync != (GameGraphicsCfg?.VSync ?? false))
                Host.VSync = GameGraphicsCfg?.VSync ?? false;
            if (!Host.IsFocus && !Host.VSync && GameGraphicsCfg.LimitFPSOnMinixmum)
                Host.VSync = true;
        };

        //加载资源文件
        var fontTask = LoadResourceGroupToGboal(
            "Fonts",
            ["ttf"],
            "Font",
            new(
                //token没啥用（
                async (_, res, token) =>
                {
                    if (!res?.IsLoaded ?? false)
                        await res?.Load();
                    ((Font)res.GetHandle())?.Size = (uint)(Host.Size.Y * 1);
                }
            )
        );

        var imgTask = LoadResourceGroupToGboal("Textures", ["png", "jpg", "jpeg"], "Image");
        var langTask = LoadResourceGroupToGboal(
            "Languages",
            ["json"],
            "StreamFile",
            new(
                async (id, obj, token) =>
                {
                    if (!(obj is RStreamFile))
                        return;
                    await obj?.Load();
                    var t = (obj.GetHandle() as StreamFile)?.Text ?? "";
                    var name = id.Split('.')[4];
                    Languages.TryAdd(name, t);
                }
            )
        );

        ScreenSurface = new UIBox()
        {
            Name = "Screen",
            color = new(0, 0, 0, 0),
            Size = new Coord2(new(), new(1, 1)),
            Index = 1,
            TouchMode = TouchModes.All,
            Parent = @Host.Root,
        };
        Overlay = new UIBox()
        {
            Name = "Overlay",
            color = new(0, 0, 0, 0),
            Size = new Coord2(new(), new(1, 1)),
            Index = 2,
            TouchMode = TouchModes.Children,
            Parent = @Host.Root,
            Visible = new Func<bool>(() => Screen.Screen.FocusScreen?.Overlays ?? true),
        };
        Background = new UIBox()
        {
            Name = "Background",
            color = new(0, 0, 0, 1),
            Size = new Coord2(new(), new(1, 1)),
            Index = 0,
            TouchMode = TouchModes.None,
            Parent = @Host.Root,
        };
        DebugInfoSurface = new DebugSurface()
        {
            Name = "DebugInfoSurface",
            Size = new Coord2(new(), new(1, 1)),
            Index = 65536,
            TouchMode = TouchModes.Children,
            Parent = @Host.Root,
            Visible = true,
        };

        await fontTask;
        await imgTask;
        await langTask;
        ReloadLanguage();

        GameCursor = new()
        {
            Name = "Cursor",
            Position = new(() => new(Host.Input.Mouse.Position, new()), true),
            Size = new(() => new(new(32 * GameUserInterfaceCfg.CursorSize), new()), true),
            Parent = Host.Root,
            Visible = new(() => (!Screen.Screen.FocusScreen?.HideCursor) ?? true, true),
            Index = 65536,
            TouchMode = TouchModes.None,
        };

        Log.Debug("Loading intro screen");
        Screen.Intro intro = new();
        await Screen.Screen.LoadScreenASync(intro);
    }

    public TimelineGame(string[] args)
    {
        Task.Run(() => Game(args)).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    async Task LoadResourceGroupToGboal(
        string GroupName,
        string[] GroupType,
        string Loader,
        Func<string, IResource, CancellationToken, Task> CreateHook = null,
        CancellationToken token = default
    )
    {
        var rm = Host.Resource;
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();
        List<Task> HookPool = [];
        foreach (var name in names)
        {
            if (token.IsCancellationRequested)
                break;
            var sp = name.Split('.');
            if (
                sp.Length > 5
                && sp[0] == "Timeline"
                && sp[1] == "Game"
                && sp[2] == "Assets"
                && sp[3] == GroupName
                && GroupType.Contains(sp.Last())
            )
            {
                Stream tg = null;
                try
                {
                    tg = assembly.GetManifestResourceStream(name);
                    var res = await rm.Create(Loader, name, tg);
                    await res.Load();
                    if (CreateHook != null)
                        HookPool.Add(CreateHook(name, res, token));
                    Log.Debug($"{GroupName}: {name} loaded.Loader:{Loader}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Cannot load resource({GroupName}/{Loader}) {name}:{ex}");
                }
                finally
                {
                    if (tg != null)
                        await tg.DisposeAsync();
                }
            }
        }
        if (CreateHook != null)
            await Task.WhenAll(HookPool);
    }

    /// <summary>
    /// 屏幕分段
    /// </summary>
    internal UIBox Background { get; private set; }
    internal UIBox ScreenSurface { get; private set; }
    internal UIBox Overlay { get; private set; }
    internal UIWidget DebugInfoSurface { get; private set; }
    internal Cursor GameCursor { get; private set; }

    /// <summary>
    /// 管理器
    /// </summary>
    public FileManager File { get; private set; }
    public Localization Localization { get; } = new();
    internal ConcurrentDictionary<string, string> Languages { get; } = new();
    private readonly object _reloadLanguageLock = new();

    internal void ReloadLanguage()
    {
        lock (_reloadLanguageLock)
        {
            Localization.ClearLanguage();
            foreach (var i in GameUserInterfaceCfg.Language)
            {
                if (!Languages.TryGetValue(i, out var lang))
                {
                    Log.Warning($"Language {i} does not exist. Skip.");
                    continue;
                }
                try
                {
                    Localization.SetLanguage(i, lang);
                    Log.Info($"Language {i} loaded");
                }
                catch (Exception ex)
                {
                    Log.Error($"Reload {i} language error:{ex}");
                }
            }
        }
    }

    internal Window @Host { get; set; }

    /// <summary>
    /// 配置文件组
    /// </summary>
    internal GraphicsCfg GameGraphicsCfg { get; set; }
    internal DebugToolCfg GameDebugToolCfg { get; set; }
    internal UserInterfaceCfg GameUserInterfaceCfg { get; set; }
    internal StorageCfg GameStorageCfg { get; set; }

    public async Task<T> LoadConfigFile<T>()
        where T : ConfigType, new()
    {
        string typeName = typeof(T).Name;
        string filePath = Path.Combine("Config", $"{typeName}.json");
        T cfg;
        void CreateIt()
        {
            cfg = new T();
            File.CreateDirectory(Path.GetDirectoryName(filePath));
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

    /// <summary>
    /// 会话
    /// </summary>
    public static TimelineGame Running { get; private set; }
}
