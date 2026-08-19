using System.Numerics;

namespace Timeline.Rulesets.Timeline.Objects;

public struct ControlNode
{
    public Vector2 StartPosition { get; init; }
    public Vector2[] Position { get; init; }
    public NodeMode Mode { get; init; }
}