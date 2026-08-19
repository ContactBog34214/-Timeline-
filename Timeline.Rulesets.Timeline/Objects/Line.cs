using System.Collections.Concurrent;
using System.Numerics;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using Timeline.Game;
using Timeline.Game.Maths;

namespace Timeline.Rulesets.Timeline.Objects;

public class Line : IDuringTime
{
    public double Time { get; set; }
    [JsonIgnore]
    public double DuringTime => Length / SpeedPerSecInPixels;
    public double SpeedPerSecInPixels { get; set; } = 128;
    public float Length { get; private set; } = 0;
    public ControlNode[] ControlNodes
    {
        get; set
        {
            if (value == null) return;
            field = value;
            BuildCache();
        }
    } = [];

    [JsonIgnore]
    private Vector2[] NodeCache { get; set; } = [];
    [JsonIgnore]
    private ConcurrentDictionary<float, Vector2> ProgressCache { get; } = [];
    public Vector2 Position { get; set; } = new();
    public Vector2 GetPositionOnLine(float Progress)
    {
        if (NodeCache == null && (NodeCache?.Length ?? 0) <= 1)
            return Position;
        if (ProgressCache.TryGetValue(Progress, out var v)) return v + Position;
        Vector2 ps = InterpolationTool.Linear(NodeCache, Progress);
        ProgressCache.TryAdd(Progress, ps);
        return ps + Position;
    }
    public void BuildCache()
    {
        int SimpleNum = 0;
        List<int> lg = [];
        foreach (var i in ControlNodes)
        {
            float m = 1.5f;
            if (i.Mode == NodeMode.Circle) m = 5;
            var n = GetSimpleNum([i.StartPosition, .. i.Position], m);
            SimpleNum += n;
            lg.Add(n);
        }
        Vector2[] newCache = new Vector2[SimpleNum + 1];
        int ptr = 0;
        int r = 0;
        foreach (var i in ControlNodes)
        {
            var sm = lg[r];
            Vector2[] dots = [i.StartPosition, .. i.Position];
            for (int p = 0; p < sm; p++)
            {
                float progress = (float)p / sm;
                Vector2 pos = new();
                switch (i.Mode)
                {
                    case NodeMode.Linear:
                        pos = InterpolationTool.Linear(dots, progress);
                        break;

                    case NodeMode.QuadraticEaseIn:
                        pos = InterpolationTool.QuadraticEaseIn(dots[0], dots.Last(), progress);
                        break;
                    case NodeMode.QuadraticEaseOut:
                        pos = InterpolationTool.QuadraticEaseOut(dots[0], dots.Last(), progress);
                        break;
                    case NodeMode.QuadraticEaseInOut:
                        pos = InterpolationTool.QuadraticEaseInOut(dots[0], dots.Last(), progress);
                        break;

                    case NodeMode.CubicEaseIn:
                        pos = InterpolationTool.CubicEaseIn(dots[0], dots.Last(), progress);
                        break;
                    case NodeMode.CubicEaseOut:
                        pos = InterpolationTool.CubicEaseOut(dots[0], dots.Last(), progress);
                        break;
                    case NodeMode.CubicEaseInOut:
                        pos = InterpolationTool.CubicEaseInOut(dots[0], dots.Last(), progress);
                        break;

                    case NodeMode.Bezier:
                        pos = InterpolationTool.Bezier(dots, progress);
                        break;
                    case NodeMode.Circle:
                        if (dots.Length == 1)
                            pos = dots[0];
                        else if (dots.Length == 2)
                            pos = InterpolationTool.Linear(dots, progress);
                        else
                            pos = InterpolationTool.GetPointOnPerfCircle(dots[0], dots[1], dots[2], progress);
                        break;
                    default:
                        goto case NodeMode.Linear;
                }
                newCache[ptr] = pos;
                ptr++;
            }
            r++;
        }
        newCache[^1] = ControlNodes.Last().Position.Last();
        float l = 0;
        for (int i = 0; i < newCache.Length - 1; i++)
            l += Vector2.Distance(newCache[0], newCache[1]);
        NodeCache = newCache;
        Length = l;
        ProgressCache.Clear();
    }
    public static int GetSimpleNum(Vector2[] Nodes, float Multiple)
    {
        var Dis = 0f;
        for (int i = 0; i < Nodes.Length - 1; i++)
        {
            Dis += Vector2.Distance(Nodes[0], Nodes[1]);
        }
        return (int)(Dis / 20 * Multiple) + 2;
    }
}