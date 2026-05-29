using ArmyCommander.Actions;
using ArmyCommander.Helpers;
using ArmyCommander.Store;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using static TaleWorlds.CampaignSystem.Army;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(Army))]
    internal static class Army_DisperseInternal_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("DisperseInternal", new Type[] { typeof(ArmyDispersionReason) })]
        private static void Postfix(Army __instance)
        {
            if (ArmyCommandsBehaviorStore.army_commands.TryGetValue(__instance, out _))
            {
                ArmyCommandsBehaviorStore.army_commands.Remove(__instance);
            }

            ACArmyOverlayUIContext.Instance?.CurrentArmyOverlayVMMixIn.OnArmyDisband(__instance);
        }
    }

    [HarmonyPatch(typeof(Army))]
    internal static class Army_Gather_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Gather", new Type[] { typeof(Settlement), typeof(MBReadOnlyList<MobileParty>) })]
        private static void Postfix(Army __instance)
        {
            if (ACHelpers.IsMercenaryArmyLeadersPolicyEnacted(__instance.LeaderParty.ActualClan))
            {
                ACActions.SubtractInfluence(100, __instance.LeaderParty.ActualClan.Kingdom.Leader.Clan);
            }

            ACArmyOverlayUIContext.Instance?.CurrentArmyOverlayVMMixIn.OnArmyGathered(__instance);
        }
    }
}