using Timeline.Game.Config;
using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline;

public class Config : ConfigType
{
    public decimal InputOffsetMs { get; set; } = 0;
    public double HitAnimationMsLength { get; set; } = 300;
}