using System.Reflection;
using System.Runtime.Loader;
using Line.Framework.Resource;

namespace Timeline.Game.ResourceTypes.Assemblies;

public class RAssembly(Stream s) : IResource
{
    public bool IsLoaded => Assembly != null;

    private AssemblyLoadContext LoadContext { get; } = new("AssemblyResource", true);
    private Assembly Assembly { get; set; }
    private Stream stream = s;
    public void Dispose()
    {
        LoadContext.Unload();
        stream?.Dispose();
    }

    public object GetHandle()
    {
        return Assembly;
    }

    public async Task Load()
    {
        if (IsLoaded) return;
        Assembly = LoadContext.LoadFromStream(stream);
    }

    public async Task Release()
    {
        if (!IsLoaded) return;
        LoadContext.Unload();
    }
}

public class TAssembly : ResourceType
{
    public override async Task<IResource> Create(Stream stream)
    {
        return new RAssembly(stream);
    }
}