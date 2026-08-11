using System.Diagnostics;
using System.Numerics;
using Line.Framework.Graphics;
using Line.Framework.IO;
using Line.Framework.Resource;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;

namespace Timeline.Game.UIWidgets;

public class SlideBar : UIWidget
{
    public DynamicValue<double> MinimumValue { get; set; } = 0;
    public DynamicValue<double> MaximumValue { get; set; } = 1;
    public DynamicValue<double> Value { get; set; } = 0;
    public DynamicValue<bool> Enabled { get; set; } = true;
    public DynamicValue<RgbaFloat> SliderColor { get; set; } =
        new RgbaFloat(0.85f, 0.85f, 0.85f, 0.95f);
    public DynamicValue<RgbaFloat> SlideBarColor { get; set; } =
        new RgbaFloat(0.7f, 0.7f, 0.7f, 0.45f);
    public DynamicValue<float> SliderWidth { get; set; } = 0.9f;
    public DynamicValue<float> SliderLength { get; set; } = 0.2f;
    public DynamicValue<bool> AutoHide { get; set; } = true;
    public DynamicValue<double> WheelSpeed { get; set; } = 10;
    public DynamicValue<string> SliderTexture { get; set; } =
        "Timeline.Game.Assets.Textures.Circle.png";
    private readonly Stopwatch sw = new();
    private ICursor _cursor = null;

    private void WhenDrag(ICursor cursor)
    {
        if (HitTest(cursor.Position))
            sw.Reset();
        if (!Enabled)
            _cursor = null;
        if (_cursor != cursor)
            return;
        var s = GetSizeOnScreen();
        var sW = SliderWidth?.Value ?? 1;
        var sL = SliderLength?.Value ?? 1;

        var max = MaximumValue?.Value ?? 0;
        var min = MinimumValue?.Value ?? 0;

        var area = s.Y - (s.X - s.X * sW / 2);
        var of = area * (1 - sL);

        var ms = MousePosition(cursor.Position);
        var d = (ms - ClickPos).Y;
        var map = max - min;
        var delay = map * d / of;
        if (Value?.ReadOnly ?? true)
            Value = new(Value?.Value ?? 0, false);
        Value.SetValue(delay + OrValue);

        if (Value > max)
        {
            Value.SetValue(max);
        }
        else if (Value < min)
        {
            Value.SetValue(min);
        }
    }

    private void Wheel(IMouse mouse)
    {
        if (!HitTest(mouse.Position))
            return;
        double d = HorizontalWheel ? mouse.WheelDelta.X : mouse.WheelDelta.Y;
        d *= -(WheelSpeed?.Value ?? 1);
        var s = GetSizeOnScreen();
        var sW = SliderWidth?.Value ?? 1;
        var sL = SliderLength?.Value ?? 1;

        var max = MaximumValue?.Value ?? 0;
        var min = MinimumValue?.Value ?? 0;

        var area = s.Y - (s.X - s.X * sW / 2);
        var of = area * (1 - sL);
        var map = max - min;
        var delay = map * d / of;

        if (Value?.ReadOnly ?? true)
            Value = new(Value?.Value ?? 0, false);
        Value.SetValue(delay + Value.Value);

        if (Value > max)
        {
            Value.SetValue(max);
        }
        else if (Value < min)
        {
            Value.SetValue(min);
        }
    }

    private void WhenCursorDown(ICursor cursor)
    {
        if (!HitTest(cursor.Position))
            return;
        if (_cursor != null)
            return;
        ClickPos = MousePosition(cursor.Position);
        _cursor = cursor;
        OrValue = Value?.Value ?? 0;
        WhenPress?.Invoke(cursor);
    }

    private void WhenCursorUp(ICursor cursor)
    {
        if (cursor != _cursor && Enabled)
            return;
        _cursor = null;
        WhenRelease?.Invoke(cursor);
    }

    private InputManager im;
    private Vector2 ClickPos = new();
    private double OrValue = 0;
    public event Action<ICursor> WhenPress;
    public event Action<ICursor> WhenRelease;

