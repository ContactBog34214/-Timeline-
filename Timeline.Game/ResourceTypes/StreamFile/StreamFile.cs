using System.Text;
using Line.Framework.Types;

namespace Timeline.Game.ResourceTypes;

public class StreamFile : IAsyncDisposable, IDisposable
{
    internal Stream Stream { get; init; }
    public string Text { get; private set; }
    public Byte[] Bytes { get; private set; }

    public StreamFile(Stream stream)
    {
        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
        Stream = new MemoryStream();
        stream.CopyTo(Stream);
    }

    public async Task Load()
    {
        Bytes = new byte[Stream.Length];
        if (Stream.CanSeek)
            Stream.Seek(0, SeekOrigin.Begin);
        await Stream.ReadAsync(Bytes);
        Text = Encoding.UTF8.GetString(Bytes);
    }

    public void Dispose()
    {
        Stream?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Stream.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
