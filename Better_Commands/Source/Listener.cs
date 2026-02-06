/*
 * 监听shift + 数字
 * 监听ctrl  + 数字
 * 监听alt   + 数字
 * 监听ctrl  + Fn
 * 监听shift + Fn
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
        private static Dictionary<KeyCode, bool> keysProcessed = new();
        private static int lastFrame = -1;
        public static void Postfix()
        {
            if (Current.Game == null)
                return;

            GroupData groupData = Current.Game?.GetComponent<GroupData>();
            if (groupData == null)
                return;

            //重置处理记录
            if (lastFrame != Time.frameCount)
            {
                lastFrame = Time.frameCount;
                keysProcessed.Clear();
            }

            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            // 检测是否按下了数字键 0-9
            for (int i = 0; i <= 9; i++)
            {
                KeyCode key = KeyCode.Alpha0 + i;

                if (Input.GetKeyDown(key))
                {
                    //防止重复处理
                    if (keysProcessed.ContainsKey(key) && keysProcessed[key])
                        continue;
                    keysProcessed[key] = true;
                    //Verse.Log.Message($"检测到按键: {key}");
                    if (GroupSettingsUtility.ShouldHandleGroupShortcuts(i)){
                        //Verse.Log.Message($"Ctrl: {ctrlPressed}, Shift: {shiftPressed}");

                        //ctrl + 数字键
                        if (ctrlPressed && !shiftPressed && !altPressed)
                        {
                            //Verse.Log.Message($"保存编组 {i}");
                            List<Pawn> selectedPawns = Find.Selector.SelectedPawns
                                .Where(p => p.Faction == Faction.OfPlayer && !p.IsAnimal)
                                .ToList();

                            if (selectedPawns.Count != 0)
                            {
                                groupData.CreateGroup(i, selectedPawns);
                                Messages.Message(Helper.Translate("BetterCommands.GroupSavingDone", ("num", i), ("count", selectedPawns.Count)), MessageTypeDefOf.TaskCompletion);
                            }
                            else{
                                Messages.Message("BetterCommands.GroupingNoValidPawnRefusion".Translate(), MessageTypeDefOf.RejectInput);
                            }
                            Event.current?.Use();
                            return;
                        }

                        //shift + 数字键
                        if (shiftPressed && !ctrlPressed && !altPressed)
                        {
                            Verse.Log.Message($"选中编组 {i}");
                            groupData.SelectGroup(i);
                            Event.current?.Use();
                            return;
                        }

                        //alt + 数字键
                        if (!ctrlPressed && altPressed && !shiftPressed)
                        {
                            List<Pawn> selectedPawns = Find.Selector.SelectedPawns
                                .Where(p => p.Faction == Faction.OfPlayer)
                                .ToList();

                            if(selectedPawns.Count != 0)
                            {
                                int cnt = groupData.DeleteFromGroup(i, selectedPawns);
                                if(cnt != 0)
                                {
                                    Messages.Message(Helper.Translate("BetterCommands.GroupDeletingDone", ("num", i), ("count", cnt)), MessageTypeDefOf.TaskCompletion);
                                }
                                else
                                {
                                    Messages.Message(Helper.Translate("BetterCommands.GroupDeletingNotContainRefusion", ("num", i)), MessageTypeDefOf.RejectInput);
                                }
                            }
                            else
                            {
                                Messages.Message("BetterCommands.GroupingNoValidPawnRefusion".Translate(), MessageTypeDefOf.RejectInput);
                            }

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
                    //Log.Message($"检测到按键: {key}");
                    if (GroupSettingsUtility.ShouldHandleGroupShortcuts(99))
                    {
                        //ctrl + F1-F11
                        if (ctrlPressed && !shiftPressed)
                        {
                            //Verse.Log.Message($"保存屏幕编组 {1 + i}");
                            groupData.SaveViewPortState(i);
                            Messages.Message(Helper.Translate("BetterCommands.CameraSavingDone", ("num", i + 1)), MessageTypeDefOf.TaskCompletion);
                            Event.current?.Use();
                            return;
                        }

                        //shift + F1-F11
                        if (shiftPressed && !ctrlPressed)
                        {
                            //Verse.Log.Message($"跳转屏幕编组 {1 + i}");
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