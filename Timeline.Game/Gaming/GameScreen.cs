using System.Numerics;
using Line.Framework.Default.UIWidgets;
using Line.Framework.Types;

namespace Timeline.Game.Gaming;

public abstract partial class GamingScreen : Screen.Screen
{
    public UIBox GamingFieldSurface { get; }
    public UIBox BackgroundSurface { get; }
    public UIBox GamingHudSurface { get; }
    public virtual double AspectRatio => 16d / 9d;
    public override bool AllowExit => false;
    public override bool Overlays => false;
    public override bool HideCursor => false;

    protected GamingScreen()
    {
        Size = new Coord2(new(), new(1, 1));
        GamingHudSurface = new()
        {
            Name = "HudSurface",
            Parent = this,
            Position = new Coord2(new(), new(0.5f)),
            Size = new Coord2(new(), new(1)),
            Anchor = new Vector2(0.5f),
            Index = 1,
        };
        BackgroundSurface = new()
        {
            Name = "BackgroundSurface",
            Parent = this,
            Position = new Coord2(new(), new(0.5f)),
            Size = new Coord2(new(), new(1)),
            Anchor = new Vector2(0.5f),
            Index = -1,
        };
        GamingFieldSurface = new()
        {
            Name = "FieldSurface",
            Parent = this,
            Position = new Coord2(new(), new(0.5f)),
            Anchor = new Vector2(0.5f),
            Index = 0,
            Size = new(
                () =>
                {
                    var s = GetSizeOnScreen();
                    float w = s.X;
                    float h = s.Y;
                    if (w / h > AspectRatio)
                    {
                        h = (float)(w / AspectRatio);
                    }
                    else if (w / h < AspectRatio)
                    {
                        w = (float)(AspectRatio * h);
                    }
                    return new(new(w, h), new());
                },
                true
            ),
        };
    }
}
