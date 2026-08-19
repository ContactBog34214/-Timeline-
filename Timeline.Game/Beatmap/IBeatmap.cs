using Line.Framework.Types;

namespace Timeline.Game.Beatmap;

public interface IBeatmap : IName
{
    long BeatmapID { get; }
    string Artist { get; set; }
    string Album { get; set; }
    List<string> MusicTag { get; }
    string BackgroundImagePath { get; set; }
    string AudioPath { get; set; }
    string[] ChartsPath { get; set; }
}