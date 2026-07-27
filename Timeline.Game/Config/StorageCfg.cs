namespace Timeline.Game.Config;

public class StorageCfg : ConfigType
{
    public bool EnableCache { get; set; } = true;
    public bool EnableCompress { get; set; } = true;
    public ulong MaximumCacheSize { get; set; } = 1024 * 1024 * 64;
}
