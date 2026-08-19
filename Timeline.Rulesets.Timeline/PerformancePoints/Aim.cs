using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline;

public class Aim : IPerformancePoints
{
    public string ID { get; } = "Timeline.Rulesets.Timeline.PerformancePoints.Aim";
    public decimal Points { get; set; } = 0;
    public string Name { get; } = "Timeline.Rulesets.Timeline.PerformancePoints.Aim.Name";
}