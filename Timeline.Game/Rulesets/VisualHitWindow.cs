namespace Timeline.Game.Rulesets;

public readonly struct VisualHitWindow : IDuringTime
{
    //Earliest:Time
    public double Time { get; init; }
    //Latest  :DuringTime
    public double DuringTime { get; init; }
    public HitLevel HitLevel { get; init; }
}