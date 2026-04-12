/*
 * 1.按下shift + 数字键   选中对应分组
 * 2.按下ctrl  + 数字键   创建分组
 */

using RimWorld;
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

    public enum GroupMemberType
    {
        Pawn,       //殖民者
        Building,   //建筑
        Invalid     //无效
    }

    public class GroupMemberData : IExposable, IEquatable<GroupMemberData>
    {
        public int thingIDNumber;
        public GroupMemberType type;

        public bool Equals(GroupMemberData other)
        {
            return thingIDNumber == other.thingIDNumber && type == other.type;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref thingIDNumber, "ThingIDNumber");
            Scribe_Values.Look(ref type, "GroupMemberType", GroupMemberType.Pawn);
        }
    }
    public class GroupData : GameComponent
    {
        private List<List<GroupMemberData>> groupList = new(10);
        private List<ViewPortState> viewPortStates = new();

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
                groupList.Add(new List<GroupMemberData>());
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
                //人员编组数据存储（新）
                for (int i = 0; i < 10; i++)
                {
                    List<GroupMemberData> list = groupList[i];
                    Scribe_Collections.Look(ref list, $"BertterCommands_GroupMembers{i}", LookMode.Deep);
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

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                //人员编组数据加载
                for (int i = 0; i < 10; i++)
                {
                    List<GroupMemberData> list = null;
                    Scribe_Collections.Look(ref list, $"BertterCommands_GroupMembers{i}", LookMode.Deep);
                    groupList[i] = list ?? new List<GroupMemberData>();
                }

                //人员编组数据加载（旧，兼容之前版本）
                for (int i = 0; i < 10; i++)
                {
                    List<int> list = null;
                    Scribe_Collections.Look(ref list, $"BertterCommands_Group{i}", LookMode.Value);
                    if (groupList[i].Empty() && !list.Empty())
                    {
                        foreach (var id in list)
                        {
                            groupList[i].Add(new GroupMemberData
                            {
                                thingIDNumber = id,
                                type = GroupMemberType.Pawn
                            });
                        }
                    }
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

        public static GroupMemberType ResolveType(Thing thing)
        {
            if (thing is Pawn pawn && pawn.Spawned && pawn.Faction == Faction.OfPlayer 
                && !pawn.IsAnimal) return GroupMemberType.Pawn;                     //仅支持非动物的殖民者编组
            if (thing is Building building && building.Spawned && building.Faction == Faction.OfPlayer
                && building is Building_Turret) return GroupMemberType.Building;    //仅支持炮塔建筑编组
            return GroupMemberType.Invalid;
        }

        //创建编组
        public bool CreateGroup(int num, List<Thing> things)
        {
            if (num < 0 || num >= 10){
                Log.Error("[Better Commands] Group number out of range.");
                return false;
            }

            groupList[num].Clear();

            foreach (var thing in things)
            {
                var type = ResolveType(thing);
                if (thing != null && type != GroupMemberType.Invalid)
                {
                    GroupMemberData memberData = new()
                    {
                        thingIDNumber = thing.thingIDNumber,
                        type = type
                    };
                    groupList[num].Add(memberData);
                    //Log.Message($"Added Pawn {pawn.Name} to group {num}");
                }
            }
            //Log.Message($"Group {num} created with {groupList[num].Count} pawns.");
            return true;
        }

        //获取编组成员（当前地图内）
        public IEnumerable<Thing> GetGroupMembers(int num)
        {
            if (num < 0 || num >= 10)
            {
                Log.Error("[Better Commands] Group number out of range.");
                yield break;
            }

            List<GroupMemberData> ids = groupList[num];
            if (ids == null || ids.Count == 0)
            {
                //Verse.Log.Message($"Group {num} is empty.");
                yield break;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                Log.Error("[Better Commands] No current map found.");
                yield break;
            }

            List<Thing> things = null;
            List<Pawn> pawns = map.mapPawns.AllPawns.Where(p => p.Spawned && p.Faction == Faction.OfPlayer).ToList();
            foreach (var member in ids)
            {
                if (member.type == GroupMemberType.Building){
                    things ??= map.listerBuildings.allBuildingsColonist.ToList<Thing>();
                    Thing thing = things.FirstOrDefault(t => t.thingIDNumber == member.thingIDNumber);
                    if (thing != null)
                    {
                        yield return thing;
                    }
                    else
                    {
                        Log.Warning($"Building with ID {member.thingIDNumber} not found on current map for group {num}.");
                    }
                }
                if (member.type == GroupMemberType.Pawn)
                {
                    Pawn pawn = pawns.FirstOrDefault(p => p.thingIDNumber == member.thingIDNumber);
                    if (pawn != null)
                    {
                        yield return pawn;
                    }
                    else
                    {
                        Log.Warning($"Pawn with ID {member.thingIDNumber} not found on current map for group {num}.");
                    }
                }
            }
        }

        //选中编组
        public bool SelectGroup(int num)
        {
            List<Thing> thingsToSelect = GetGroupMembers(num).ToList();
            if (thingsToSelect.Count > 0)
            {
                Find.Selector.ClearSelection();
                foreach (var thing in thingsToSelect)
                {
                    Find.Selector.Select(thing, false, true);
                    //如果启用了自动征召且征召可用，则播放音效并征召未征召的单位
                    //Log.Message($"[BetterCommands] Selecting pawns, auto draft:{BetterCommandsMod.CurrentAutoDraftOption}");
                    if (thing is Pawn pawn && BetterCommandsMod.CurrentAutoDraftOption && !pawn.Drafted)
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
                //Log.Warning($"No valid pawns found in group {num} to select.");
                return false;
            }
            return true;
        }

        //删除整个编组
        public bool DeleteGroup(int num)
        {
            if (num < 0 || num >= 10)
            {
                Log.Error("[Better Commands] Group number out of range.");
                return false;
            }

            groupList[num].Clear();
            return true;
        }

        //获取当前地图内的合法编组目标
        public IEnumerable<Pawn> getValidPawns()
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                Log.Error("[Better Commands] No current map found.");
                yield break;
            }

            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (pawn.Spawned && pawn.Faction == Faction.OfPlayer && !pawn.IsAnimal)
                {
                    yield return pawn;
                }
            }
        }

        //移出编组
        public int DeleteFromGroup(int num, List<Thing> things)
        {
            if (num < 0 || num >= 10)
            {
                Log.Error("[Better Commands] Group number out of range.");
                return 0;
            }

            var cnt = 0;
            foreach (var thing in things)
            {
                if (thing != null)
                {
                    var type = ResolveType(thing);
                    var map = Find.CurrentMap;
                    if (map == null)
                    {
                        Log.Error("[Better Commands] No current map found.");
                        continue;
                    }
                    if (type != GroupMemberType.Invalid)
                    {
                        GroupMemberData member = new()
                        {
                            thingIDNumber = thing.thingIDNumber,
                            type = type,
                        };
                        if (groupList[num].Remove(member))
                        {
                            cnt++;
                        }
                    }
                }
            }
            return cnt;
        }

        //向编组添加单个殖民者
        public bool AddToGroup(int num, Thing thing)
        {
            if (num < 0 || num >= 10)
            {
                Log.Error("[Better Commands] Group number out of range.");
                return false;
            }
            
            var type = ResolveType(thing);
            GroupMemberData member = new()
            {
                thingIDNumber = thing.thingIDNumber,
                type = type
            };
            if (type == GroupMemberType.Invalid || groupList[num].Contains(member)) return false; //避免重复添加/添加非法对象
            groupList[num].Add(member);
            return true;
        }

        //跳转到对应编组中心
        public bool JumpToGroupCenter(int index)
        {
            List<Thing> things = GetGroupMembers(index).ToList();
            if(things == null || things.Count == 0) return false;

            IntVec3 center = IntVec3.Zero;
            foreach (var thing in things)
            {
                center += thing.Position;
            }
            center /= things.Count;

            Map map = Find.CurrentMap;
            if (map ==  null) return false;
            CameraJumper.TryJump(center, map);

            return true;
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