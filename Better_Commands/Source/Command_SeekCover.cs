using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BetterCommands.Commands
{
    [StaticConstructorOnStartup]
    public class Command_SeekCover : Command_Action
    {
        //private static readonly KeyBindingDef keyBindingDef = DefDatabase<KeyBindingDef>.GetNamed("BetterCommands_Marching");
        private static readonly Texture2D iconTexture;
        private static readonly JobDef jobDef = DefDatabase<JobDef>.GetNamed("BetterGoto");

        private const int SearchRadius = 20;
        private const float DistanceWeight = 0.3f;

        private static readonly List<IntVec3> book = new();
        private static int lastProcessedFrame = -1;

        static Command_SeekCover()
        {
            iconTexture = ContentFinder<Texture2D>.Get("Better_Commands/UI/Commands/SeekCoverCommandButton");
            //iconTexture = BaseContent.BadTex;
        }
        public Command_SeekCover()
        {
            // 设置标签、描述和图标
            this.defaultLabel = "BetterCommands.SeekCoverLabel".Translate();
            this.defaultDesc = "BetterCommands.SeekCoverDescription".Translate();
            this.icon = iconTexture;
            //this.hotKey = keyBindingDef;
        }
        public override void ProcessInput(Event ev)
        {
            // 防止同一帧内多次调用导致重复发放任务
            if (Time.frameCount == lastProcessedFrame) return;
            lastProcessedFrame = Time.frameCount;

            List<Pawn> pawns = Find.Selector.SelectedPawns
                .Where(p => p.Spawned && p.Faction == Faction.OfPlayer && p.Drafted)
                .ToList();

            book.Clear();
            bool flag = false;
            foreach (Pawn pawn in pawns)
            {
                IAttackTarget target = AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat);
                if (target != null)
                {
                    IntVec3 targetPos = getBestCover(pawn, target);
                    if (targetPos == pawn.Position) continue;
                    Job job = JobMaker.MakeJob(jobDef, targetPos);
                    pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                    book.Add(targetPos);
                } 
                else if (!flag)
                {
                    flag = true;
                    Messages.Message("BetterCommands.SeekCoverEnemyNotFoundRefusion".Translate(), MessageTypeDefOf.RejectInput);
                }
            }
        }

        private IntVec3 getBestCover(Pawn pawn, IAttackTarget enemy)
        {
            if (pawn == null || !pawn.Spawned){
                Log.Error("[BetterCommands] Command_SeekCover : Invalid Command Executor!");
                return new IntVec3();
            }
            if (enemy == null || enemy.Thing == null){
                Messages.Message("BetterCommands.SeekCoverEnemyNotFoundRefusion".Translate(), MessageTypeDefOf.RejectInput);
                return pawn.Position;
            }

            IntVec3 pos = enemy.Thing.Position;
            Map map = pawn.Map;
            float bestScore = TryGetScore(pos, pawn, pawn.Position);
            IntVec3 bestCover = pawn.Position;
            ReservationManager manager = map.reservationManager;
            //枚举搜索半径内所有格子
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, SearchRadius, false))
            {
                //边界及可立入检查
                if (!cell.InBounds(map) || !cell.Standable(map)) continue;
                //可达性检查
                if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly)) continue;
                //是否已被预定
                if (book.Contains(cell) || cell.GetFirstPawn(map) != null) continue;

                float score = TryGetScore(pos, pawn, cell);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCover = cell;
                }
            }

            return bestScore > 0 ? bestCover : pawn.Position;
        }

        private float TryGetScore(IntVec3 enemy, Pawn pawn, IntVec3 cell)
        {
            Map map = pawn.Map;
            float blockChance = CoverUtility.CalculateOverallBlockChance(cell, enemy, map);
            float distFactor = Square(1f - cell.DistanceTo(pawn.Position) / SearchRadius);
            float score = blockChance * (1 - DistanceWeight) + distFactor * DistanceWeight;
            return score;
        }

        private static float Square(float a) =>
            a * a;
    }
}
