using System.Numerics;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Screen.Debug;

namespace Timeline.Game.Screen;

public partial class DebugSurface : UIWidget
{
    private readonly PerformanceMonitor pf = null;

    public DebugSurface()
    {
        pf = new();
        TouchMode = TouchModes.None;
        pf.Index = 0;
        pf.Visible = new(() =>
        {
            return TimelineGame.Running?.GameGraphicsCfg.ShowFPS ?? false;
        });
        pf.Anchor = new Vector2(1);
        pf.FontSize = 25;
        pf.FontId = ["Timeline.Game.Assets.Fonts.CascadiaMono.ttf","Timeline.Game.Assets.Fonts.NotoSansSC.ttf"];
        pf.Parent = this;
        pf.RoundedCorner=6;
        pf.Size = new Coord2(new(100, 60), new());
        pf.Position = new Coord2(new(-20, -20), new(1, 1));
    }
}
