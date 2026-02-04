using Verse;
using Verse.AI;

namespace BetterCommands.Commands
{
    //TargetA - 攻击目标
    //TargetB - 行军目标
    public class JobDriver_BetterAttakStatic : JobDriver_AttackStatic
    {
        private static readonly JobDef marchDef = DefDatabase<JobDef>.GetNamed("Marching");

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
                        Job job = JobMaker.MakeJob(marchDef, TargetB);
                        pawn.jobs.jobQueue.EnqueueFirst(job);
                    }
                }
            });
            return toils;
        }
    }
}