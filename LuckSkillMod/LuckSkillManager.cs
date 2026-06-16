using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckSkillMod
{
    public class LuckSkillManager
    {
        private readonly IMonitor Monitor;
        private readonly IModHelper Helper;
        private readonly ModConfig Config;

        // 幸运技能数据
        private int LuckLevel = 0;
        private int LuckExperience = 0;
        private const int MaxLevel = 10;
        private const int ExpPerLevel = 100;
        private const string DataKey = "LuckSkillData";

        public LuckSkillManager(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            this.Monitor = monitor;
            this.Helper = helper;
            this.Config = config;
        }

        public void LoadLuckData()
        {
            try
            {
                var data = this.Helper.Data.ReadSaveData<LuckSkillData>(DataKey);
                if (data != null)
                {
                    this.LuckLevel = Math.Min(data.Level, MaxLevel);
                    this.LuckExperience = data.Experience;
                    this.Monitor.Log($"幸运技能已加载 - 等级: {this.LuckLevel}, 经验: {this.LuckExperience}", LogLevel.Info);
                }
                else
                {
                    this.LuckLevel = 0;
                    this.LuckExperience = 0;
                    this.Monitor.Log("新游戏 - 幸运技能已初始化", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"加载幸运技能数据时出错: {ex}", LogLevel.Error);
            }
        }

        public void SaveLuckData()
        {
            try
            {
                var data = new LuckSkillData
                {
                    Level = this.LuckLevel,
                    Experience = this.LuckExperience
                };
                this.Helper.Data.WriteSaveData(DataKey, data);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"保存幸运技能数据时出错: {ex}", LogLevel.Error);
            }
        }

        public void AddExperience(int amount)
        {
            if (this.LuckLevel >= MaxLevel)
            {
                this.Monitor.Log("幸运技能已达到最高等级！", LogLevel.Info);
                return;
            }

            this.LuckExperience += amount;
            int levelUpsThisTurn = 0;

            while (this.LuckExperience >= ExpPerLevel && this.LuckLevel < MaxLevel)
            {
                this.LuckExperience -= ExpPerLevel;
                this.LuckLevel++;
                levelUpsThisTurn++;
            }

            if (levelUpsThisTurn > 0)
            {
                this.Monitor.Log($"\n████ 幸运技能提升！ ████\n您的幸运技能现在达到 {this.LuckLevel} 级！\n", LogLevel.Info);
                this.ShowLuckBenefit();
            }
            else if (amount > 0)
            {
                this.Monitor.Log($"✨ 获得 {amount} 点幸运技能经验值 ({this.LuckExperience}/{ExpPerLevel})", LogLevel.Info);
            }
        }

        public void SetLevel(int level)
        {
            this.LuckLevel = Math.Clamp(level, 0, MaxLevel);
            this.LuckExperience = 0;
            this.SaveLuckData();
        }

        public void ShowLuckInfo()
        {
            string info = $@"
╔════════════════════════════════════════════════════════╗
║       幸运技能信息                      ║
╠════════════════════════════════════════════════════════╣
║ 等级: {this.LuckLevel,2}/10
║ 经验: {this.LuckExperience,3}/{ExpPerLevel}
║ 进度: [{GetProgressBar(this.LuckExperience, ExpPerLevel)}] {(this.LuckExperience * 100 / ExpPerLevel)}%
╠════════════════════════════════════════════════════════╣
║ 当前奖励效果:
";

            // 显示当前等级的奖励
            double qualityBonus = (this.LuckLevel / 2.0) * this.Config.QualityChanceIncrease;
            double priceBonus = (this.LuckLevel / 2.0) * this.Config.PriceMultiplier * 100;

            info += $"║ • 金品质概率: +{qualityBonus:F1}%\n";
            info += $"║ • 售价提升: +{priceBonus:F1}%\n";
            info += "╚════════════════════════════════════════════════════════╝\n";

            this.Monitor.Log(info, LogLevel.Info);
        }

        public void ShowLuckBenefit()
        {
            double qualityBonus = (this.LuckLevel / 2.0) * this.Config.QualityChanceIncrease;
            double priceBonus = (this.LuckLevel / 2.0) * this.Config.PriceMultiplier * 100;

            this.Monitor.Log($"新增奖励: 金品质概率 +{qualityBonus:F1}%, 售价提升 +{priceBonus:F1}%", LogLevel.Info);
        }

        public void OnSkillLevelChanged(string skill, int newLevel)
        {
            // 其他技能升级时可以触发幸运技能经验
            if (skill != "Luck")
            {
                this.AddExperience(this.Config.ExpPerActivity["Farming"]);
            }
        }

        public void OnObjectListChanged(IEnumerable<KeyValuePair<Vector2, SObject>> added, IEnumerable<KeyValuePair<Vector2, SObject>> removed)
        {
            // 检测金品质物品
            foreach (var item in added)
            {
                if (item.Value.Quality == 4) // 金品质
                {
                    this.AddExperience(this.Config.ExpPerActivity["TreasureFind"]);
                }
            }
        }

        private string GetProgressBar(int current, int max, int length = 10)
        {
            int filled = (int)((double)current / max * length);
            return new string('█', filled) + new string('░', length - filled);
        }

        // 获取幸运技能的金币收益倍数
        public double GetPriceMultiplier()
        {
            return 1.0 + ((this.LuckLevel / 2.0) * this.Config.PriceMultiplier);
        }

        // 获取金品质物品的额外概率
        public int GetQualityBonus()
        {
            return (int)((this.LuckLevel / 2.0) * this.Config.QualityChanceIncrease);
        }

        public int GetCurrentLevel() => this.LuckLevel;
        public int GetCurrentExperience() => this.LuckExperience;
    }

    public class LuckSkillData
    {
        public int Level { get; set; }
        public int Experience { get; set; }
    }
}
