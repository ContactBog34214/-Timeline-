using System.Collections.Concurrent;
using Timeline.Game.Screen.Gaming;

namespace Timeline.Game.Rulesets;

public interface IGameSession : IHasTime, IAsyncDisposable
{
        /// <summary>
        /// 创建游戏会话显示器
        /// </summary>
        void CreateGamingScreen();
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
        int Combo { get; }
        int MaxCombo { get; }
        ConcurrentDictionary<HitLevel, long> NumOfAllHitLevels { get; }
        Task<HitLevel> JudgeObject(IHitObject Object, double time);
        decimal Health { get; }
        bool Pause { get; set; }
        IDifficultySetting[] DifficultySettings { get; }
}

public enum GetMode
{
        Minixmum = -1,
        Default = 0,
        Maximum = 1,
}