
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using BetterCommands.Settings;


namespace BetterCommands.Patches
{
    [HarmonyPatch(typeof(KeyBindingDef))]
    [HarmonyPatch(nameof(KeyBindingDef.KeyDownEvent), MethodType.Getter)]
    public static class Patch_KeyBindingDef_KeyDownEvent
    {
        public static bool Prefix(KeyBindingDef __instance, ref bool __result)
        {
            if (Current.Game == null) return true;

            GroupShortcutMode groupShortcutMode = GroupSettingsUtility.CurrentGroupShortcutMode;
            if (groupShortcutMode == GroupShortcutMode.Conflict) return true;

            KeyCode mainKey = __instance.MainKey;
            bool isNumberKey = mainKey >= KeyCode.Alpha0 && mainKey <= KeyCode.Alpha9;
            bool isFnKey = mainKey >= KeyCode.F1 && mainKey <= KeyCode.F11;
            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (isNumberKey)
            {
                int keyNum = mainKey - KeyCode.Alpha0;
                if (GroupSettingsUtility.ShouldBlockSpeedControl(keyNum, ctrlPressed, shiftPressed))
                {
                    __result = false;
                    return false;
                }
            }

            if (isFnKey)
            {
                int keyNum = 99; //99 表示 F1~F11
                if (GroupSettingsUtility.ShouldBlockSpeedControl(keyNum, ctrlPressed, shiftPressed))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}

