using System.Diagnostics;
using System.Numerics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Graphics;
using Line.Framework.Resource;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;
using SDL3;

namespace Timeline.Game.UIWidgets;

public class NineGridScaleImage : UIImage
{
    public NineGridScaleImage(ResourceManager rm)
        : base(rm)
    {
        manager = rm;
    }

    protected ResourceManager manager;
    public DynamicValue<double> Top { get; set; } = 0;
    public DynamicValue<double> Bottom { get; set; } = 0;
    public DynamicValue<double> Left { get; set; } = 0;
    public DynamicValue<double> Right { get; set; } = 0;
    public DynamicValue<float> FadeInDuration { get; set; } = 0.2f;
    private bool load = false;
    private readonly Stopwatch stopwatch = new();
    private float fadeIn = 0;
    private string loadRes = "";

    public override async Task RendererContext(RendererContextArgs args)
    {
        var collector = args.Collector;
        if (args.width * args.height <= 0)
            return;

        Vertex tl = new(new(0, 0), BackgroundColor, new(new(), new(0, 0)), null, null, 1);
        Vertex tr = new(
            new((float)args.width, 0),
            BackgroundColor,
            new(new(), new(1, 0)),
            null,
            null,
            1
        );
        Vertex br = new(
            new((float)args.width, (float)args.height),
            BackgroundColor,
            new(new(), new(1, 1)),
            null,
            null,
            1
        );
        Vertex bl = new(
            new(0, (float)args.height),
            BackgroundColor,
            new(new(), new(0, 1)),
            null,
            null,
            1
        );
        collector.DrawVertex([tl, tr, bl, tr, bl, br], this);

        var img = null as ResourceSetArg;
        if (manager.ResourceIsLoaded(TextureId))
        {
            img = await manager.GetResource<ResourceSetArg>(TextureId);
        }
        else
        {
            await manager.LoadResource(TextureId);
            load = true;
            fadeIn = Math.Max(FadeInDuration, 0);
            stopwatch.Restart();
        }

        if (img?.Texture == null || img?.ResourceSet == null)
            return;
        void DrawUVBox(Vector2 Pos, Vector2 Size, Coord2 StartUV, Coord2 EndUV, RgbaFloat color)
        {
            Vector2 sUV =
                StartUV.offset / new Vector2(img.Texture.Width, img.Texture.Height) + StartUV.scale;
            Vector2 eUV =
                EndUV.offset / new Vector2(img.Texture.Width, img.Texture.Height) + EndUV.scale;
            Vertex tl = new(Pos, color, new(new(), sUV), img.Texture, img.ResourceSet, 1);
            Vertex tr = new(
                Pos + new Vector2(Size.X, 0),
                color,
                new(new(), new(eUV.X, sUV.Y)),
                img.Texture,
                img.ResourceSet,
                1
            );
            Vertex br = new(Pos + Size, color, new(new(), eUV), img.Texture, img.ResourceSet, 1);
            Vertex bl = new(
                Pos + new Vector2(0, Size.Y),
                color,
                new(new(), new(sUV.X, eUV.Y)),
                img.Texture,
                img.ResourceSet,
                1
            );
            collector.DrawVertex([tl, tr, bl, tr, bl, br], this);
        }
        var imgSize = new Vector2(img.Texture.Width, img.Texture.Height);
        var T = Top?.Value ?? 0;
        var B = Bottom?.Value ?? 0;
        var L = Left?.Value ?? 0;
        var R = Right?.Value ?? 0;
        Vector2 Judg = UseWidgetSize ? new((float)args.width, (float)args.height) : imgSize;
        Judg /= (float?)CornerScale?.Value ?? 1f;
        if (T + B > Judg.Y)
        {
            T = T / (T + B) * Judg.Y;
            B = Judg.Y - T;
        }

        if (L + R > Judg.X)
        {
            L = L / (L + R) * Judg.X;
            R = Judg.X - L;
        }

        var IT = T;
        var IB = imgSize.Y - B;
        var IL = L;
        var IR = imgSize.X - R;

        var m = Math.Min(args.width / imgSize.X, args.height / imgSize.Y);
        var ST = T * m * CornerScale;
        var SB = args.height - B * m * CornerScale;
        var SL = L * m * CornerScale;
        var SR = args.width - R * m * CornerScale;

        if (UseWidgetSize)
        {
            ST = T * CornerScale;
            SB = args.height - B * CornerScale;
            SL = L * CornerScale;
            SR = args.width - R * CornerScale;

            IT = T / m;
            IB = imgSize.Y - B / m;
            IL = L / m;
            IR = imgSize.X - R / m;
        }

        var cl = Color?.Value ?? new(1, 1, 1, 1f);
        float opacity = 1;
        var t = stopwatch.ElapsedMilliseconds * 1000;
        if (load)
        {
            if (fadeIn > t)
                opacity = t / fadeIn;
            else
                load = false;
        }
        cl *= new RgbaFloat(1, 1, 1, opacity);

        //四角
        DrawUVBox(
            new(0, 0),
            new((float)SL, (float)ST),
            new(),
            new(new((float)IL, (float)IT), new()),
            cl
        );
        DrawUVBox(
            new((float)SR, 0),
            new((float)(args.width - SR), (float)ST),
            new(new((float)IR, 0), new()),
            new(new(imgSize.X, (float)IT), new()),
            cl
        );
        DrawUVBox(
            new(0, (float)SB),
            new((float)SL, (float)(args.height - SB)),
            new(new(0, (float)IB), new()),
            new(new((float)IL, imgSize.Y), new()),
            cl
        );
        DrawUVBox(
            new((float)SR, (float)SB),
            new((float)(args.width - SR), (float)(args.height - SB)),
            new(new((float)IR, (float)IB), new()),
            new(imgSize, new()),
            cl
        );

        //四边

        var IW = IR - IL;

        //上下
        DrawUVBox(
            new((float)SL, 0),
            new((float)(SR - SL), (float)ST),
            new(new((float)IL, 0), new()),
            new(new((float)IR, (float)IT), new()),
            cl
        );
        DrawUVBox(
            new((float)SL, (float)SB),
            new((float)(SR - SL), (float)(args.height - SB)),
            new(new((float)IL, (float)IB), new()),
            new(new((float)(IL + IW), imgSize.Y), new()),
            cl
        );

        //左右
        DrawUVBox(
            new(0, (float)ST),
            new((float)SL, (float)(SB - ST)),
            new(new(0, (float)IT), new()),
            new(new((float)IL, (float)IB), new()),
            cl
        );
        DrawUVBox(
            new((float)SR, (float)ST),
            new((float)(args.width - SR), (float)(SB - ST)),
            new(new((float)IR, (float)IT), new()),
            new(new(imgSize.X, (float)IB), new()),
            cl
        );

        //中心
        DrawUVBox(
            new((float)SL, (float)ST),
            new((float)(SR - SL), (float)(SB - ST)),
            new(new((float)IL, (float)IT), new()),
            new(new((float)IR, (float)IB), new()),
            cl
        );
    }

    public DynamicValue<bool> UseWidgetSize { get; set; } = false;
    public DynamicValue<double> CornerScale { get; set; } = 1;
}
