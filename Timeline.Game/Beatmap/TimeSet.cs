namespace Timeline.Game.Beatmap;

public struct TimeSet : IHasTime
{
    public double Time { get; set; }
    public double BPM { get; set; }
    public int Beat { get; set; }
}