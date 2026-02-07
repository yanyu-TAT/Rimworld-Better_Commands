using BetterCommands.Core;
using RimWorld;

namespace BetterCommands.UI
{
    public class MainButtonWorker_GroupingMainTab : MainButtonWorker_ToggleTab
    {
        public override bool Visible => BetterCommandsMod.CurrentGroupingMainTabVisibility;
    }
}