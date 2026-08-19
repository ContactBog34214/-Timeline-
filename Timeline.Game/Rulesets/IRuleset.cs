using System.Collections.Concurrent;
using Line.Framework.Resource;
using Line.Framework.Types;
using Timeline.Game.Beatmap;
using Timeline.Game.ResourceTypes;

namespace Timeline.Game.Rulesets;

public interface IRuleset : IName, IDescription
{
        Task<IGameSession> CreateGameSession(IBeatmap Beatmap, IChart Chart, CancellationToken cancellationToken);
        static HitLevel[] HitLevels { get; }
        IRulesetConfigs RulesetConfigs { get; }
        string TypeID { get; }
        ConcurrentDictionary<string, StreamFile> Languages { get; }
        ResourceManager RuleSetResources { get; }
        KeybindItem[] KeybindItems { get; }
}
