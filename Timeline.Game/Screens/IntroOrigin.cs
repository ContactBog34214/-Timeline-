using System.Diagnostics;
using Line.Framework;
using Line.Framework.Graphics;
using Line.Framework.Resource.Graphic;
using Line.Framework.UI;
using Line.Framework.UI.DefaultWidget;
using Timeline.Game.Config;

namespace Timeline.Game.Screen;

public partial class Intro : Screen
{
    private async Task OriginScreen()
    {
        float L(float a)
        {
            return 1 - (1f - a) * (1f - a) * (1f - a);
        }

        var host = TimelineGame.Running;
        var rm = host?.Host?.Resource;
        var defaultFont = host?.GameUserInterfaceCfg?.DefaultFont ?? "";
        var r = rm?.GetResource(defaultFont);

        var title = new UIText(rm)
        {
            Name = "Title",
            color = new(1, 1, 1, 1),
            FontId = defaultFont,
            Text = $"-Timeline- {host?.VersionTag ?? ""}",
            Anchor = new(0.5f),
            Position = new(new(), new(0.5f)),
            Size = new(new(), new(9, 9)),
            FontScale = 0,
            Parent = this,
            XAlignment = Alignment.Center,
            YAlignment = Alignment.Center,
            TouchMode = TouchModes.None,
            Opacity = 0,
            Scale = new(1f / 9f),
        };

        Base.Start();
        while (2500 >= Base.ElapsedMilliseconds)
        {
            var t = Base.ElapsedMilliseconds;
            if (t < 1400)
            {
                var a = t / 2500f;
                title.Opacity = t / 1400f;
                title.FontScale = L(a);
            }
            else
            {
                var a = t / 2500f;
                title.Opacity = 1f - (t - 1400f) / 1100f;
                title.FontScale = L(a);
            }
        }
        title.Dispose();
    }
}
