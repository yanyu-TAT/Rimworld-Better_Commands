/*
 * 1.按下shift + 数字键   选中对应分组
 * 2.按下ctrl  + 数字键   创建分组
 */

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace BetterCommands.Core
{
    public struct ViewPortState
    {
        public IntVec3 position;
        public float zoom;
        public int mapID;
    }
    public class GroupData : GameComponent
    {
        private List<List<int>> groupList = new List<List<int>>();
        private List<ViewPortState> viewPortStates = new List<ViewPortState>();

        public GroupData(Game game)
        {
            InitializeGroups();
        }

        private void InitializeGroups()
        {
            groupList.Clear();
            viewPortStates.Clear();

            for (int i = 0; i < 10; i++)
            {
                groupList.Add(new List<int>());
            }

            for (int i = 0; i < 12; i++)
            {
                viewPortStates.Add(new ViewPortState());
            }
        }

        public override void ExposeData() //持久化的数据存取
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //人员编组数据存储
                for (int i = 0; i < 10; i++)
                {
                    List<int> list = groupList[i];
                    Scribe_Collections.Look(ref list, $"BertterCommands_Group{i}", LookMode.Value);
                }

                //屏幕视角数据存储
                for (int i = 0; i < viewPortStates.Count; i++)
                {
                    ViewPortState state = viewPortStates[i];
                    Scribe_Values.Look(ref state.position, $"BertterCommands_ViewPortPosition{i}");
                    Scribe_Values.Look(ref state.zoom, $"BertterCommands_ViewPortZoom{i}");
                    Scribe_Values.Look(ref state.mapID, $"BertterCommands_ViewPortMapID{i}");
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                //人员编组数据加载
                for (int i = 0; i < 10; i++)
                {
                    List<int> list = null;
                    Scribe_Collections.Look(ref list, $"BertterCommands_Group{i}", LookMode.Value);
                    groupList[i] = list ?? new List<int>();
                }

                //屏幕视角数据加载
                for (int i = 0; i < viewPortStates.Count; i++)
                {
                    ViewPortState state = new();
                    Scribe_Values.Look(ref state.position, $"BertterCommands_ViewPortPosition{i}");
                    Scribe_Values.Look(ref state.zoom, $"BertterCommands_ViewPortZoom{i}");
                    Scribe_Values.Look(ref state.mapID, $"BertterCommands_ViewPortMapID{i}");
                    viewPortStates[i] = state;
                }
            }
        }

        //创建编组
        public bool CreateGroup(int num, List<Pawn> pawns)
        {
            if (num < 0 || num >= 10){
                Log.Error("[Better Commands] Group number out of range.");
                return false;
            }

            groupList[num].Clear();

            foreach (var pawn in pawns)
            {
                if (pawn != null)
                {
                    groupList[num].Add(pawn.thingIDNumber);
                    //Log.Message($"Added Pawn {pawn.Name} to group {num}");
                }
            }
            //Log.Message($"Group {num} created with {groupList[num].Count} pawns.");
            return true;
        }

        //选中编组
        public bool SelectGroup(int num)
        {
            if (num < 0 || num >= 10) { 
                Log.Error("[Better Commands] Group number out of range.");
                return false; 
            }

            List<int> ids = groupList[num];
            if (ids == null || ids.Count == 0)
            {
                //Verse.Log.Message($"Group {num} is empty.");
                return false;
            }

            Map map = Find.CurrentMap;
            if (map == null) { 
                Log.Error("[Better Commands] No current map found.");
                return false; 
            }

            List<Pawn> pawnsToSelect = new List<Pawn>();
            foreach (var id in ids)
            {
                Pawn pawn = map.mapPawns.AllPawns.Find(p => p.thingIDNumber == id && p.Spawned && p.Faction == Faction.OfPlayer);
                if (pawn != null)
                {
                    pawnsToSelect.Add(pawn);
                    //Verse.Log.Message($"Selected Pawn {pawn.Name} from group {num}");
                }
            }
            if (pawnsToSelect.Count > 0)
            {
                Find.Selector.ClearSelection();
                foreach (var pawn in pawnsToSelect)
                {
                    Find.Selector.Select(pawn, false, true);
                    //如果启用了自动征召且征召可用，则播放音效并征召未征召的单位
                    //Log.Message($"[BetterCommands] Selecting pawns, auto draft:{BetterCommandsMod.CurrentAutoDraftOption}");
                    if (BetterCommandsMod.CurrentAutoDraftOption && !pawn.Drafted)
                    {
                        //依据原版逻辑进行判定
                        bool canDraft = !(pawn.Downed || pawn.Deathresting);
                        if(ModsConfig.BiotechActive && pawn.IsColonyMech && canDraft)
                        {
                            AcceptanceReport acceptanceReport = MechanitorUtility.CanDraftMech(pawn);
                            if(!acceptanceReport)
                            {
                                canDraft = false;
                            }
                        }

                        //Log.Message($"")
                        if (canDraft)
                        {
                            SoundDefOf.DraftOn.PlayOneShotOnCamera();
                            pawn.drafter.Drafted = true;
                        }
                    }
                }
            }
            else
            {
                //Verse.Log.Message($"No valid pawns found in group {num} to select.");
            }
            return true;
        }

        //移出编组
        public int DeleteFromGroup(int num, List<Pawn> pawns)
        {
            if (num < 0 || num >= 10)
            {
                Log.Error("[Better Commands] Group number out of range.");
                return 0;
            }

            var cnt = 0;
            foreach (var pawn in pawns)
            {
                if (pawn != null)
                {
                    var id = pawn.thingIDNumber;
                    if (groupList[num].Remove(id))
                    {
                        cnt++; 
                    }
                }
            }
            return cnt;
        }

        //保存屏幕编组
        public bool SaveViewPortState(int index)
        {
            Log.Message("[Better Commands] Saving viewport state: " + index);
            if (index < 0 || index >= viewPortStates.Count) return false;

            viewPortStates[index] = new ViewPortState
            {
                position = Find.CameraDriver.MapPosition,
                zoom = Find.CameraDriver.RootSize,
                mapID = Find.CurrentMap.uniqueID
            };
            return true;
        }

        //跳转至屏幕编组
        public bool JumpToViewPortState(int index)
        {
            //Log.Message("[Better Commands] Jumping to viewport state: " + index);
            if (index < 0 || index >= viewPortStates.Count) { 
                Log.Error("[Better Commands] Viewport state index out of range.");
                return false; 
            }

            ViewPortState state = viewPortStates[index];
            if (state.mapID < 0) { 
                Log.Error("[Better Commands] Invalid map ID stored in viewport state.");
                return false; 
            }

            Map targetMap = Find.Maps.Find(m => m.uniqueID == state.mapID);
            if (targetMap == null) { 
                Log.Warning("[Better Commands] Target map not found.");
                Messages.Message("BetterCommands.CameraTargetMapNotFoundRefusion".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            try
            {
                CameraJumper.TryJump(state.position, targetMap);
                Find.CameraDriver.SetRootSize(state.zoom);
            }
            catch (System.Exception ex)
            {
                Log.Error("[Better Commands] Error jumping to viewport state: " + ex.Message);
                return false;
            }
            return true;
        }
    }
}