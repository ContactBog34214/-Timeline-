using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline.DifficulySettings;

public class ObjectSize : IDifficultySetting
{
    public string DifficultySettingID { get; } = "Timeline.Rulesets.Timeline.DifficulySettings.ObjectSize";

    public decimal Value { get; set; } = 0;

    public string Name { get; } = "Timeline.Rulesets.Timeline.DifficulySettings.ObjectSize.Name";

    public decimal MinixmumValue { get; } = 0;

    public decimal MaximumValue { get; } = 10;
}