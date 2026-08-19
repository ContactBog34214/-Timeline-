using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline;

public class Beat : IPerformancePoints
{
    public string ID { get; } = "Timeline.Rulesets.Timeline.PerformancePoints.Beat";
    public decimal Points { get; set; } = 0;
    public string Name { get; } = "Timeline.Rulesets.Timeline.PerformancePoints.Beat.Name";
}