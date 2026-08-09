using Line.Framework.Resource;

namespace Timeline.Game.ResourceTypes;

public class RStreamFile : IResource
{
    public object GetHandle()
    {
        return file;
    }

    private StreamFile file;
    internal Stream Stream { get; init; }

    public async Task Load()
    {
        file = new(Stream);
        await file.Load();
    }

    public async Task Release()
    {
        var t = file?.DisposeAsync();
        if (t.HasValue)
            await t.Value;
        file = null;
    }

    public bool IsLoaded => file != null;

    public void Dispose()
    {
        Stream?.Dispose();
        Release().GetAwaiter().GetResult();
    }
}
