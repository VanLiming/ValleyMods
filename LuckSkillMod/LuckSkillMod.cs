using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

namespace LuckSkillMod
{
    public class LuckSkillMod : Mod
    {
        private LuckSkillManager LuckManager;
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.LuckManager = new LuckSkillManager(this.Monitor, this.Helper, this.Config);

            // 注册命令
            helper.ConsoleCommands.Register("luck_info", "显示当前幸运技能信息", this.ShowLuckInfo);
            helper.ConsoleCommands.Register("luck_add_exp", "增加幸运技能经验值（调试用）", this.AddLuckExp);
            helper.ConsoleCommands.Register("luck_set_level", "设置幸运技能等级（调试用）", this.SetLuckLevel);

            // 注册事件
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.Player.LevelChanged += this.OnLevelChanged;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.LuckManager.LoadLuckData();
            this.Monitor.Log("幸运技能MOD已加载", LogLevel.Info);
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            this.LuckManager.SaveLuckData();
        }

        private void OnLevelChanged(object sender, LevelChangedEventArgs e)
        {
            // 玩家升级时的处理
            this.LuckManager.OnSkillLevelChanged(e.Skill, e.NewLevel);
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            // 物体列表改变时的处理（用于检测物品捡取）
            this.LuckManager.OnObjectListChanged(e.Added, e.Removed);
        }

        private void ShowLuckInfo(string command, string[] args)
        {
            this.LuckManager.ShowLuckInfo();
        }

        private void AddLuckExp(string command, string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out int exp))
            {
                this.LuckManager.AddExperience(exp);
                this.Monitor.Log($"增加了 {exp} 点幸运技能经验值", LogLevel.Info);
            }
            else
            {
                this.Monitor.Log("用法: luck_add_exp <经验值>", LogLevel.Info);
            }
        }

        private void SetLuckLevel(string command, string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out int level))
            {
                if (level >= 0 && level <= 10)
                {
                    this.LuckManager.SetLevel(level);
                    this.Monitor.Log($"幸运技能等级已设置为 {level}", LogLevel.Info);
                }
                else
                {
                    this.Monitor.Log("技能等级必须在0-10之间", LogLevel.Info);
                }
            }
            else
            {
                this.Monitor.Log("用法: luck_set_level <0-10>", LogLevel.Info);
            }
        }
    }
}
