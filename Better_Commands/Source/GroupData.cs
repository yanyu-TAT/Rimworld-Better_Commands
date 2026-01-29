/*
 * 1.按下shift + 数字键   选中对应分组
 * 2.按下ctrl  + 数字键   创建分组
 */

using RimWorld;
using Verse;
using System.Collections.Generic;

namespace BetterCommands.Core
{
    public class GroupData : GameComponent
    {
        private List<List<int>> groupList = new List<List<int>>();

        public GroupData(Game game)
        {
            for (int i = 0; i < 10; i++)
            {
                groupList.Add(new List<int>());
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                for (int i = 0; i < 10; i++)
                {
                    List<int> list = groupList[i];
                    Scribe_Collections.Look(ref list, $"BertterCommands_Group{i}", LookMode.Value);
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                for (int i = 0; i < 10; i++)
                {
                    List<int> list = null;
                    Scribe_Collections.Look(ref list, $"BertterCommands_Group{i}", LookMode.Value);
                    groupList[i] = list ?? new List<int>();
                }
            }
        }

        public void CreateGroup(int num, List<Pawn> pawns)
        {
            if (num < 0 || num >= 10) return;

            groupList[num].Clear();

            foreach (var pawn in pawns)
            {
                if (pawn != null)
                {
                    groupList[num].Add(pawn.thingIDNumber);
                    Log.Message($"Added Pawn {pawn.Name} to group {num}");
                }
            }
            Log.Message($"Group {num} created with {groupList[num].Count} pawns.");
        }

        public void SelectGroup(int num)
        {
            if (num < 0 || num >= 10) return;

            List<int> ids = groupList[num];
            if (ids == null || ids.Count == 0)
            {
                Verse.Log.Message($"Group {num} is empty.");
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null) return;

            List<Pawn> pawnsToSelect = new List<Pawn>();
            foreach (var id in ids)
            {
                Pawn pawn = map.mapPawns.AllPawns.Find(p => p.thingIDNumber == id && p.Spawned && p.Faction == Faction.OfPlayer);
                if (pawn != null)
                {
                    pawnsToSelect.Add(pawn);
                    Verse.Log.Message($"Selected Pawn {pawn.Name} from group {num}");
                }
            }
            if (pawnsToSelect.Count > 0)
            {
                Find.Selector.ClearSelection();
                foreach (var pawn in pawnsToSelect)
                {
                    Find.Selector.Select(pawn, false, true);
                }
            }
            else
            {
                Verse.Log.Message($"No valid pawns found in group {num} to select.");
            }
        }
    }
}