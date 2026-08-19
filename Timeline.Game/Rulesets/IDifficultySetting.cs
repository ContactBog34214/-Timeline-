using Line.Framework.Types;

namespace Timeline.Game.Rulesets;

public interface IDifficultySetting : IName
{
    string DifficultySettingID { get; }
    decimal Value { get; }
    decimal MinixmumValue { get; }
    decimal MaximumValue { get; }
}