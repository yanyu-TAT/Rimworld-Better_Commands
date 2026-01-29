
using HarmonyLib;
using RimWorld;
using UnityEngine;
using BetterCommands.Settings;

namespace BetterCommands.Patches
{
    [HarmonyPatch(typeof(TimeControls))]
    [HarmonyPatch("DoTimeControlsGUI")]
    public static class Patch_TimeControls_DoTimeControlsGUI
    {
        public static bool Prefix(Rect timerRect)
        {
            if (GroupSettingsUtility.CurrentGroupShortcutMode == GroupShortcutMode.Conflict) return true;

            if (Event.current != null && Event.current.isKey)
            {
                KeyCode keyCode = Event.current.keyCode;

                int keyNum = keyCode - KeyCode.Alpha0;
                bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (GroupSettingsUtility.ShouldBlockSpeedControl(keyNum, ctrlPressed, shiftPressed))
                {
                    Event.current.Use();
                    return false;
                }
            }
            return true;
        }
    }
}

