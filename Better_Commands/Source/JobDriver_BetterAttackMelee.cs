using Verse;
using Verse.AI;

namespace BetterCommands.Commands
{
    //TargetA - 攻击目标
    //TargetB - 被原版用作了其他用途
    //TargetC - 行军目标
    public class JobDriver_BetterAttackMelee : JobDriver_AttackMelee
    {
        private static readonly JobDef marchDef = DefDatabase<JobDef>.GetNamed("Marching");
        private const int MAX_CHASE_Dist = 30; //最多偏离原路线的距离
        private static readonly int MaxDistSquare = MAX_CHASE_Dist * MAX_CHASE_Dist;

        private IntVec3 chaseStartPos;
        bool isChasing = false;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            IEnumerable<Toil> toils = base.MakeNewToils(); 
            
            AddFinishAction(delegate (JobCondition condition)
            {
                Pawn pawn = this.pawn;
                if (!pawn.IsPlayerControlled || !pawn.Drafted)
                    return;

                if (!this.job.playerInterruptedForced && condition != JobCondition.InterruptForced)
                {
                    //如果没有后续工作则恢复行军状态
                    if (pawn.jobs.jobQueue.Count == 0)
                    {
                        Job job = JobMaker.MakeJob(marchDef, TargetC);
                        pawn.jobs.jobQueue.EnqueueFirst(job);
                    }
                }
            });

            //初始化追击起始点
            yield return Toils_General.Do(delegate
            {
                chaseStartPos = pawn.Position;
                isChasing = true;
            });

            //添加失败条件：超过最大追击距离或目标距离过远则取消追击
            AddFailCondition(() =>
            {
                if (!isChasing)
                    return false;
                //当前位置距离追击起始位置的距离
                int chaseDist = chaseStartPos.DistanceToSquared(pawn.Position);
                //当前位置距离追击目标的距离
                int targetDist = pawn.Position.DistanceToSquared(TargetA.Pawn.Position);
                //Log.Message($"chase: {chaseDist} target:{}")
                return chaseDist > MaxDistSquare || targetDist > MaxDistSquare;
            });

            //原版逻辑
            foreach (Toil toil in toils)
            {
                yield return toil;
            }
        }

    }
}