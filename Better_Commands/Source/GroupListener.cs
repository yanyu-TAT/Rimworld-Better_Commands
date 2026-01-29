/*
 * 监听shift + 数字
 * 监听ctrl  + 数字
 */

using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using BetterCommands.Settings;

namespace BetterCommands.Core
{
    [HarmonyPatch(typeof(Root))]
    [HarmonyPatch("Update")]
    public static class GroupListener
    {
        static void Postfix()
        {
            if (Current.Game == null)
                return;

            GroupData groupData = Current.Game?.GetComponent<GroupData>();
            if (groupData == null)
                return;

            for (int i = 0; i <= 9; i++)
            {
                KeyCode key = KeyCode.Alpha0 + i;

                if (Input.GetKeyDown(key))
                {
                    //Verse.Log.Message($"检测到按键: {key}");
                    if (GroupSettingsUtility.ShouldHandleGroupShortcuts(i)){
                        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                        Verse.Log.Message($"Ctrl: {ctrlPressed}, Shift: {shiftPressed}");

                        if (ctrlPressed && !shiftPressed)
                        {
                            Verse.Log.Message($"保存编组 {i}");
                            List<Pawn> selectedPawns = Find.Selector.SelectedPawns
                                .Where(p => p.Faction == Faction.OfPlayer)
                                .ToList();

                            if (selectedPawns.Count != 0)
                            {
                                groupData.CreateGroup(i, selectedPawns);
                                Messages.Message($"编组 {i} 已保存 ({selectedPawns.Count}个单位)", MessageTypeDefOf.TaskCompletion);
                            }
                            else                             {
                                Messages.Message("没有选中可编组单位", MessageTypeDefOf.RejectInput);
                            }
                            Event.current?.Use();
                            return;
                        }

                        if (shiftPressed && !ctrlPressed)
                        {
                            Verse.Log.Message($"选中编组 {i}");
                            groupData.SelectGroup(i);
                            Event.current?.Use();
                            return;
                        }
                    }
                }
            }
        }
    }
}