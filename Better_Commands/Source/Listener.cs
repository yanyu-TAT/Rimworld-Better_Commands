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
    public static class Listener
    {
        static void Postfix()
        {
            if (Current.Game == null)
                return;

            GroupData groupData = Current.Game?.GetComponent<GroupData>();
            if (groupData == null)
                return;

            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // 检测是否按下了数字键 0-9
            for (int i = 0; i <= 9; i++)
            {
                KeyCode key = KeyCode.Alpha0 + i;

                if (Input.GetKeyDown(key))
                {
                    //Verse.Log.Message($"检测到按键: {key}");
                    if (GroupSettingsUtility.ShouldHandleGroupShortcuts(i)){
                        //Verse.Log.Message($"Ctrl: {ctrlPressed}, Shift: {shiftPressed}");

                        //ctrl + 数字键
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

                        //shift + 数字键
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

            // 检测是否按下了F1-F11 (F12由于与hugslib冲突，不检测)
            for (int i = 0;i < 11;i++)
            {
                KeyCode key = KeyCode.F1 + i;
                if (Input.GetKeyDown(key))
                {
                    Log.Message($"检测到按键: {key}");
                    if (GroupSettingsUtility.ShouldHandleGroupShortcuts(99))
                    {
                        //ctrl + F1-F11
                        if (ctrlPressed && !shiftPressed)
                        {
                            Verse.Log.Message($"保存屏幕编组 {1 + i}");
                            groupData.SaveViewPortState(i);
                            Messages.Message($"屏幕编组 {1 + i} 已保存", MessageTypeDefOf.TaskCompletion);
                            Event.current?.Use();
                            return;
                        }

                        //shift + F1-F11
                        if (shiftPressed && !ctrlPressed)
                        {
                            Verse.Log.Message($"跳转屏幕编组 {1 + i}");
                            groupData.JumpToViewPortState(i);
                            Event.current?.Use();
                            return;
                        }
                    }
                }
            }
        }
    }
}