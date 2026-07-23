using Line.Framework.Graphics;
using Line.Framework.UI;

namespace Timeline.Game.Screen;

public abstract class Screen : UIWidget, IDescription
{
    public virtual bool AllowExit { get; } = true;
    public virtual bool Overlays { get; } = true;
    public virtual bool HideCursor { get; } = true;
    public abstract Task Load();
    public static Screen FocusScreen { get; private set; } = null;

    public static void LoadScreen(Screen scr)
    {
        lock (Loadqueue)
        {
            Loadqueue.Add(scr);
        }
        if (LoadingScreen != null)
        {
            while (Loadqueue.Count != 0)
            {
                LoadingScreen?.Wait();
                if (Loadqueue[0] == scr)
                    break;
            }
        }
        FocusScreen?.Dispose();
        FocusScreen = scr;
        if (scr == null)
        {
            if (Loadqueue[0] == scr)
            {
                Loadqueue.RemoveAt(0);
            }
            return;
        }
        LoadingScreen = Task.Run(() =>
        {
            FocusScreen?.Load();
            if (Loadqueue[0] == scr)
            {
                Loadqueue.RemoveAt(0);
            }
        });
        if (LoadingScreen != null)
            LoadingScreen.Wait();
    }

    public static async Task LoadScreenASync(Screen scr)
    {
        lock (Loadqueue)
        {
            Loadqueue.Add(scr);
        }
        if (LoadingScreen != null)
        {
            while (Loadqueue.Count != 0)
            {
                await LoadingScreen;
                if (Loadqueue[0] == scr)
                    break;
            }
        }
        FocusScreen?.Dispose();
        FocusScreen = scr;
        if (scr == null)
        {
            if (Loadqueue[0] == scr)
            {
                Loadqueue.RemoveAt(0);
            }
            return;
        }
        LoadingScreen = Task.Run(() =>
        {
            FocusScreen?.Load();
            if (Loadqueue[0] == scr)
            {
                Loadqueue.RemoveAt(0);
            }
        });
        await LoadingScreen;
    }

    private static Task LoadingScreen = null;
    private static readonly List<Screen> Loadqueue = new();
    public virtual string Description { get; set; } = "";

    public override void RendererContext(RendererContextArgs args)
    {
        var collector = args.Collector;
        var color = new Veldrid.RgbaFloat(0, 0, 0, 1);
        var s = GetSizeOnScreen();
        if (s.X <= 0 && s.Y <= 0)
        {
            return;
        }
        var tl = new WindowsRenderer.Vertex(new(0, 0), color, new(new(), new(0, 0)), null, null, 1);
        var tr = new WindowsRenderer.Vertex(
            new((float)args.width, 0),
            color,
            new(new(), new(1, 0)),
            null,
            null,
            1
        );
        var bl = new WindowsRenderer.Vertex(
            new(0, (float)args.height),
            color,
            new(new(), new(0, 1)),
            null,
            null,
            1
        );
        var br = new WindowsRenderer.Vertex(
            new((float)args.width, (float)args.height),
            color,
            new(new(), new(1, 1)),
            null,
            null,
            1
        );
        collector.DrawVertex([tl, tr, bl], this);
        collector.DrawVertex([tr, bl, br], this);
    }
}
