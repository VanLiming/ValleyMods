using System.Collections.Generic;

namespace LuckSkillMod
{
    public class ModConfig
    {
        /// <summary>每个技能等级的幸运奖励增加值</summary>
        public double LuckBonusPerLevel { get; set; } = 2.5;

        /// <summary>每个技能等级的价格倍数增加（百分比）</summary>
        public double PriceMultiplier { get; set; } = 0.05;

        /// <summary>每个技能等级的金品质概率增加（百分比）</summary>
        public int QualityChanceIncrease { get; set; } = 5;

        /// <summary>各项活动获得的经验值</summary>
        public Dictionary<string, int> ExpPerActivity { get; set; } = new Dictionary<string, int>
        {
            { "Fishing", 2 },
            { "Mining", 1 },
            { "Farming", 1 },
            { "TreasureFind", 5 }
        };
    }
}
