using ArmyCommander.ACBehaviors.Context;
using ArmyCommander.Store;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(MobileParty))]
    internal static class MobileParty_LastVisitedSettlement_Setter_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MobileParty.LastVisitedSettlement), MethodType.Setter)]
        private static void Prefix(MobileParty __instance, Settlement __0)
        {
            Army currentArmy = __instance.Army;

            if (currentArmy != null &&
                currentArmy.LeaderParty == __instance &&
                ArmyCommandsBehaviorStore.army_commands.ContainsKey(currentArmy))
            {
                Settlement previousSettlement = __instance.LastVisitedSettlement;

                if (previousSettlement != null && previousSettlement != __0)
                {
                    ArmyCommandsContext.ArmyLastVisitedSettlementCache[currentArmy] = previousSettlement;
                }
            }
        }
    }
}