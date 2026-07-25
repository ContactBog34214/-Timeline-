namespace Timeline.Game.Config;

public class UserInterfaceCfg : ConfigType
{
    public string IntroScreen { get; set; } = IntroScreens.Origin;
    public string DefaultFont { get; set; } = "Timeline.Game.Assets.Fonts.GenJyuuGothic-Normal.ttf";
}

public static class IntroScreens
{
    public const string Origin = "Origin";
}
