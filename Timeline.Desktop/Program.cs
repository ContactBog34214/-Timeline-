using Line.Framework;
using Timeline;
using Timeline.Game;

namespace Timeline.Desktop;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var Game = new TimelineGame();
        await Entry.Run(async (_, args) => await Game.Game(args), args);
    }
}