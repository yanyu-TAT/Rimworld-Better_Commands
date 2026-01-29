/*
 * 模组主类
 */

using HarmonyLib;
using Verse;

namespace BetterCommands.Core
{
    public class BetterCommandsMod : Mod
    {
        public static Harmony harmonyInstance;

        public BetterCommandsMod(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("yanyu.bettercommandsmod");
            harmonyInstance.PatchAll();

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                if (Current.Game != null && Current.Game.GetComponent<GroupData>() != null)
                {
                    Current.Game.components.Add(new GroupData(Current.Game));
                }
            });
        }

    }
}