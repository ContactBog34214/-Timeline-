using System.Diagnostics;
using Line.Framework;
using Line.Framework.Graphics;
using Line.Framework.UI;
using Line.Framework.UI.DefaultWidget;
using Timeline.Game.Config;

namespace Timeline.Game.Screen;

public partial class Intro : Screen
{
    public override bool AllowExit => false;
    public override bool HideCursor => false;
    public override bool Overlays => false;
    public readonly Stopwatch Base=new();
    public override async Task Load()
    {
        Base.Reset();
        Log.Debug("Loaded intro screen");
        switch (TimelineGame.Running.GameUserInterfaceCfg.IntroScreen)
        {
            case IntroScreens.Origin:
                OriginScreen();
                break;
        }
    }
}