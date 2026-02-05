using BetterCommands.Core;
using RimWorld;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BetterCommands.Commands
{
    [StaticConstructorOnStartup]
    public class Command_Marching : Command_Action
    {
        private static readonly KeyBindingDef keyBindingDef = DefDatabase<KeyBindingDef>.GetNamed("BetterCommands_Marching");
        private static readonly JobDef marchDef = DefDatabase<JobDef>.GetNamed("Marching");
        private static readonly JobDef shootDef = DefDatabase<JobDef>.GetNamed("BetterAttackStatic");
        private static readonly JobDef meleeDef = DefDatabase<JobDef>.GetNamed("BetterAttackMelee");
        private static readonly Texture2D iconTexture;

        static Command_Marching()
        {
            iconTexture = ContentFinder<Texture2D>.Get("Better_Commands/UI/Commands/MarchingCommandButton");
        }
        public Command_Marching()
        {
            // 设置标签、描述和图标
            this.defaultLabel = "BetterCommands.MarchingLabel".Translate();
            this.defaultDesc = "BetterCommands.MarchingDescription".Translate();
            this.icon = iconTexture;
            this.hotKey = keyBindingDef;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GizmoResult result =  base.GizmoOnGUI(topLeft, maxWidth, parms);
            //修改gui
            return result;
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
                    Messages.Message("BetterCommands.MarchingNoValidPawnRefusion".Translate(), MessageTypeDefOf.RejectInput);
                    return false;
                }

                foreach (var pawn in drafetdPawns)
                {
                    if (!pawn.CanReach(target.Cell, PathEndMode.OnCell, Danger.Deadly))
                    {
                        Messages.Message(Helper.Translate("BetterCommands.MarchingNoPathRefusion", ("pawnName", pawn.Name)), MessageTypeDefOf.RejectInput);
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
                Messages.Message("BetterCommands.MarchingNoValidPawnRefusion".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            //发布指令
            foreach (var pawn in draftedPawns)
            {
                //if (pawn.CurJobDef.defNameHash != shootDef.defNameHash)
                //{
                    //如果单位未在进行攻击，则给予行军工作
                Job job = JobMaker.MakeJob(marchDef, target);
                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                //}
                //else
                //{//如果单位正在进行攻击，则覆盖其攻击工作
                //    LocalTargetInfo shootTarget = pawn.CurJob.targetA;
                //    Job job = JobMaker.MakeJob(shootDef, shootTarget, target);
                //    pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                //}
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