using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline.DifficulySettings;

public class Distance : IDifficultySetting
{
    public string DifficultySettingID { get; } = "Timeline.Rulesets.Timeline.DifficulySettings.Distance";

    public decimal Value { get; set; } = 0;

    public string Name { get; } = "Timeline.Rulesets.Timeline.DifficulySettings.Distance.Name";

    public decimal MinixmumValue { get; } = 0;

    public decimal MaximumValue { get; } = 10;
}