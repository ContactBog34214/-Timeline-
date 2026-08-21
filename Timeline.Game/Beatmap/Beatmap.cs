using System.Security.Cryptography;
using Line.Framework.Types;

namespace Timeline.Game.Beatmap;

public sealed class Map : IName
{
    public string Name { get; set; } = "";

    public long BeatmapID { get; set; } = RandomNumberGenerator.GetInt32(int.MinValue, -1);
    public string Artist { get; set; } = "";
    public string Album { get; set; } = "";
    public List<string> MusicTag { get; } = [];
    public string BackgroundImagePath { get; set; } = null;
    public string AudioPath { get; set; } = null;
    public List<string> ChartsPath { get; set; } = [];
}