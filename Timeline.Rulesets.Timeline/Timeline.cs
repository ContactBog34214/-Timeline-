using System.Collections.Concurrent;
using Line.Framework.IO;
using Line.Framework.Resource;
using Timeline.Game.Beatmap;
using Timeline.Game.Config;
using Timeline.Game.ResourceTypes;
using Timeline.Game.Rulesets;

namespace Timeline.Rulesets.Timeline;

public class Ruleset : IRuleset
{
    public async Task<IGameSession<IRuleset>> CreateGameSession(Map bm, Game.Beatmap.Chart c, CancellationToken token)
    {
        if (!(c is Chart f)) throw new InvalidDataException($"{TypeID} cannot load the map.");
        var s = new GameSession
        {
            Ruleset = this,
            TimeSets = [.. c.TimeSets],
            DifficultySettings = c.DifficultySettings,
            HitObjects = [.. c.HitObjects],
            Lines = [.. f.Lines],
        };
        return default;
    }

    public static HitLevel[] HitLevels { get; } =
    {
        new()
        {
            Name="Perfect",
            Accuracy=1m,
        },
        new()
        {
            Name="Just",
            Accuracy=0.8m,
        },
        new()
        {
            Name="Great",
            Accuracy=0.5m,
        },
        new()
        {
            Name="Good",
            Accuracy=0.3m,
        },
        new()
        {
            Name="Lost",
            Accuracy=0m,
        }
    };
    public ConfigType RulesetConfigs { get; set; } = new Config();
    public string TypeID { get; } = "Timeline.Rulesets.Timeline";
    public string Name { get; } = "Timeline.Rulesets.Timeline.Name";
    public string Description { get; } = "Timeline.Rulesets.Timeline.Description";
    public ConcurrentDictionary<string, StreamFile> Languages { get; } = new();
    public ResourceManager RuleSetResources { get; set; }

    public KeybindItem[] KeybindItems { get; } = [
        new(){
            ID="Timeline.Rulesets.Timeline.KeybindItems.ClickFirst",
            Name="Timeline.Rulesets.Timeline.KeybindItems.ClickFirst.Name",
            Description="Timeline.Rulesets.Timeline.KeybindItems.ClickFirst.Description",
            DefaultKeys=[
                KeyCode.A,KeyCode.B,KeyCode.C,KeyCode.D,KeyCode.E,KeyCode.F,KeyCode.G,
                KeyCode.H,KeyCode.I,KeyCode.J,KeyCode.K,KeyCode.L,KeyCode.M,KeyCode.N,
                KeyCode.O,KeyCode.P,KeyCode.Q,KeyCode.R,KeyCode.S,KeyCode.T,KeyCode.U,
                KeyCode.V,KeyCode.W,KeyCode.X,KeyCode.Y,KeyCode.Z,
                ],
            MaximumNum=100,
            MultiInput=true,
        },
    ];

    public RulesetPermission[] UsingPermissions { get; } = [
        RulesetPermission.Cursor,
        RulesetPermission.Touch
        ];
}