using ArmyCommander.Helpers;
using ArmyCommander.Store;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using Messages.FromClient.ToLobbyServer;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.LinQuick;
using static TaleWorlds.CampaignSystem.Party.MobileParty;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch]
    internal static class SetPartyAiActionOriginal
    {
        private static MethodBase TargetMethod()
        {
            Type detailType = AccessTools.Inner(
                typeof(SetPartyAiAction),
                "SetPartyAiActionDetail"
            );

            return AccessTools.Method(
                typeof(SetPartyAiAction),
                "ApplyInternal",
                new Type[]
                {
                    typeof(MobileParty),
                    typeof(Settlement),
                    typeof(MobileParty),
                    typeof(CampaignVec2),
                    detailType,
                    typeof(MobileParty.NavigationType),
                    typeof(bool),
                    typeof(bool)
                });
        }

        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ApplyInternal(
            MobileParty owner,
            Settlement settlement,
            MobileParty mobileParty,
            CampaignVec2 position,
            int detail,
            MobileParty.NavigationType navigationType,
            bool isFromPort,
            bool isTargetingPort)
        {
            throw new NotImplementedException("Harmony reverse patch stub (ApplyInternal).");
        }
    }


    [HarmonyPatch(typeof(SetPartyAiAction), "ApplyInternal")]
    internal static class SetPartyAiAction_ApplyInternal_Patch
    {
        private static bool Prefix(
            MobileParty owner,
            object[] __args)
        {

            string detailName = __args[4]?.ToString();
            Settlement original_target_settlement = (Settlement)__args[1];
            MobileParty engaged_party = (MobileParty)__args[2];


            bool recalculatedAI = ACAIBehaviorHelpers.AiBehaviorRecalculated(owner, detailName, original_target_settlement, engaged_party);

            return !recalculatedAI;
        }
    }
}