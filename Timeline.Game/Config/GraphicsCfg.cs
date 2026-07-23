using Line.Framework.Graphics;

namespace Timeline.Game.Config;

public partial class GraphicsCfg:ConfigType
{
    public GraphicBackend GraphicBackend { get; set; } = GraphicBackend.OpenGL;
    public bool FullScreen { get; set; } = true;
    public bool ParallelRender { get; set; } = true;
    public bool VSync { get; set; } = true;
    public float FPSLimit { get; set; } = 1000;
}
