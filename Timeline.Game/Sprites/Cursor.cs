using System.Numerics;
using Line.Framework.Graphics;
using Line.Framework.Resource;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;

namespace Timeline.Game.Sprites;

public class Cursor : UIWidget
{
    readonly ResourceManager rm;
    public RgbaFloat Color { get; set; } = new(1, 1, 1, 1f);

    public Cursor()
    {
        rm = TimelineGame.Running.Host.Resource;
    }

    public override void RendererContext(RendererContextArgs args)
    {
        Vector2 sizeOnScreen = new((float)args.width, (float)args.height);
        if (!(sizeOnScreen.X <= 0f) || !(sizeOnScreen.Y <= 0f))
        {
            UIDrawCollector collector = args.Collector;
            if (
                rm.GetResource("Timeline.Game.Assets.Textures.Cursor.png")
                    is ResourceSetArg { ResourceSet: var resourceSet, Texture: var texture }
                && resourceSet != null
            )
            {
                WindowsRenderer.Vertex vertex5 = new WindowsRenderer.Vertex(
                    new Vector2(0f, 0f),
                    Color,
                    new Coord2(default(Vector2), new Vector2(0f, 0f)),
                    texture,
                    resourceSet,
                    1f
                );
                WindowsRenderer.Vertex vertex6 = new WindowsRenderer.Vertex(
                    new Vector2((float)args.height, 0f),
                    Color,
                    new Coord2(default(Vector2), new Vector2(1f, 0f)),
                    texture,
                    resourceSet,
                    1f
                );
                WindowsRenderer.Vertex vertex7 = new WindowsRenderer.Vertex(
                    new Vector2(0f, (float)args.width),
                    Color,
                    new Coord2(default(Vector2), new Vector2(0f, 1f)),
                    texture,
                    resourceSet,
                    1f
                );
                WindowsRenderer.Vertex vertex8 = new WindowsRenderer.Vertex(
                    new Vector2((float)args.height, (float)args.width),
                    Color,
                    new Coord2(default(Vector2), new Vector2(1f, 1f)),
                    texture,
                    resourceSet,
                    1f
                );
                collector.DrawVertex(
                    new WindowsRenderer.Vertex[3] { vertex5, vertex6, vertex7 },
                    this
                );
                collector.DrawVertex(
                    new WindowsRenderer.Vertex[3] { vertex6, vertex7, vertex8 },
                    this
                );
            }
        }
    }
}
