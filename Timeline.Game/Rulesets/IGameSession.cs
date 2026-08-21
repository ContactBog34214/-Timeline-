using System.Collections.Concurrent;
using Line.Framework.IO;
using Timeline.Game.Beatmap;
using Timeline.Game.Gaming;

namespace Timeline.Game.Rulesets;

public interface IGameSession<T> : IHasTime, IAsyncDisposable where T : IRuleset
{
        /// <summary>
        /// 创建游戏会话显示器
        /// </summary>
        void CreateGamingClient(InputManager im);
        /// <summary>
        /// 游戏会话显示器
        /// </summary>
        GamingScreen ActiveGamingScreen { get; }
        /// <summary>
        /// 所有打击物件
        /// </summary>
        IHitObject[] HitObjects { get; }
        /// <summary>
        /// 获取准度
        /// </summary>
        /// <param name="获取模式"></param>
        /// <returns>精准度(0%~105%)</returns>
        decimal GetAccuracy(GetMode Mode);
        /// <summary>
        /// 获取分数
        /// </summary>
        /// <param name="获取模式"></param>
        /// <returns>分数(0~1066625)</returns>
        decimal GetScore(GetMode Mode);
        /// <summary>
        /// 连击数
        /// </summary>
        int Combo { get; }
        /// <summary>
        /// 最大连击数
        /// </summary>
        int MaxCombo { get; }
        /// <summary>
        /// 所有判定的判定数
        /// </summary>
        ConcurrentDictionary<HitLevel, long> NumOfAllHitLevels { get; }
        /// <summary>
        /// 判定物件
        /// </summary>
        /// <param name="物件"></param>
        /// <param name="判定发生时间"></param>
        /// <returns>判定结果</returns>
        Task<HitLevel> JudgeObject(IHitObject Object, double time);
        /// <summary>
        /// 血量(如果有)
        /// </summary>
        decimal Health { get; }
        /// <summary>
        /// 暂停中
        /// </summary>
        bool Pause { get; set; }
        /// <summary>
        /// 谱面难度设定
        /// </summary>
        IDifficultySetting[] DifficultySettings { get; }
        /// <summary>
        /// 主规则集
        /// </summary>
        T Ruleset { get; }
        /// <summary>
        /// 时间轴设定
        /// </summary>
        TimeSet[] TimeSets { get; }
}

public enum GetMode
{
        Minixmum = -1,
        Default = 0,
        Maximum = 1,
}