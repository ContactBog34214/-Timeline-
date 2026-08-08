using System.Numerics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Maths;

namespace Timeline.Game.Screen;

public partial class Intro : Screen
{
    private async Task OriginScreen()
    {
        var host = TimelineGame.Running;
        var rm = host?.Host?.Resource;
        var defaultFont = host?.GameUserInterfaceCfg?.DefaultFont ?? "";

        var title = new UIText(rm)
        {
            Name = "Title",
            color = new RgbaFloat(1, 1, 1, 1f),
            FontId = [defaultFont],
            Text = $"-Timeline- {host?.VersionTag ?? ""}",
            Anchor = new Vector2(0.5f),
            Position = new Coord2(new(), new(0.5f)),
            Size = new Coord2(new(), new(1)),
            FontSize = 0,
            Parent = this,
            XAlignment = Alignment.Center,
            YAlignment = Alignment.Center,
            TouchMode = TouchModes.None,
            Index = 0,
        };

        Base.Start();
        while (2500 >= Base.ElapsedMilliseconds)
        {
            var t = Base.ElapsedMilliseconds;
            if (t < 1400)
            {
                var a = t / 2500f;
                title.Opacity = t / 1400f;
                title.FontSize =
                    InterpolationTool.QuadraticEaseOut(new(0, 0), new(1, 0), a).X
                    * host.Host.Size.Y
                    / 13.5f;
            }
            else
            {
                var a = t / 2500f;
                title.Opacity = 1f - (t - 1400f) / 1100f;
                title.FontSize =
                    InterpolationTool.QuadraticEaseOut(new(0, 0), new(1, 0), a).X
                    * host.Host.Size.Y
                    / 13.5f;
            }
            await Task.Delay(2);
        }
        title.Dispose();
    }
}
