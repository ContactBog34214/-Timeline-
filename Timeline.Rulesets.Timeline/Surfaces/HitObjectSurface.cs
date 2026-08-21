using System.Collections.Concurrent;
using System.Numerics;
using Line.Framework.Graphics;
using Line.Framework.Resource;
using Line.Framework.Resource.Graphic;
using Line.Framework.Types;
using Line.Framework.UI;
using Timeline.Game.Maths;
using Timeline.Game.Rulesets;
using Timeline.Rulesets.Timeline.DifficulySettings;
using Timeline.Rulesets.Timeline.HitObjects;

namespace Timeline.Rulesets.Timeline.Surfaces;

public class HitObjectSurface(Timeline rs, GameSession gs) : UIWidget
{
    private readonly Timeline Ruleset = rs;
    private readonly GameSession GameSession = gs;
    public override async Task RendererContext(RendererContextArgs args)
    {
        var collector = args.Collector;
        //计算最早显示时间
        var DT = GameSession.DifficultySettings.LastOrDefault(c => c is Distance);
        decimal t = Math.Clamp(DT?.Value ?? 0, DT?.MinixmumValue ?? 0, DT?.MaximumValue ?? 1);
        double FadeInSec = (double)(3 - t * 0.275m);
        double Earliest = GameSession.Time - FadeInSec;

        //计算物件大小
        var OS = GameSession.DifficultySettings.LastOrDefault(c => c is ObjectSize);
        decimal s = Math.Clamp(OS?.Value ?? 0, OS?.MinixmumValue ?? 0, OS?.MaximumValue ?? 1);
        decimal Scale = 1 - s * 0.08m;
        float LineSize = (float)(Scale * 13);

        //筛选显示控件
        var lines = GameSession.Lines
            .Where(c => Earliest <= c.Time &&
            c.Time + c.DuringTime <= GameSession.Time + FadeInSec * 3 + (Ruleset.RulesetConfigs as RulesetConfig)?.HitAnimationMsLength)
            .OrderBy(c => -c.Time);
        var hitObj = GameSession.HitObjects
            .Where(
                c => Earliest <= c.Time &&
                (
                    Equals(c.HitResult, default) ||
                    c.HitTime + (Ruleset.RulesetConfigs as RulesetConfig)?.HitAnimationMsLength >= GameSession.Time
                )
            );

        //绘制小节
        async Task DrawLine(Objects.Line line)
        {
            //绘制线条
            var r = LineSize / 2;
            var num = line.SimpleNum;
            Vector2[] verticesPos = new Vector2[num * 2 + 2];
            int ptr = 0;
            Vector2 last = new();
            for (int i = 0; i <= num; i++)
            {
                float p = (float)i / num;
                double now = line.Time + line.DuringTime * p;
                if (now > FadeInSec * 3 + GameSession.Time) break;
                if (now <= GameSession.Time) continue;
                Vector2 pos = line.GetPositionOnLine(p);
                float ag = InterpolationTool.GetAngle(pos - last);
                last = pos;
                Vector2 ofs = InterpolationTool.Rotate(new(r, 0), ag);

                verticesPos[ptr] = line.Position - ofs;
                verticesPos[ptr + 1] = line.Position + ofs;

                ptr += 2;
            }

            //转换
            var LineBackgroundImage = await Ruleset.RuleSetResources.GetResource<ResourceSetArg>("LineBackground");
            Vertex[] vertices = new Vertex[ptr];
            float lo = (float)Math.Clamp(1d - (line.Time - GameSession.Time) / FadeInSec, 0f, 1f);
            RgbaFloat cl = line.Color *
                new RgbaFloat(1f, 1f, 1f, lo);
            Parallel.For(0, vertices.Length, i =>
            {
                if (i >= verticesPos.Length || i >= vertices.Length) return;
                Vector2 ps = verticesPos[i];
                Vector2 VertexPos = new(
                    (640 + ps.X) / 1280f,
                    1 - (480 + ps.Y) / 960f
                    );
                Vector2 p = VertexPos * new Vector2((float)args.width, (float)args.height);
                vertices[i] = new(
                    p,
                    cl,
                    new(new(), VertexPos),
                    LineBackgroundImage?.Texture,
                    LineBackgroundImage?.ResourceSet,
                    1
                );
            });

            //提交
            for (int i = 0; i < vertices.Length - 2; i++)
            {
                if (i + 2 >= vertices.Length) break;
                if (vertices[i] == null) break;
                if (vertices[i + 1] == null) break;
                if (vertices[i + 2] == null) break;
                collector.DrawVertex(
                    [
                        vertices[i],
                        vertices[i + 1],
                        vertices[i + 2]
                    ],
                    this
                    );
            }

            //绘制物件
            var Queue = hitObj
                .Where(
                    c => c.Position.X == line.LineID &&
                    (c.Time <= GameSession.Time + FadeInSec * 2.5 || !Equals(c.HitResult, default)));
            var TickImg = await Ruleset.RuleSetResources.GetResource<ResourceSetArg>("Tick");
            var DelayImg = await Ruleset.RuleSetResources.GetResource<ResourceSetArg>("Delay");
            var MemoryImg = await Ruleset.RuleSetResources.GetResource<ResourceSetArg>("Memory");

            ConcurrentDictionary<IHitObject, Vertex[]> RC = new();
            var cs = (float)(90 * Scale);
            await Parallel.ForEachAsync(Queue, async (i, _) =>
            {
                float spg = (float)((i.Time - line.Time) / line.DuringTime);
                Vector2 pos = line.GetPositionOnLine(spg);
                float epg = (float)((i.Time + i.DuringTime - line.Time) / line.DuringTime);
                if (i is Tick t || i is Memory m)
                {
                    float r = cs / 2;
                    float ang = InterpolationTool.GetAngle(
                        pos - line.GetPositionOnLine(spg - 2f / line.Length)
                    );
                    Vector2 tl = InterpolationTool.Rotate(new(-r, r), ang);
                    Vector2 tr = InterpolationTool.Rotate(new(r, r), ang);
                    Vector2 bl = InterpolationTool.Rotate(new(-r, -r), ang);
                    Vector2 br = InterpolationTool.Rotate(new(r, -r), ang);
                    Vertex[] c = new Vertex[4];
                    int ptr = 0;
                    (Vector2, Vector2)[] vectors = [new(tl, new(0, 0)), new(tr, new(1, 0)), new(bl, new(0, 1)), new(br, new(1, 1))];
                    ResourceSetArg Use = i is Tick ? TickImg : MemoryImg;
                    foreach (var item in vectors)
                    {
                        Vector2 FPos = item.Item1 + line.Position + pos;
                        FPos += new Vector2(640, 480);
                        FPos /= new Vector2(1280, 960);
                        FPos.Y = 1 - FPos.Y;

                        FPos *= new Vector2((float)args.width, (float)args.height);
                        float o = 1;
                        if (GameSession.Time >= i.Time)
                            o = 1 - (float)Math.Clamp((GameSession.Time - i.Time) / 0.1f, 0, 1);
                        else if (i.Time - 3 * FadeInSec < GameSession.Time)
                            o = 1 - (float)Math.Clamp((i.Time - GameSession.Time) / (3 * FadeInSec), 0, 1);
                        o *= lo;

                        RgbaFloat cl = line.Color *
                        new RgbaFloat(1, 1, 1, o);
                        c[ptr] = new(FPos, cl, new(new(), item.Item2), Use.Texture, Use.ResourceSet, 1);
                        ptr++;
                    }
                    RC.TryAdd(i, [c[0], c[1], c[2], c[1], c[2], c[3]]);
                }
                else if (i is Delay d)
                {
                    Vector2 imgSize = new(DelayImg?.Texture.Width ?? 1, DelayImg?.Texture.Height ?? 1);
                    float HeadR = Math.Min(imgSize.X, imgSize.Y) / 2;
                    float r = cs / 2;
                    float ang = InterpolationTool.GetAngle(
                        pos - line.GetPositionOnLine(spg - 2f / line.Length)
                    );
                    int SimpleNum = (int)(Math.Clamp(d.DuringTime / line.DuringTime * 1.35, 0, 1) * line.SimpleNum);
                    (Vector2 pos, Vector2 uv) LastL = new();
                    (Vector2 pos, Vector2 uv) LastR = new();
                    List<(Vector2, Vector2)> vertsPos = [];
                    double startTime = Math.Clamp(GameSession.Time, i.Time, i.Time + i.DuringTime);
                    double endTime = Math.Clamp(GameSession.Time + FadeInSec * 3, i.Time, i.Time + i.DuringTime);

                    //Head
                    {
                        spg = (float)((startTime - line.Time) / line.DuringTime);
                        Vector2 tl = InterpolationTool.Rotate(new(-r, 0), ang) + pos;
                        Vector2 tr = InterpolationTool.Rotate(new(r, 0), ang) + pos;
                        Vector2 bl = InterpolationTool.Rotate(new(-r, -r), ang) + pos;
                        Vector2 br = InterpolationTool.Rotate(new(r, -r), ang) + pos;
                        vertsPos.Add(new(tl, new(0, imgSize.Y - HeadR)));
                        vertsPos.Add(new(tr, new(imgSize.X, imgSize.Y - HeadR)));
                        vertsPos.Add(new(bl, new(0, imgSize.Y)));
                        vertsPos.Add(new(br, new(imgSize.X, imgSize.Y)));
                        LastL = new(tl, new(0, imgSize.Y - HeadR));
                        LastR = new(tr, new(imgSize.X, imgSize.Y - HeadR));
                    }

                    //Body
                    float ScaleArea = imgSize.Y - 2 * HeadR;
                    Vector2 LastPos = pos;
                    double h = (startTime - i.Time) / i.DuringTime;
                    double e = (endTime - i.Time) / i.DuringTime;
                    for (int idx = 1; idx <= SimpleNum; idx++)
                    {
                        float DelayProgress = (float)idx / SimpleNum;
                        if (h > DelayProgress) continue;
                        if (e < DelayProgress) break;
                        float LineProgress = (float)((i.Time + DelayProgress * i.DuringTime - line.Time) / line.DuringTime);
                        Vector2 NodePos = line.GetPositionOnLine(LineProgress);
                        float angle = InterpolationTool.GetAngle(NodePos - LastPos);
                        if (idx != SimpleNum)
                            LastPos = NodePos;
                        Vector2 LP = InterpolationTool.Rotate(new(-r, 0), angle);
                        Vector2 RP = InterpolationTool.Rotate(new(r, 0), angle);
                        float hP = imgSize.Y - HeadR - ScaleArea * DelayProgress;
                        (Vector2, Vector2) FLP = new(LP + NodePos, new(0, hP));
                        (Vector2, Vector2) FRP = new(RP + NodePos, new(imgSize.X, hP));

                        vertsPos.AddRange([FLP, FRP, LastL]);
                        vertsPos.AddRange([FRP, LastL, LastR]);

                        LastL = FLP;
                        LastR = FRP;
                    }

                    //Tail
                    {
                        epg = (float)((endTime - line.Time) / line.DuringTime);
                        Vector2 ps = line.GetPositionOnLine(epg);
                        ang = InterpolationTool.GetAngle(ps - LastPos);
                        Vector2 tl = InterpolationTool.Rotate(new(-r, 0), ang) + ps;
                        Vector2 tr = InterpolationTool.Rotate(new(r, 0), ang) + ps;
                        Vector2 bl = InterpolationTool.Rotate(new(-r, -r), ang) + ps;
                        Vector2 br = InterpolationTool.Rotate(new(r, -r), ang) + ps;
                        vertsPos.Add(new(tl, new(0, imgSize.Y - HeadR)));
                        vertsPos.Add(new(tr, new(imgSize.X, imgSize.Y - HeadR)));
                        vertsPos.Add(new(bl, new(0, imgSize.Y)));
                        vertsPos.Add(new(br, new(imgSize.X, imgSize.Y)));
                        LastL = new(tl, new(0, imgSize.Y - HeadR));
                        LastR = new(tr, new(imgSize.X, imgSize.Y - HeadR));
                    }

                    //转换
                    Vertex[] c = new Vertex[vertsPos.Count];
                    {
                        int ptr = 0;
                        foreach (var item in vertsPos)
                        {
                            Vector2 FPos = item.Item1 + line.Position + pos;
                            FPos += new Vector2(640, 480);
                            FPos /= new Vector2(1280, 960);
                            FPos.Y = 1 - FPos.Y;

                            FPos *= new Vector2((float)args.width, (float)args.height);
                            float o = 1;
                            if (GameSession.Time >= i.Time + i.DuringTime)
                                o = 1 - (float)Math.Clamp((GameSession.Time - i.Time) / 0.1f, 0, 1);
                            else if (i.Time - 3 * FadeInSec < GameSession.Time)
                                o = 1 - (float)Math.Clamp((i.Time - GameSession.Time) / (3 * FadeInSec), 0, 1);
                            o *= lo;

                            RgbaFloat cl = line.Color *
                            new RgbaFloat(1, 1, 1, o);
                            c[ptr] = new(FPos, cl, new(item.Item2, new()), DelayImg?.Texture, DelayImg?.ResourceSet, 1);
                            ptr++;
                        }
                    }

                    //提交
                    {
                        Vertex[] s = new Vertex[c.Length * 3 - 6];
                        int ptr = 0;
                        for (int idx = 0; idx < c.Length - 2; idx++)
                        {
                            s[ptr] = c[idx];
                            s[ptr + 1] = c[idx + 1];
                            s[ptr + 2] = c[idx + 2];
                            ptr += 3;
                        }
                        RC.TryAdd(i, s);
                    }
                }
            });

            var vs = RC
                .Where(c => c.Value != default)
                .OrderBy(c => -c.Key.Time)
                .Select(c => c.Value);
            foreach (var i in vs)
                collector.DrawVertex(
                    i,
                    this
                    );
        }

        foreach (var i in lines)
        {
            await DrawLine(i);
        }
    }
}