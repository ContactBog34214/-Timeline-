using System.Numerics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Screen.Debug;
using Timeline.Game.UIWidgets;

namespace Timeline.Game.Screen;

public partial class DebugSurface : UIWidget
{
    private PerformanceChart PerformanceChartRender { get; init; }
    private PerformanceChart PerformanceChartupdate { get; init; }

    public DebugSurface()
    {
        var Host = TimelineGame.Running;
        _ = new PerformanceMonitor(Host.Host.Resource)
        {
            Index = 0,
            Visible = new(() =>
            {
                return Host.GameGraphicsCfg.ShowFPS;
            }),
            Anchor = new Vector2(1),
            FontSize = 25,
            FontId =
            [
                "Timeline.Game.Assets.Fonts.CascadiaMono.ttf",
                "Timeline.Game.Assets.Fonts.NotoSansSC.ttf",
            ],
            Parent = this,
            RoundedCorner = 20,
            Size = new Coord2(new(100, 60), new()),
            Position = new Coord2(new(-20, -20), new(1, 1)),
        };
        TouchMode = TouchModes.None;
        PerformanceChartRender = new(Host.Host.Resource)
        {
            Index = -1,
            Visible = new(() => Host.GameDebugToolCfg.PerformanceChartVisiable, true),
            Parent = this,
            Size = new Coord2(new(0, 400), new(0.4f, 0)),
            Multiple = 10,
            Position = new Coord2(new(), new(1, 1)),
            Anchor = new Vector2(1),
            MarkSize = 1,
            Num = 128,
            MarkFontId =
                (List<string>)
                    [
                        "Timeline.Game.Assets.Fonts.CascadiaMono.ttf",
                        "Timeline.Game.Assets.Fonts.NotoSansSC.ttf",
                    ],
            MarkPrefix = (a) =>
                Host.Localization.Get("Timeline.Game.DebugSurface.Ms", [a.ToString()]),
            BufferSize = 128,
        };
        Host.Host.OnRender += PerformanceChartRender.Update;
        PerformanceChartupdate = new(Host.Host.Resource)
        {
            Index = -2,
            Visible = new(() => Host.GameDebugToolCfg.PerformanceChartVisiable, true),
            Parent = this,
            Size = new Coord2(new(0, 400), new(0.4f, 0)),
            Multiple = 10,
            Position = new Coord2(new(0, -200), new(1, 1)),
            Anchor = new Vector2(1),
            MarkSize = 1,
            Num = 128,
            MarkFontId =
                (List<string>)
                    [
                        "Timeline.Game.Assets.Fonts.CascadiaMono.ttf",
                        "Timeline.Game.Assets.Fonts.NotoSansSC.ttf",
                    ],
            MarkPrefix = (a) =>
                Host.Localization.Get("Timeline.Game.DebugSurface.Ms", [a.ToString()]),
            BufferSize = 128,
        };
        Host.Host.OnUpdate += PerformanceChartupdate.Update;
    }
}
