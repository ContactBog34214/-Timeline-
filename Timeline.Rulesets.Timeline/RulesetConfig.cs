using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline;

public class RulesetConfig : IRulesetConfigs
{
    public decimal InputOffsetMs { get; set; } = 0;
    public double HitAnimationMsLength { get; set; } = 300;
}