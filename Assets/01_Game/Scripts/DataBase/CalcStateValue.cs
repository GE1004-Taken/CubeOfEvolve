using System;

namespace Game.Utils
{
    public static class StateValueCalculator
    {
        /// <summary>
        /// レベルに応じて状態値（ステータス値）をスケーリングして返します。
        /// </summary>
        /// <param name="baseValue">基準となる値</param>
        /// <param name="currentLevel">現在のレベル</param>
        /// <param name="maxLevel">最大レベル</param>
        /// <param name="maxRate">最大増減率（例：0.5f → 最大+50%、-0.3f → 最大-30%）</param>
        /// <param name="exponent">成長カーブ（1.0fで線形、2.0fで指数カーブ）</param>
        /// <returns>スケーリングされた状態値</returns>
        public static float CalcStateValue(
            float baseValue,
            int currentLevel,
            int maxLevel = 5,
            float maxRate = 0.5f,
            float exponent = 1f)
        {
            if (currentLevel <= 1) return baseValue;
            if (currentLevel >= maxLevel) return baseValue * (1f + maxRate);

            float progress = MathF.Pow((currentLevel - 1f) / (maxLevel - 1f), exponent);
            return baseValue * (1f + maxRate * progress);
        }
    }
}
