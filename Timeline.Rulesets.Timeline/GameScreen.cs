using Timeline.Game.Screen.Gaming;

namespace Timeline.Rulesets.Timeline;

public class GameScreen : GamingScreen
{
    public override bool AllowExit => false;
    public override double AspectRatio => 4d / 3d;
    public override string Description { get; set; } = "Timeline.Rulesets.Timeline.GameScreen.Description";
    public override bool HideCursor => true;
    public override async Task Load() { }
    internal GameSession GameSession { get; }
    internal GameScreen(GameSession gs)
    {
        GameSession = gs;
    }
}