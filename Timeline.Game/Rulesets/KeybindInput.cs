using System.Collections.Concurrent;
using Line.Framework.IO;

namespace Timeline.Game.Rulesets;

public class KeybindInput : IDisposable
{
    public virtual KeybindItem KeybindItem { get; }
    protected virtual ConcurrentDictionary<KeyCode, string> PressedKey { get; } = [];
    protected virtual List<string> VirtualKeys { get; } = [];
    public virtual List<string> PressedKeys => [.. VirtualKeys];
    protected virtual InputManager IM { get; }
    protected virtual Action DoPressEvent { get; }
    protected virtual Action DoReleaseEvent { get; }
    public virtual event Action<string> OnKeyPress;
    public virtual event Action<string> OnKeyRelease;

    public KeybindInput(InputManager input)
    {
        IM = input;
        IM.KeyDown += OnPressKey;
        IM.KeyUp += OnReleaseKey;
    }
    protected virtual void OnPressKey(KeyCode key)
    {
        string id = Guid.NewGuid().ToString();
        if (!(KeybindItem.Keys?.Value.Contains(key) ?? false)) return;
        if (PressedKey.Count != 0 && !KeybindItem.MultiInput) return;
        if (PressedKey.TryGetValue(key, out _)) return;
        PressedKey.TryAdd(key, id);
        Press(id);
    }
    public virtual void Press(string id)
    {
        if (VirtualKeys.Count != 0 && !KeybindItem.MultiInput) return;
        VirtualKeys.Add(id);
        OnKeyPress?.Invoke(id);
    }
    public virtual void Release(string id)
    {
        if (!VirtualKeys.Contains(id)) return;
        VirtualKeys.Remove(id);
        OnKeyRelease?.Invoke(id);
    }
    protected virtual void OnReleaseKey(KeyCode key)
    {
        if (!PressedKey.TryGetValue(key, out _)) return;
        PressedKey.TryRemove(key, out var g);
        Release(g);
    }
    public void Dispose()
    {
        IM?.KeyDown -= OnPressKey;
        IM?.KeyUp -= OnReleaseKey;
    }
}