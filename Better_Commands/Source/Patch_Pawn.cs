using BetterCommands.Commands;
using HarmonyLib;
using Verse;

namespace BetterCommands.Patches
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_Pawn
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }

            if (__instance.IsPlayerControlled && __instance.Drafted)
            {
                yield return new Command_Marching();
            }
        }
    }
}