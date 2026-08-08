using System.Numerics;
using Line.Framework.Default.Graphics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Graphics;
using Line.Framework.IO;
using Line.Framework.Resource;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Maths;
using Timeline.Game.UIWidgets;

namespace Timeline.Game.Screen.Debug;

public sealed class PerformanceMonitor : NineGridScaleImage
{
    public DynamicValue<bool> FPSVisiable { get; set; } = true;
    public DynamicValue<bool> InputLagVisiable { get; set; } = true;
    private readonly Action<double> OnRender = null;
    private readonly Action<double> OnUpdate = null;
    public float RoundedCorner { get; set; } = 10;
    public RgbaFloat TextColor { get; set; } = new(175, 255, 168, 255);
    public Alignment XAlignment
    {
        get => Printer.XAlignment;
        set => Printer.XAlignment = value;
    }
    public Alignment YAlignment
    {
        get => Printer.YAlignment;
        set => Printer.YAlignment = value;
    }
    public List<string> FontId
    {
        get;
        set
        {
            if (value != null)
                field = value;
            Printer.FontId = value;
        }
    } = [];
    private readonly UIText Printer = null;
    public float FontSize
    {
        get => Printer.FontSize;
        set { Printer.FontSize = value; }
    }
    private float RenderFPS = 0;
    private float UpdateMs = 0;
    public float UpdateSpeed
    {
        get;
        set
        {
            field = value;
            if (field < 0)
                field = 0;
        }
    } = 0.2f;

    public PerformanceMonitor(ResourceManager rm)
        : base(rm)
    {
        Color = new RgbaFloat(127, 127, 127, 48);
        Host = TimelineGame.Running?.Host ?? null;
        if (!(Host?.Exists ?? false))
            throw new Exception($"No any games are running");
        Printer = new(Host.Resource)
        {
            Text = "",
            XAlignment = Alignment.Right,
            YAlignment = Alignment.Center,
            color = new RgbaFloat(1, 1, 1, 1f),
        };
        FontId.Add(TimelineGame.Running.GameUserInterfaceCfg.DefaultFont);
        Printer.FontId = new(FontId);

        void UpdateText()
        {
            if (InputLagVisiable)
            {
                Printer.Text = $"{((int)(UpdateMs * 10f)) / 10f}ms";
                if (FPSVisiable)
                    Printer.Text += "\n";
            }
            if (FPSVisiable)
                Printer.Text += $"{(int)RenderFPS}fps";
        }

        OnRender = (b) =>
        {
            float fps = 1000f / (float)b;
            RenderFPS = InterpolationTool.QuadraticEaseIn(new(RenderFPS), new(fps), UpdateSpeed).X;
            UpdateText();
        };
        OnUpdate = (b) =>
        {
            UpdateMs = InterpolationTool
                .QuadraticEaseIn(new(UpdateMs), new((float)b), UpdateSpeed)
                .X;
        };

        Host.OnRender += OnRender;
        Host.OnUpdate += OnUpdate;
    }

    readonly Window Host = null;

    public override async Task RendererContext(RendererContextArgs args)
    {
        float rcs = RoundedCorner;
        if (args.width / 2 < rcs)
            rcs = (float)args.width / 2;
        if (args.height / 2 < rcs)
            rcs = (float)args.height / 2;

        var collector = args.Collector;

        var size = Math.Min(args.width, args.height);
        UseWidgetSize = true;
        Top = Bottom = Left = Right = size / 2f;
        CornerScale = rcs / size;
        TextureId = "Timeline.Game.Assets.Textures.Circle.png";
        await base.RendererContext(args);

        Vector2 DrawArea =
            new Vector2((float)args.width, (float)args.height)
            - new Vector2(rcs) * new Vector2(0.5f, 0);
        UIDrawCollector TextCollector = new();
        RendererContextArgs TextArgs = new()
        {
            X = args.X,
            Y = args.Y,
            width = DrawArea.X,
            height = DrawArea.Y,
            Collector = TextCollector,
        };

        Printer?.color = TextColor;
        Printer?.XAlignment = Alignment.Right;
        Printer?.YAlignment = Alignment.Center;
        Printer?.RendererContext(TextArgs);
        var tc = TextArgs.Collector;
        foreach (var i in tc.Verts)
            collector.DrawVertex(i.Vert, this);
    }

    public override void Dispose()
    {
        base.Dispose();
        Host?.OnRender -= OnRender;
        Host?.OnUpdate -= OnUpdate;
        Printer?.Dispose();
    }
}
