using Line.Framework.Types;

namespace Timeline.Game.Rulesets;

public readonly struct HitLevel : IName
{
        public string Name { get; init; }
        public decimal Accuracy { get; init; }
        public RgbaFloat Color { get; init; }
}
