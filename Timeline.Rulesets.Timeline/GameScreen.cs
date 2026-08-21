using Line.Framework.Types;
using Timeline.Game.Gaming;
using Timeline.Rulesets.Timeline.Surfaces;

namespace Timeline.Rulesets.Timeline;

public sealed class GameScreen : GamingScreen
{
    public override bool AllowExit => false;
    public override double AspectRatio => 4d / 3d;
    public override string Description { get; set; } = "Timeline.Rulesets.Timeline.GameScreen.Description";
    public override bool HideCursor => true;
    public override async Task Load() { }
    internal GameSession GameSession { get; }
    private readonly HitObjectSurface HitObjectSurface;
    internal GameScreen(GameSession gs)
    {
        GameSession = gs;
        HitObjectSurface = new(gs.Ruleset, gs)
        {
            Name = "HitObjectSurface",
            Size = new Coord2(new(), new(1)),
            Parent = GamingFieldSurface,
        };
    }
    public override void Dispose()
    {
        HitObjectSurface?.Dispose();
        base.Dispose();
    }
}