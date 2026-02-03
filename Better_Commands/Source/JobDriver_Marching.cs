using RimWorld;
using Verse;
using Verse.AI;

namespace BetterCommands.Commands
{
    public class JobDriver_Marching : JobDriver
    {
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
                int ticker = Current.Game.tickManager.gameStartAbsTick;
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

                if (!actor.CurrentEffectiveVerb.verbProps.IsMeleeAttack)
                {
                    //远程攻击只锁定有视线的目标
                    flags = TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll;
                } else
                {
                    //近战攻击锁定所有威胁目标（现不做实现）
                    Log.Warning("[BetterCommands] Melee attack target scanning not implemented yet.");
                    return;
                }

                IAttackTarget target = AttackTargetFinder.BestShootTargetFromCurrentPosition(
                    actor,
                    flags,
                    null,
                    minDistance: 0f,
                    maxDistance: 9999f
                );

                if ( target != null)
                {
                    //发现目标，开始攻击
                    Job attackJob = CreateAttackJob(actor, target);
                    if (attackJob != null)
                    {
                        Job curJob = actor.jobs.curJob;
                        //将当前任务放回队列前端
                        actor.jobs.jobQueue.EnqueueFirst(curJob);
                        actor.jobs.StartJob(attackJob, JobCondition.InterruptOptional);
                    }
                    return;
                }
            };

            yield return moving;
        }

        // 创建攻击任务
        private Job CreateAttackJob(Pawn pawn, IAttackTarget target)
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
                Log.Warning("[BetterCommands] Melee attack job creation not implemented yet.");
                return null;
            }
            else
            {
                //远程攻击
                Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, thing);
                job.endIfCantShootTargetFromCurPos = true;
                return job;
            }
        }
    }
}