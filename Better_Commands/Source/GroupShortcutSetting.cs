using System;
using System.ComponentModel;


namespace BetterCommands.Settings
{
    public enum GroupShortcutMode
    {
        [Description("BetterCommands.GroupShortcutCompactMode")]
        Compact,        //shift + 数字选中编组，双击跳转至中心
        [Description("BetterCommands.GroupShortcutDisableMode")]
        Disable,        //禁用数字1~4的组合快捷键，其他数字单键按下后选中编组，双击跳转至中心
        [Description("BetterCommands.GroupShortcutConflictMode")]
        Conflict        //禁用数字1~4的原版功能，所有数字单键按下后选中编组，双击跳转至中心
    }

    public static class GroupSettingsUtility
    {
        public static GroupShortcutMode CurrentGroupShortcutMode =>
            BetterCommands.Core.BetterCommandsMod.CurrentGroupShortcutMode;

        public static bool ShouldHandleGroupShortcuts(int keyNum)
        {
            //Log.Message("[Better Commands] Checking group shortcut for keyNum: " + keyNum);
            var mode = CurrentGroupShortcutMode;
            if ((keyNum >= 5 && keyNum <= 9) || keyNum == 0 ) return true;

            //99表示Fn键，依据快捷键模式决定是否处理
            if (keyNum >= 1 && keyNum <= 4 || keyNum == 99)
            {
                return mode != GroupShortcutMode.Disable;
            }

            return false;
        }

        public static bool ShouldHandleNumberOnly(int keyNum)
        {
            return CurrentGroupShortcutMode switch
            {
                GroupShortcutMode.Compact => false,
                GroupShortcutMode.Disable => !(keyNum >= 1 && keyNum <= 4),
                GroupShortcutMode.Conflict => true,
                _ => false,
            };
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
