using System.Security.Cryptography;
using Timeline.Game.Beatmap;
using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline;

public sealed class Chart : Game.Beatmap.Chart
{
    public override string RulesetID { get; } = "Timeline.Rulesets.Timeline";
    public List<Objects.Line> Lines { get; set; } = [];
}