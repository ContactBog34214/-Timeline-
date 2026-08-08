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

    public override async Task RendererContext(RendererContextArgs args)
    {
        Vector2 sizeOnScreen = new((float)args.width, (float)args.height);
        if (!(sizeOnScreen.X <= 0f) || !(sizeOnScreen.Y <= 0f))
        {
            UIDrawCollector collector = args.Collector;
            if (
                await rm.GetResource<ResourceSetArg>("Timeline.Game.Assets.Textures.Cursor.png")
                    is ResourceSetArg { ResourceSet: var resourceSet, Texture: var texture }
                && resourceSet != null
            )
            {
                Vertex vertex5 = new Vertex(
                    new Vector2(0f, 0f),
                    Color,
                    new Coord2(default(Vector2), new Vector2(0f, 0f)),
                    texture,
                    resourceSet,
                    1f
                );
                Vertex vertex6 = new Vertex(
                    new Vector2((float)args.height, 0f),
                    Color,
                    new Coord2(default(Vector2), new Vector2(1f, 0f)),
                    texture,
                    resourceSet,
                    1f
                );
                Vertex vertex7 = new Vertex(
                    new Vector2(0f, (float)args.width),
                    Color,
                    new Coord2(default(Vector2), new Vector2(0f, 1f)),
                    texture,
                    resourceSet,
                    1f
                );
                Vertex vertex8 = new Vertex(
                    new Vector2((float)args.height, (float)args.width),
                    Color,
                    new Coord2(default(Vector2), new Vector2(1f, 1f)),
                    texture,
                    resourceSet,
                    1f
                );
                collector.DrawVertex(new Vertex[3] { vertex5, vertex6, vertex7 }, this);
                collector.DrawVertex(new Vertex[3] { vertex6, vertex7, vertex8 }, this);
            }
        }
    }
}
