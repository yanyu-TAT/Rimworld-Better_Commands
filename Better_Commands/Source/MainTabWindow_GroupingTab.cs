using BetterCommands.Core;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;


namespace BetterCommands.UI
{
    public class MainTabWindow_GroupingTab : MainTabWindow
    {
        //窗口大小
        public override UnityEngine.Vector2 RequestedTabSize => new Vector2(700f, 600f);

        //UI相关常量
        private const float GroupRowHeight = 35f;
        private const float SelectedRowHeight = 100f;
        private const float PawnIconSize = 70f;
        private const float ButtonWidth = 180f;

        //当前编组列表滚动位置
        private Vector2 groupScrollPos = Vector2.zero;
        //当前展开的编组编号，-1表示未展开
        public int selectedGorupIndex = -1;
        //已展开的编组的成员列表滚动位置
        public Vector2 selectedGroupScrollPos = Vector2.zero;

        private static GroupData groupData =>
            Current.Game?.GetComponent<GroupData>();


        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "BetterCommands.GroupingTabTitle".Translate());
            Text.Font = GameFont.Small;

            Rect listRect = new Rect(0f, 45f, inRect.width, inRect.height - 45f);
            DrawGroupList(listRect);
        }

        private void DrawGroupList(Rect outRect)
        {
            //计算总行高
            float totalHeight = GroupRowHeight * 10 + selectedGorupIndex < 0 ? SelectedRowHeight : 0;

            //滚动视图区域
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, totalHeight);

            //进行滚动
            Widgets.BeginScrollView(outRect, ref groupScrollPos, viewRect);
            float curY = 0f;

            for (int i = 0; i < 10; i++)
            {
                DrawSingleGroupRow(new Rect(0f, curY, viewRect.width, GroupRowHeight), i);
                curY += GroupRowHeight;

                if (i == selectedGorupIndex)
                {
                    DrawPawnIcons(new Rect(0f, curY, viewRect.width, SelectedRowHeight), i);
                    curY += SelectedRowHeight;
                }
            }

            Widgets.EndScrollView();
        }

        private void DrawSingleGroupRow(Rect rowRect, int index)
        {
            Widgets.DrawAltRect(rowRect);

            //填充编组标签内容
            string groupLabel = Helper.Translate("BetterCommands.GroupingTabGroupLabel", ("num", index));
            List<Pawn> pawns = groupData.GetGroupMembers(index).ToList();
            if (pawns.Count > 0)
            {
                //若编组不为空则标记数量
                groupLabel += Helper.Translate("BetterCommands.GroupingTabGroupLabelExtra", ("count", pawns.Count));
            }

            //绘制编组标签
            Rect labelRect = new Rect(rowRect.x + 10f, rowRect.y + 5f, 180f, 25f);
            Widgets.Label(labelRect, groupLabel);

            //绘制按钮（从右到左）
            //展开
            string expandButtonLabel = selectedGorupIndex == 
                index ? 
                "BetterCommands.GroupingTabGroupFoldUp".Translate() : 
                "BetterCommands.GroupingTabGroupExpand".Translate();
            Rect expandButtonRect = new Rect(rowRect.xMax - ButtonWidth / 2, rowRect.y + 5f, 80f, 25f);
            if (Widgets.ButtonText(expandButtonRect, expandButtonLabel))
            {
                if (selectedGorupIndex == index)
                {
                    //收起
                    selectedGorupIndex = -1;
                }
                else
                {
                    //展开当前，重置滚动进度
                    selectedGorupIndex = index;
                    selectedGroupScrollPos = Vector2.zero;
                }
            }
            //删除
            string deleteButtonLabel = "BetterCommands.GroupingTabGroupDelete".Translate();
            Rect deleteButtonRect = new Rect(expandButtonRect.xMax - ButtonWidth, rowRect.y + 5f, 80f, 25f);
            if (Widgets.ButtonText(deleteButtonRect, deleteButtonLabel))
            {
                if (pawns.Count > 0)
                {
                    //弹窗确认
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        Helper.Translate("BetterCommands.GroupingTabGroupDeleteConfirmWindow", ("num", index)),
                        "BetterCommands.Confirm".Translate(),
                        () => groupData.DeleteGroup(index),
                        "BetterCommands.Cancel".Translate(),
                        null
                    ));
                }
            }
            //选中
            string selectButtonLabel = "BetterCommands.GroupingTabGroupSelect".Translate();
            Rect selectButtonRect = new Rect(deleteButtonRect.xMax - ButtonWidth, rowRect.y + 5f, 80f, 25f);
            if (Widgets.ButtonText(selectButtonRect, selectButtonLabel))
            {
                groupData.SelectGroup(index);
                //关闭窗口
                Close();
            }

            if (Widgets.ButtonInvisible(rowRect))
            {
                //点击整行时的反馈
            }
        }

        private void DrawPawnIcons(Rect rowRect, int index)
        {
            //绘制背景
            Widgets.DrawBoxSolid(rowRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));

            //获取成员列表
            List<Pawn> groupMemebers = groupData.GetGroupMembers(index).ToList();

            //计算横向滚动区域
            float totalIconsWidth = (groupMemebers.Count + 1.5f) * PawnIconSize; //预留添加按钮位置
            Rect scrollViewRect = new Rect(rowRect.x, rowRect.y, totalIconsWidth, rowRect.height);
            Rect visibleRect = new Rect(rowRect.x, rowRect.y, rowRect.width, rowRect.height);

            //进行横向滚动
            Widgets.BeginScrollView(visibleRect, ref selectedGroupScrollPos, scrollViewRect);
            float curX = 0f;

            foreach (Pawn pawn in groupMemebers)
            {
                DrawIconWithDelete(new Rect(curX, rowRect.y, PawnIconSize, rowRect.height), pawn, index);
                curX += PawnIconSize;
            }

            DrawAddMemberButton(new Rect(curX, rowRect.y, PawnIconSize, rowRect.height), index);
            Widgets.EndScrollView();
        }

        private void DrawIconWithDelete(Rect iconRect, Pawn pawn, int index)
        {
            Widgets.ThingIcon(new Rect(iconRect.center.x - 24f, iconRect.y + 5f, 48f, 48f), pawn);

            //绘制名字（截断处理）
            string name = pawn.Name?.ToStringShort ?? "...";
            if (name.Length > 4) name = name.Substring(0, 4) + "..";
            Rect nameRect = new(iconRect.x, iconRect.y + 55f, iconRect.width, 20f);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(nameRect, name);
            Text.Anchor = TextAnchor.UpperLeft;

            //绘制删除按钮
            Rect deleteBtnRect = new Rect(iconRect.xMax - 20f, iconRect.y + 5f, 15f, 15f);
            if (Widgets.ButtonImage(deleteBtnRect, TexButton.Delete))
            {
                List<Pawn> list = new();
                list.Add(pawn);
                groupData.DeleteFromGroup(index, list);
            }

            //悬停显示全名
            if (Mouse.IsOver(iconRect))
            {
                TooltipHandler.TipRegion(iconRect, pawn.Name.ToStringFull);
            }
        }

        private void DrawAddMemberButton(Rect buttonRect, int index)
        {
            List<FloatMenuOption> options = new();
            List<Pawn> validPawns = groupData.getValidPawns().ToList();
            List<Pawn> curGroupMembers = groupData.GetGroupMembers(index).ToList();

            foreach(Pawn pawn in validPawns)
            {
                if (!curGroupMembers.Contains(pawn))
                {
                    options.Add(new FloatMenuOption(
                        pawn.Name.ToStringShort,
                        () => groupData.AddToGroup(index, pawn)
                    ));
                }
            }
            
            if (options.Count <= 0)
            {
                options.Add(new FloatMenuOption(
                    "BetterCommands.GroupingTabNoValidPawnToAdd".Translate(),
                    null
                ));
            }

            if (Widgets.ButtonImage(buttonRect, TexButton.Add))
            {
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }
}
