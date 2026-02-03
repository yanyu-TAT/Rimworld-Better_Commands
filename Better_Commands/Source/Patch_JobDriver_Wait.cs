using HarmonyLib;
using Verse;
using Verse.AI;

namespace BetterCommands.Patches
{
    [HarmonyPatch(typeof(JobDriver_Wait))]
    public class Patch_JobDriver_Wait
    {
        private static JobDef marchJobDef = DefDatabase<JobDef>.GetNamed("Marching");

        [HarmonyPatch("MakeNewToils")]
        [HarmonyPostfix]
        public static void Postix(JobDriver_Wait __instance, ref IEnumerable<Toil> __result)
        {
            try
            {
                var toils = new List<Toil>(__result);
                if (toils.Empty()) return;

                Toil waitToil = toils[0];
                Action<int> originAction = waitToil.tickIntervalAction;

                waitToil.tickIntervalAction = (int delta) =>
                {
                    originAction(delta);
                    RestartMarch(__instance);
                };

                //Log.Message("[BetterCommands] Wait toil edit complete!");

                toils[0] = waitToil;
                __result = toils;
                return;
            } catch (Exception ex)
            {
                Log.Error($"[BetterCommands] Error patching JobDriver_Wait: {ex.Message}");
            }
        }

        private static void RestartMarch(JobDriver_Wait instance)
        {
            try
            {
                //Log.Message("[BetterCommands] Edited wait toil running!");
                var pawn = instance.pawn;

                if (pawn == null || !pawn.Spawned || !pawn.IsPlayerControlled) return;

                JobDef def = pawn.jobs.AllJobs().ToList()[0].def;
                Log.Message($"[BetterCommands] Next Toil: {def.defName}");
                if (def.defName == marchJobDef.defName)
                {
                    instance.ReadyForNextToil();
                }
            } catch(Exception ex)
            {
                Log.Error($"[BetterCommands] Error Restarting Marching Job: {ex.Message}");
            }
        }
    }
}