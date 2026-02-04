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
        public static SettingHandle<bool> autoDraftOptionHandle;

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
                "BetterCommands.GroupShortcutMode",
                "BetterCommands.GroupShortcutModeTitle".Translate(),
                "BetterCommands.GroupShortcutModeDescription".Translate(),
                GroupShortcutMode.Compact
            );
            groupShortcutSettingHandle.CustomDrawerHeight = 70f;

            autoDraftOptionHandle = Settings.GetHandle<bool>(
                "BetterCommands.AutoDraftOption",
                "BetterCommands.AutoDraftOptionTitle".Translate(),
                "BetterCommands.AutoDraftOptionDescription".Translate(),
                false
            );
            autoDraftOptionHandle.CustomDrawerHeight = 70f;

            Log.Message("BetterCommands: 设置项添加成功");
        }

        public static GroupShortcutMode CurrentGroupShortcutMode => 
            groupShortcutSettingHandle?.Value ?? GroupShortcutMode.Compact;
        public static bool CurrentAutoDraftOption => 
            autoDraftOptionHandle?.Value ?? false;
    }

    public class Helper
    {
        public static TaggedString Translate(string key, params (string, object)[] args)
        {
            String str = key.Translate();
            if (args == null || args.Length == 0)
            {
                return str;
            }

            NamedArgument[] arguments = args.Select(t => new NamedArgument(t.Item2, t.Item1)).ToArray();
            return str.Formatted(arguments);
        }
    }
}

/* Todo List:
 * - [x] 实现编组保存与选中功能
 * - [x] 实现屏幕位置编组（编屏）功能
 * - [x] 实现行军指令
 * - [x] 创建行军JobDef
 * - [x] 创建行军工作类
 * - [x] 添加行军按钮
 * - [x] 测试行军功能
 * - [x] 修复检测逻辑
 * - [x] 测试检测逻辑
 * - [x] 修复恢复移动逻辑
 * - [x] 测试恢复移动逻辑
 * - [x] 绘制行军按钮icon
 * - [ ] 添加行军快捷键
 * - [ ] 加入近战行军索敌逻辑
 * - [x] 添加选中单位移出编组的功能
 * - [x] 编写文本本地化逻辑
 * - [x] 添加自动征召选项
 * - [ ] 添加编组可视化ui
 */

/* Develop Log:
 * 01/29 04:32 修复重影问题；快捷键冲突问题待修复
 * 01/29 16:07 并非快捷键问题，而是数组未初始化导致的错误，已修复；存档功能待测试；计划添加覆盖键位功能
 * 01/29 21:43 存档功能测试通过；成功添加设置选项，允许用户选择快捷键覆盖模式
 * 02/03 00:21 实现屏幕位置编组及存档编组功能并测试通过
 * 02/03 20:47 初步实现了行军功能并添加了按钮，测试中殖民者在遇敌时未能正常攻击，推测是检测逻辑问题，待修复
 * 02/03 21:09 经分析，是检测计时器设置有误，已修复。新的测试中发现殖民者在受到攻击后不会再次前进，待修复
 * 02/03 22:17 新测试发现被发现的敌人移出攻击范围外时殖民者不会继续移动，待修复
 * 02/04 00:00 发现的问题基本均已修复且测试通过，待后续进行更多测试
 * 02/04 01:21 应用户要求添加了移出编组功能，测试通过
 * 02/04 01:50 绘制并添加了行军按钮icon，经测试保证了近战殖民者行军不会出错
 * 02/04 16:04 添加了本地化翻译，添加了可配置的编组自动征召选项
 * 02/05 03:18 修正了行军修改目标地点的逻辑，测试通过
 */

/* Develop Plan:
 * 人员快捷编组   - Done
 * 屏幕快捷编组   - Done
 * 移动并攻击     - Done
 * 添加翻译文件
 * 编组管理UI
 * 就近寻找掩体
 */