    public override void SetParent(UINode value)
    {
        im?.CursorDown -= WhenCursorDown;
        im?.CursorMove -= WhenDrag;
        im?.CursorUp -= WhenCursorUp;
        im?.MouseWheel -= Wheel;

        base.SetParent(value);

        var root = FindRoot(value as UIWidget) as UIScreen;
        im = root?.window.Input;
        im?.CursorDown += WhenCursorDown;
        im?.CursorMove += WhenDrag;
        im?.CursorUp += WhenCursorUp;
        im?.MouseWheel += Wheel;
    }

    private readonly ResourceManager resource;
    public DynamicValue<bool> HorizontalWheel { get; set; } = false;

    public SlideBar(ResourceManager rm)
    {
        resource = rm;
    }

    public override async Task RendererContext(RendererContextArgs args)
    {
        var collector = args.Collector;
        var textureId = SliderTexture?.Value ?? "";
        if (textureId.Length == 0)
            return;
        var img = await resource.GetResource<ResourceSetArg>(textureId);
        if (img == default)
            return;

        double headSize = args.width / 2;
        if (MaximumValue - MinimumValue <= 0)
            return;

        var BgColor = SlideBarColor?.Value ?? new();
        var MainColor = SliderColor?.Value ?? new(1, 1, 1, 1f);
        var Opacity = 1f;
        if (AutoHide)
        {
            if (_cursor != null)
                sw.Reset();
            if (_cursor == null && !sw.IsRunning)
                sw.Restart();
            if (sw.ElapsedMilliseconds > 2500)
                Opacity = Maths
                    .InterpolationTool.CubicEaseOut(
                        new(1),
                        new(0),
                        Math.Min(1f, (sw.ElapsedMilliseconds - 2500) / 2000f)
                    )
                    .X;
        }
        else
        {
            if (sw.IsRunning)
                sw.Reset();
        }
        RgbaFloat o = new(1, 1, 1, Opacity + 0.2f);
        BgColor *= o;
        MainColor *= o;

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
        //背景
        //两头
        DrawUVBox(
            new(0),
            new Vector2((float)headSize) * new Vector2(2, 1),
            new(),
            new(new(), new(1, 0.5f)),
            BgColor
        );
        DrawUVBox(
            new(0, (float)(args.height - headSize)),
            new Vector2((float)headSize) * new Vector2(2, 1),
            new(new(), new(0, 0.5f)),
            new(new(), new(1)),
            BgColor
        );
        //中间
        collector.DrawRect(
            new()
            {
                X = 0,
                Y = (float)headSize,
                Width = 2 * (float)headSize,
                Height = (float)(args.height - 2 * headSize),
            },
            BgColor,
            this
        );

        //滑条块
        headSize *= SliderWidth?.Value ?? 1;
        if (!Enabled)
            MainColor =
                (MainColor + new RgbaFloat(0.5f, 0.5f, 0.5f, 0.8f)) / new RgbaFloat(2, 2, 2, 2f);
        else if (_cursor != null)
            MainColor =
                (MainColor + new RgbaFloat(0.7f, 0.7f, 0.7f, 1f)) / new RgbaFloat(2, 2, 2, 2f);

        float start = (float)(args.width - headSize * 2) / 2;
        var area = (float)args.height - ((float)args.width - (float)headSize);
        var sl = SliderLength?.Value ?? 0;
        var len = area * sl;
        var of = area - len;
        var max = MaximumValue?.Value ?? 0;
        var min = MinimumValue?.Value ?? 0;
        var now = Value?.Value ?? 0;
        var p = (float)((now - min) / (max - min));
        //头
        DrawUVBox(
            new(start, start + of * p),
            new Vector2((float)headSize) * new Vector2(2, 1),
            new(),
            new(new(), new(1, 0.5f)),
            MainColor
        );
        DrawUVBox(
            new(start, (float)(start + of * p + len)),
            new Vector2((float)headSize) * new Vector2(2, 1),
            new(new(), new(0, 0.5f)),
            new(new(), new(1)),
            MainColor
        );

        //中间
        collector.DrawRect(
            new()
            {
                X = start,
                Y = (float)(start + of * p + headSize),
                Width = (float)headSize * 2,
                Height = len - (float)headSize,
            },
            MainColor,
            this
        );
    }
}
