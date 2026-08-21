using System.Numerics;

namespace Timeline.Game.Rulesets;

public interface IHitObject : IDuringTime
{
        static string Name { get; }
        static string ID { get; }
        HitLevel HitResult { get; }
        double HitTime { get; set; }
        Vector2 Position { get; }
        IPerformancePoints[] PerformancePoints { get; }
}
