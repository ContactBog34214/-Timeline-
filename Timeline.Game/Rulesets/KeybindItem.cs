using Line.Framework.IO;
using Line.Framework.Types;

namespace Timeline.Game.Rulesets;

public class KeybindItem : IName, IDescription
{
    public string Name { get; init; }
    public string Description { get; init; }
    public string ID { get; init; }
    public KeyCode[] DefaultKeys { get; init; } = [];
    public DynamicValue<KeyCode[]> Keys { get; set; }
    public int MaximumNum { get; init; }
    public virtual bool MultiInput { get; init; }
    public KeybindItem()
    {
        Keys = DefaultKeys;
    }
}