using System.Numerics;
using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline.HitObjects;

public class Tick : IHitObject
{
    public static string Name { get; } = "Timeline.Rulesets.Timeline.HitObjects.Tick.Name";
    public static string ID { get; } = "Timeline.Rulesets.Timeline.HitObjects.Tick";
    public Vector2 Position { get; set; } = new();

    public IPerformancePoints[] PerformancePoints { get; } = [new Aim(), new Beat()];

    public double DuringTime { get; } = 0;

    public double Time { get; set; } = 0;
    public HitLevel HitResult { get; set; } = default;
    public double HitTime { get; set; } = 0;
}