using System.Numerics;
using Line.Framework.Default.Graphics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Graphics;
using Line.Framework.IO;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Maths;

namespace Timeline.Game.Screen.Debug;

public sealed class PerformanceMonitor : UIWidget
{
    public DynamicValue<bool> FPSVisiable { get; set; } = true;
    public DynamicValue<bool> InputLagVisiable { get; set; } = true;
    private readonly Action<double> OnRender = null;
    private readonly Action<double> OnUpdate = null;
    public float RoundedCorner { get; set; } = 10;
    public RgbaFloat BackgroundColor { get; set; } = new(127, 127, 127, 48);
    public RgbaFloat TextColor { get; set; } = new(175,255,168, 255);
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

    public PerformanceMonitor()
    {
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

        Vector2 DrawArea =
            new Vector2((float)args.width, (float)args.height) - 2 * new Vector2(rcs);

        var collector = args.Collector;
        UIDrawCollector TextCollector = new();
        RendererContextArgs TextArgs = new()
        {
            X = args.X,
            Y = args.Y,
            width = DrawArea.X,
            height = DrawArea.Y,
            Collector = TextCollector,
        };
        ResourceSetArg Circle =
            await Host.Resource.GetResource("Timeline.Game.Assets.Textures.Circle.png") as ResourceSetArg;

        void DrawUVBox(Vector2 Pos, Vector2 Size, Coord2 StartUV, Coord2 EndUV)
        {
            if (Circle == null)
                return;
            Vector2 sUV =
                StartUV.offset / new Vector2(Circle.Texture.Width, Circle.Texture.Height)
                + StartUV.scale;
            Vector2 eUV =
                EndUV.offset / new Vector2(Circle.Texture.Width, Circle.Texture.Height)
                + EndUV.scale;
            Vertex tl = new(
                Pos,
                BackgroundColor,
                new(new(), sUV),
                Circle.Texture,
                Circle.ResourceSet,
                1
            );
            Vertex tr = new(
                Pos + new Vector2(Size.X, 0),
                BackgroundColor,
                new(new(), new(eUV.X, sUV.Y)),
                Circle.Texture,
                Circle.ResourceSet,
                1
            );
            Vertex br = new(
                Pos + Size,
                BackgroundColor,
                new(new(), eUV),
                Circle.Texture,
                Circle.ResourceSet,
                1
            );
            Vertex bl = new(
                Pos + new Vector2(0, Size.Y),
                BackgroundColor,
                new(new(), new(sUV.X, eUV.Y)),
                Circle.Texture,
                Circle.ResourceSet,
                1
            );
            collector.DrawVertex([tl, tr, bl], this);
            collector.DrawVertex([tr, bl, br], this);
        }
        void DrawBox(Vector2 Pos, Vector2 Size)
        {
            Vertex tl = new(Pos, BackgroundColor, new(new(), new(0, 0)), null, null, 1);
            Vertex tr = new(
                Pos + new Vector2(Size.X, 0),
                BackgroundColor,
                new(new(), new(1, 0)),
                null,
                null,
                1
            );
            Vertex br = new(Pos + Size, BackgroundColor, new(new(), new(1, 1)), null, null, 1);
            Vertex bl = new(
                Pos + new Vector2(0, Size.Y),
                BackgroundColor,
                new(new(), new(0, 1)),
                null,
                null,
                1
            );
            collector.DrawVertex([tl, tr, bl], this);
            collector.DrawVertex([tr, bl, br], this);
        }
        if (Circle?.Texture != null)
        {
            //Rounded Corner
            {
                DrawUVBox(new(0, 0), new(rcs), new(new(), new(0)), new(new(), new(0.5f)));
                DrawUVBox(
                    new((float)args.width - rcs, 0),
                    new(rcs),
                    new(new(), new(0.5f, 0)),
                    new(new(), new(1, 0.5f))
                );
                DrawUVBox(
                    new(0, (float)args.height - rcs),
                    new(rcs),
                    new(new(), new(0, 0.5f)),
                    new(new(), new(0.5f, 1f))
                );
                DrawUVBox(
                    new((float)args.width - rcs, (float)args.height - rcs),
                    new(rcs),
                    new(new(), new(0.5f, 0.5f)),
                    new(new(), new(1, 1))
                );
            }
            //Fill
            {
                DrawBox(new(rcs, 0), new(DrawArea.X, rcs));
                DrawBox(new(rcs, rcs + DrawArea.Y), new(DrawArea.X, rcs));
                DrawBox(new(0, rcs), new(rcs, DrawArea.Y));
                DrawBox(new(rcs + DrawArea.X, rcs), new(rcs, DrawArea.Y));
                DrawBox(new(rcs), DrawArea);
            }
        }

        Printer?.RendererContext(TextArgs);
        var tc = TextArgs.Collector;
        tc.Update();
        foreach (var i in tc.Verts)
        {
            List<Vertex> vertices = i.Vert.ToList();
            for (int idx = 0; idx < vertices.Count; idx++)
            {
                var item = vertices[idx];
                vertices[idx] = new(
                    item.Position + new Vector2(rcs),
                    TextColor,
                    item.UV,
                    item.Texture,
                    item.ResourceSet,
                    item.Opacity
                );
            }
            collector.DrawVertex(vertices.ToArray(), this);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        Host?.OnRender -= OnRender;
        Host?.OnUpdate -= OnUpdate;
        Printer?.Dispose();
    }
}
