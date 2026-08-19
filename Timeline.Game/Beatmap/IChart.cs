using Line.Framework.Types;
using Timeline.Game.Rulesets;

namespace Timeline.Game.Beatmap;

public interface IChart : IName
{
    List<IHitObject> HitObjects { get; set; }
    long ChartID { get; set; }
    IDifficultySetting[] DifficultySettings { get; set; }
    List<string> ChartTags { get; set; }
    List<TimeSet> TimeSets { get; set; }
}