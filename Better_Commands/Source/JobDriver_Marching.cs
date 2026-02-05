using RimWorld;
using Verse;
using Verse.AI;

namespace BetterCommands.Commands
{
    public class JobDriver_Marching : JobDriver
    {
        private static readonly JobDef shootDef = DefDatabase<JobDef>.GetNamed("BetterAttackStatic");
        private static readonly JobDef meleeDef = DefDatabase<JobDef>.GetNamed("BetterAttackMelee");

        private const int CheckInterval = 60;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil moving = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            moving.tickAction = () =>
            {
                int ticker = Find.TickManager.TicksGame;
                Pawn actor = this.pawn;
                if (actor == null || !actor.Spawned || !actor.Drafted)
                {
                    return;
                }

                //隔一小段时间检测
                if (ticker % CheckInterval != 0)
                {
                    return;
                }

                TargetScanFlags flags = TargetScanFlags.None;
                float range;
                VerbProperties curProperties = actor.CurrentEffectiveVerb.verbProps;

                if (!curProperties.IsMeleeAttack)
                {
                    //远程攻击锁定有视线的威胁目标
                    flags = TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll;
                    range = curProperties.range;
                } 
                else
                {
                    //近战攻击锁定可抵达的威胁目标
                    flags = TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedReachable;
                    range = 30f; //只锁定30格内单位
                }

                IAttackTarget target = AttackTargetFinder.BestAttackTarget(actor, flags, maxDist: range);

                if (target != null)
                {
                    //发现目标，开始攻击
                    Log.Message($"[BetterCommand] Hostile detected");
                    Job attackJob = CreateAttackJob(actor, target, this.job.targetA);
                    if (attackJob != null)
                        actor.jobs.StartJob(attackJob, JobCondition.InterruptForced);
                    return;
                }
            };

            yield return moving;
        }

        // 创建攻击任务
        internal static Job CreateAttackJob(Pawn pawn, IAttackTarget target, LocalTargetInfo pos)
        {
            Thing thing = target?.Thing;
            if(thing == null || !thing.Spawned)
            {
                Log.Error("[BetterCommands] CreateAttackJob: Invalid Attack Target");
                return null;
            }

            //获取攻击方式
            Verb verb = pawn.TryGetAttackVerb(thing);
            if (verb == null)
            {
                Log.Error("[BetterCommands] CreateAttackJob: No valid attack verb found");
                return null;
            }

            if (verb.verbProps.IsMeleeAttack)
            {
                //近战攻击
                Job job = JobMaker.MakeJob(meleeDef, thing, null, pos);
                return job;
            }
            else
            {
                //远程攻击
                Job job = JobMaker.MakeJob(shootDef, thing, pos);
                job.endIfCantShootTargetFromCurPos = true;
                return job;
            }
        }
    }
}