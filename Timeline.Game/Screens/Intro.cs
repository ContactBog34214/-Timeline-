using System.Diagnostics;
using Line.Framework.Graphics;
using Line.Framework.UI;
using Line.Framework.UI.DefaultWidget;

namespace Timeline.Game.Screen;

public class Intro : Screen
{
    public override bool AllowExit => false;
    public override bool HideCursor => false;
    public override bool Overlays => false;
    readonly Stopwatch Base=new();
    public override void Load()
    {
        Base.Start();
    }
    public override void RendererContext(RendererContextArgs args)
    {
        var collector = args.Collector;
        var color=new Veldrid.RgbaFloat(0,0,0,1);
        var s = GetSizeOnScreen();
            if (s.X <= 0 && s.Y <= 0)
            {
                return;
            }
            var tl = new WindowsRenderer.Vertex(
                new(0, 0),
                color,
                new(new(), new(0, 0)),
                null,
                null,
                1
            );
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