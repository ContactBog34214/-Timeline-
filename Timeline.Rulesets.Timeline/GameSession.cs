using System.Collections.Concurrent;
using Line.Framework.IO;
using Timeline.Game.Beatmap;
using Timeline.Game.Gaming;
using Timeline.Game.Rulesets;
using Timeline.Rulesets.Timeline.DifficulySettings;
using Timeline.Rulesets.Timeline.HitObjects;

namespace Timeline.Rulesets.Timeline;

public class GameSession : IGameSession<Timeline>, IHasVisualHitWindow
{
    public Objects.Line[] Lines { get; set; } = [];
    public GamingScreen? ActiveGamingScreen { get; set; }
    public IHitObject[] HitObjects { get; set; } = [];
    public VisualHitWindow[] HitWindows { get; private set; } = [];
    public int Combo { get; set; } = 0;
    public int MaxCombo { get; set; } = 0;
    public ConcurrentDictionary<HitLevel, long> NumOfAllHitLevels { get; } = new();
    public decimal Health { get; set; } = 0;
    public bool Pause { get; set; } = false;
    public IDifficultySetting[] DifficultySettings
    {
        get; set
        {
            if (value == null) return;
            field = value;
            ApplyDifficultySettings();
        }
    } = [];
    public void ApplyDifficultySettings()
    {
        //判定窗口
        {
            var HitwindowSetting = DifficultySettings.LastOrDefault(c => c is HitWindow) ?? new HitWindow();
            var final = Math.Clamp(
                HitwindowSetting.Value,
                HitwindowSetting.MinixmumValue,
                HitwindowSetting.MaximumValue
                );
            var Table = Timeline.HitLevels;
            double GetValAbs(double Source, double Scale) =>
                Math.Abs(Math.Max(0, Source - Scale * (double)final));

            //配置表
            ConcurrentDictionary<HitLevel, (double Source, double Scale)> Windows = [];
            Parallel.ForEach(Table, i =>
            {
                bool failed = false;
                (double Source, double Scale) cfg = default;

                switch (i.Name)
                {
                    case "Perfect":
                        cfg = new(25, 1.75);
                        break;
                    case "Just":
                        cfg = new(35, 1.5);
                        break;
                    case "Great":
                        cfg = new(50, 2);
                        break;
                    case "Good":
                        cfg = new(80, 4);
                        break;
                    case "Lost":
                        cfg = new(120, -10);
                        break;
                    default:
                        failed = true;
                        break;
                }
                if (!failed) Windows.TryAdd(i, cfg);
            });

            //更新
            var tmp = new VisualHitWindow[Windows.Count];
            var idx = 0;
            foreach (var i in Windows)
            {
                var t = GetValAbs(i.Value.Source, i.Value.Scale);
                tmp[idx] = new()
                {
                    HitLevel = i.Key,
                    Time = -t,
                    DuringTime = t,
                };
                idx++;
            }
            HitWindows = tmp;
        }
    }
    public double Time { get; set; } = 0;

    public required Timeline Ruleset { get; init; }

    public required TimeSet[] TimeSets { get; set; }

    public void CreateGamingClient(InputManager im)
    {
        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        ActiveGamingScreen?.Dispose();
    }

    public decimal GetAccuracy(GetMode Mode)
    {
        var Lvs = Timeline.HitLevels;
        ConcurrentDictionary<HitLevel, IEnumerable<IHitObject>> Levels = new();
        IEnumerable<IHitObject>? Def = default;
        HitLevel DefHitLevel = new() { Accuracy = 0 };
        Parallel.ForEach([.. Lvs, default], Lv =>
        {
            if (Equals(Lv, default))
            {
                //准度模式为"当前模式"时舍弃没判定的
                if (Mode == GetMode.Default) return;
                var ori = Lv;
                //最小或最大理论值替换
                Lv = Lvs.OrderBy(c => Mode == GetMode.Maximum ? c.Accuracy : -c.Accuracy).Last();
                Def = HitObjects.Where(c => Equals(c.HitResult, ori));
                DefHitLevel = Lv;
            }
            else Levels.TryAdd(Lv, HitObjects.Where(c => Equals(c.HitResult, Lv)));
        });

        //合并
        if (Def != null)
        {
            if (Levels.TryGetValue(DefHitLevel, out var t))
            {
                Levels.TryUpdate(DefHitLevel, [.. t, .. Def], t);
            }
            else
            {
                Levels.TryAdd(DefHitLevel, Def);
            }
        }

        //常规音符准度计算(100%)变量
        decimal p = 0;
        decimal tot = 0;
        //Memory音符准度计算(5%)变量
        decimal mp = 0;
        decimal mtot = 0;
        foreach (var i in Lvs)
        {
            if (!Levels.TryGetValue(i, out var item)) continue;
            //常规计算
            var c = item.Count();
            tot += c;
            p += c * i.Accuracy;
            //Memory音符奖励准度计算
            var MemoriesObject = item.Where(c => c is Memory);
            c = MemoriesObject.Count();
            mtot += c;
            mp += c * i.Accuracy * i.Accuracy;
        }
        if (mtot <= 0) mp = mtot = 1;
        if (tot <= 0) p = tot = 1;
        return (p / tot) + mp / mtot * 0.05m;
    }

    public decimal GetScore(GetMode Mode)
    {
        /**组成:1,000,000              =1,066,625
         *Acc  :  650,000*(acc^2)      =  716,625
         *Combo:  350,000*(Combo/Total)=  350,000
         */
        if (Mode == GetMode.Default) Mode = GetMode.Minixmum;
        var acc = GetAccuracy(Mode);
        var unHited = HitObjects.Where(c => Equals(c.HitResult, default));
        var maxCombo = MaxCombo;
        if (Mode == GetMode.Maximum)
            maxCombo = Math.Max(MaxCombo, Combo + unHited.Count());
        return 650000m * acc * acc + 350000m * (maxCombo / HitObjects.Length);
    }

    public Task<HitLevel> JudgeObject(IHitObject Object, double time)
    {
        throw new NotImplementedException();
    }
}