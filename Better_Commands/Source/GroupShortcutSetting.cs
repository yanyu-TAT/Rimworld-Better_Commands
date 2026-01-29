using System;

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
            var mode = CurrentGroupShortcutMode;

            if ((keyNum >= 5 && keyNum <= 9) || keyNum == 0) return true;

            if (keyNum >= 1 && keyNum <= 4)
            {
                return mode != GroupShortcutMode.Disable;
            }

            return false;
        }

        public static bool ShouldBlockSpeedControl(int keyNum, bool ctrlPressed, bool shiftPressed)
        {
            var mode = CurrentGroupShortcutMode;

            if (keyNum >= 1 && keyNum <= 4)
            {
                switch (mode)
                {
                    case GroupShortcutMode.Compact:
                        return ctrlPressed || shiftPressed;
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
