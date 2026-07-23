namespace Timeline.Game.Config;

public class UserInterfaceCfg:ConfigType
{
    public string IntroScreen { get; set; } = IntroScreens.Origin;
}

public static class IntroScreens
{
    public const string Origin = "Origin";
}