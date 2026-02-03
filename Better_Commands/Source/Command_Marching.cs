using RimWorld;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BetterCommands.Commands
{
    public class Command_Marching : Command_Action
    {
        private static JobDef marchDef = DefDatabase<JobDef>.GetNamed("Marching");
        public Command_Marching()
        {
            // 设置标签、描述和图标（之后需添加翻译文件）
            this.defaultLabel = "行军";
            this.defaultDesc = "保持警戒前进，有敌人进入范围时原地停止并攻击";
            //this.icon = ContentFinder<Texture2D>.Get("UI/Commands/BetterCommands_Marching", true);
            this.icon = BaseContent.BadTex; // 占位图标，之后替换
        }

        public override void ProcessInput(Event ev)
        {
            Func<LocalTargetInfo, bool> validator = (LocalTargetInfo target) =>
            {
                if (target == null || !target.Cell.IsValid || !target.IsValid)
                {
                    Log.Error("[BetterCommands] Invalid target selected for marching command.");
                    return false;
                }

                //检查是否可达
                var drafetdPawns = Find.Selector.SelectedObjects.OfType<Pawn>().Where(p => p.Drafted).ToList();

                if (!drafetdPawns.Any())
                {
                    Messages.Message("请至少选择一个已征召的角色以执行行军命令。", MessageTypeDefOf.RejectInput);
                    return false;
                }

                foreach (var pawn in drafetdPawns)
                {
                    if (!pawn.CanReach(target.Cell, PathEndMode.OnCell, Danger.Deadly))
                    {
                        Messages.Message($"角色 {pawn.Name} 无法到达目标位置。", MessageTypeDefOf.RejectInput);
                        return false;
                    }
                }

                return true;
            }; 

            Find.Targeter.BeginTargeting(
                targetParams : TargetingParameters.ForCell(),
                action : GiveMarchingJob,
                highlightAction : HighlightAction,
                targetValidator : validator
            );
        }

        private static void GiveMarchingJob(LocalTargetInfo target)
        {
            var draftedPawns = Find.Selector.SelectedObjects
                .OfType<Pawn>()
                .Where(p => p.IsPlayerControlled && p.Drafted)
                .ToList();

            if ( !draftedPawns.Any() )
            {
                Messages.Message("没有选中的已征召单位。", MessageTypeDefOf.RejectInput);
                return;
            }

            //发布指令
            foreach (var pawn in draftedPawns)
            {
                Job job = JobMaker.MakeJob(marchDef, target);
                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }

        private static void HighlightAction(LocalTargetInfo target)
        {
            if (target.IsValid && target.Cell.IsValid)
            {
                //绘制高光特效
            }
        }
    }
}