using Line.Framework.UI;

namespace Timeline.Game.Screen;

public abstract class Screen : UIWidget, IDescription
{
    public virtual bool AllowExit { get; } = true;
    public virtual bool Overlays { get; } = true;
    public virtual bool HideCursor { get; } = true;
    public abstract void Load();
    public static Screen FocusScreen { get; private set; }

    public static void LoadScreen(Screen scr)
    {
        FocusScreen?.Dispose();
        FocusScreen = scr;
        FocusScreen?.Load();
    }

    public virtual string Description { get; set; } = "";
}
