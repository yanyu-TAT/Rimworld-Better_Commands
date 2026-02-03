using System;
using Verse;

namespace BetterCommands.Settings
{
    public enum GroupShortcutMode
    {
        [EnumDescription("兼容模式")]
        Compact,
        [EnumDescription("禁用模式")]
        Disable,
        [EnumDescription("冲突模式")]
        Conflict
    }

    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public EnumDescriptionAttribute(string description = null)
        {
            Description = description;
        }
    }

    public static class GroupSettingsUtility
    {
        public static GroupShortcutMode CurrentGroupShortcutMode =>
            BetterCommands.Core.BetterCommandsMod.CurrentGroupShortcutMode;

        public static bool ShouldHandleGroupShortcuts(int keyNum)
        {
            //Log.Message("[Better Commands] Checking group shortcut for keyNum: " + keyNum);
            var mode = CurrentGroupShortcutMode;
            //99表示Fn键，始终处理
            if ((keyNum >= 5 && keyNum <= 9) || keyNum == 0 || keyNum == 99) return true;

            if (keyNum >= 1 && keyNum <= 4)
            {
                return mode != GroupShortcutMode.Disable;
            }

            return false;
        }

        public static bool ShouldBlockSpeedControl(int keyNum, bool otherKeyPressed)
        {
            //传入的keyNum为99时表示Fn键，阻止所有原版快捷键
            var mode = CurrentGroupShortcutMode;

            if ((keyNum >= 1 && keyNum <= 4) || keyNum == 99)
            {
                switch (mode)
                {
                    case GroupShortcutMode.Compact:
                        return otherKeyPressed;
                    case GroupShortcutMode.Disable:
                        return false;
                    case GroupShortcutMode.Conflict:
                        return false;
                }
            }

            return false;
        }
    }
}
