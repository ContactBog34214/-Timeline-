using System.Security.Cryptography;
using Line.Framework.Types;
using Timeline.Game.Rulesets;

namespace Timeline.Game.Beatmap;

public abstract class Chart : IName
{
    public abstract string RulesetID { get; }
    public string Name { get; set; }
    public List<IHitObject> HitObjects { get; set; }
    public long ChartID { get; set; } = RandomNumberGenerator.GetInt32(int.MinValue, -1);
    public virtual IDifficultySetting[] DifficultySettings { get; set; }
    public List<string> ChartTags { get; set; }
    public List<TimeSet> TimeSets { get; set; }
}