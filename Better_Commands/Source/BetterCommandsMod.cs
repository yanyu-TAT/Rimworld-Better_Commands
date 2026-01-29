/*
 * 模组主类
 */

using HarmonyLib;
using HugsLib.Settings;
using RimWorld;
using Verse;
using BetterCommands.Settings;

namespace BetterCommands.Core
{
    public class BetterCommandsMod : HugsLib.ModBase
    {
        public static BetterCommandsMod Instance { get; private set; }

        public static Harmony harmonyInstance;

        public static SettingHandle<GroupShortcutMode> groupShortcutSettingHandle;

        public override string ModIdentifier => "yanyu.bettercommands";

        public BetterCommandsMod() : base()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            base.Initialize();

            harmonyInstance = new Harmony("yanyu.bettercommands");

            try
            {
                harmonyInstance.PatchAll();
                Log.Message("BetterCommands: Harmony补丁应用成功");
            }
            catch (System.Exception ex)
            {
                Log.Error($"BetterCommands: Harmony补丁应用失败: {ex}");
            }
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            Log.Message("BetterCommands: 添加设置项中。。。");
            RegisterSettings();
        }

        private void RegisterSettings()
        {
            groupShortcutSettingHandle = Settings.GetHandle<GroupShortcutMode>(
                "GroupShortcutMode",
                "编组快捷键模式",
                "控制数字键1~4相关的快捷键行为模式",
                GroupShortcutMode.Compact
            );

            groupShortcutSettingHandle.CustomDrawerHeight = 70f;
            groupShortcutSettingHandle.Description =
                "• 兼容模式: 组合键不切换游戏速度（推荐）\n" +
                "• 禁用模式: 1-4 键仅用于切换游戏速度，不触发编组功能（稳定）\n" +
                "• 冲突模式: 组合键同时触发编组和切换游戏速度";

            Log.Message("BetterCommands: 设置项添加成功");
        }

        public static GroupShortcutMode CurrentGroupShortcutMode => 
            groupShortcutSettingHandle?.Value ?? GroupShortcutMode.Compact;
    }
}

/*
 * 01/29 04:32 修复重影问题；快捷键冲突问题待修复
 * 01/29 16:07 并非快捷键问题，而是数组未初始化导致的错误，已修复；存档功能待测试；计划添加覆盖键位功能
 * 01/29 21:43 存档功能测试通过；成功添加设置选项，允许用户选择快捷键覆盖模式
 */