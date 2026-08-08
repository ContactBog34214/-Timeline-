using System.Numerics;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Screen.Debug;
using Timeline.Game.UIWidgets;

namespace Timeline.Game.Screen;

public partial class DebugSurface : UIWidget
{
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
    }
}
