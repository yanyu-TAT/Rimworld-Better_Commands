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
        private const float DoubleClick_Time = 0.33f; //双击时间间隔

        //用于阻止重复处理
        private static Dictionary<KeyCode, bool> keysProcessed = new();
        private static int lastFrame = -1;

        //用于检测双击
        private static Dictionary<KeyCode, float> lastClicked = new();

        /// <summary>
        /// 记录双击事件，若在规定时间内再次点击同一数字键，则执行跳转到编组中心的操作
        /// </summary>
        /// <param name="groupIndex">编组索引</param>
        /// <param name="key">按下的键</param>
        /// <param name="groupData">编组数据对象</param>
        private static void HandleDoubleClick(int groupIndex, KeyCode key, GroupData groupData)
        {
            var now = Time.realtimeSinceStartup;
            if (lastClicked.ContainsKey(key) && now - lastClicked[key] < DoubleClick_Time)
            {
                //Log.Message($"[BetterCommands] lastKeyPressed : {groupIndex} : {now - lastClicked[key]}");
                groupData.JumpToGroupCenter(groupIndex);
                lastClicked.Remove(key);
            }
            else
            {
                lastClicked[key] = now;
            }
        }
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

            //检查双击超时
            for (int i = 0; i <= 9; i++)
            {
                var now = Time.realtimeSinceStartup;
                KeyCode key = KeyCode.Alpha0 + i;
                if (lastClicked.ContainsKey(key) && now - lastClicked[key] >= DoubleClick_Time)
                {
                    lastClicked.Remove(key);
                    groupData.SelectGroup(i);
                }
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
                    if (GroupSettingsUtility.ShouldHandleGroupShortcuts(i))
                    {
                        //Verse.Log.Message($"Ctrl: {ctrlPressed}, Shift: {shiftPressed}");

                        //ctrl + 数字键
                        if (ctrlPressed && !shiftPressed && !altPressed)
                        {
                            //Verse.Log.Message($"保存编组 {i}");
                            List<Thing> selectedThings = Find.Selector.SelectedObjects
                                .OfType<Thing>()
                                .Where(p => GroupData.ResolveType(p) != GroupMemberType.Invalid)
                                .ToList();

                            if (selectedThings.Count != 0)
                            {
                                groupData.CreateGroup(i, selectedThings);
                                Messages.Message(Helper.Translate("BetterCommands.GroupSavingDone", ("num", i), ("count", selectedThings.Count)), MessageTypeDefOf.TaskCompletion);
                            }
                            else
                            {
                                Messages.Message("BetterCommands.GroupingNoValidPawnRefusion".Translate(), MessageTypeDefOf.RejectInput);
                            }
                            Event.current?.Use();
                            return;
                        }

                        //shift + 数字键
                        if (shiftPressed && !ctrlPressed && !altPressed)
                        {
                            HandleDoubleClick(i, key, groupData);
                            Event.current?.Use();
                            return;
                        }

                        //alt + 数字键
                        if (!ctrlPressed && altPressed && !shiftPressed)
                        {
                            List<Thing> selectedThings = Find.Selector.SelectedObjects
                                .OfType<Thing>()
                                .Where(p => GroupData.ResolveType(p) != GroupMemberType.Invalid)
                                .ToList();

                            if (selectedThings.Count != 0)
                            {
                                int cnt = groupData.DeleteFromGroup(i, selectedThings);
                                if (cnt != 0)
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

                        //仅数字键
                        if (!ctrlPressed && !shiftPressed && !altPressed && GroupSettingsUtility.ShouldHandleNumberOnly(i))
                        {
                            HandleDoubleClick(i, key, groupData);
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