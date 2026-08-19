using Line.Framework.Types;

namespace Timeline.Game.Rulesets;

public interface IPerformancePoints : IName
{
    string ID { get; }
    decimal Points { get; }
}