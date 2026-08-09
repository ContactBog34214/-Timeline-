using Line.Framework.Resource;

namespace Timeline.Game.ResourceTypes;

public class TStreamFile : ResourceType
{
    public override async Task<IResource> Create(Stream stream)
    {
        var Stream = new MemoryStream();
        await stream.CopyToAsync(Stream);
        return new RStreamFile() { Stream = Stream };
    }
}
