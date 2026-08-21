using System.Numerics;
using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline.HitObjects;

public class Memory : IHitObject
{
    public static string Name { get; } = "Timeline.Rulesets.Timeline.HitObjects.Memory.Name";
    public static string ID { get; } = "Timeline.Rulesets.Timeline.HitObjects.Memory";
    public HitLevel HitResult { get; set; } = default;

    public Vector2 Position { get; set; } = new();

    public IPerformancePoints[] PerformancePoints { get; } = [new Aim(), new Beat()];

    public double DuringTime => 0;

    public double Time { get; set; } = 0;
    public double HitTime { get; set; } = 0;
}