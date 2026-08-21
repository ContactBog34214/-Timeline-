using System.Collections.Concurrent;
using Line.Framework.Resource;
using Line.Framework.Types;
using Timeline.Game.Beatmap;
using Timeline.Game.Config;
using Timeline.Game.ResourceTypes;

namespace Timeline.Game.Rulesets;

public interface IRuleset : IName, IDescription
{
        Task Load(CancellationToken token);
        Task<IGameSession<IRuleset>> CreateGameSession(Map Beatmap, Chart Chart, CancellationToken cancellationToken);
        static HitLevel[] HitLevels { get; }
        ConfigType RulesetConfigs { get; set; }
        string TypeID { get; }
        ConcurrentDictionary<string, StreamFile> Languages { get; }
        ResourceManager RuleSetResources { get; set; }
        KeybindItem[] KeybindItems { get; }
        RulesetPermission[] UsingPermissions { get; }
}
