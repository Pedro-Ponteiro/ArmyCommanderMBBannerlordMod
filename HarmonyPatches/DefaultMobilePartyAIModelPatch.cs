using ArmyCommander.Helpers;
using ArmyCommander.Store;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultMobilePartyAIModel))]
    internal static class DefaultMobilePartyAIModel_GetBestInitiativeBehavior_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DefaultMobilePartyAIModel.GetBestInitiativeBehavior))]
        private static void Postfix(
            MobileParty mobileParty,
            ref AiBehavior bestInitiativeBehavior,
            ref MobileParty bestInitiativeTargetParty,
            ref float bestInitiativeBehaviorScore,
            ref Vec2 averageEnemyVec)
        {
            if (mobileParty.Army == null)
            {
                return;
            }

            if (!ArmyCommandsBehaviorStore.army_commands.TryGetValue(
                    mobileParty.Army,
                    out var playerCommands))
            {
                return;
            }


            if (mobileParty.Army.LeaderParty != mobileParty)
            {
                return;
            }


            if (bestInitiativeBehavior == AiBehavior.EngageParty &&
                !playerCommands.CanEngageEnemyParties)
            {

                if (playerCommands.CanHelpAlliedParties && ACAIBehaviorHelpers.IsEngagedPartyFightingAlly(bestInitiativeTargetParty))
                {
                    return;
                }


                ClearInitiativeBehavior(
                    ref bestInitiativeBehavior,
                    ref bestInitiativeTargetParty,
                    ref bestInitiativeBehaviorScore,
                    ref averageEnemyVec);

                return;
            }

            if (IsFleeBehavior(bestInitiativeBehavior) &&
                !playerCommands.CanRunFromDanger)
            {
                ClearInitiativeBehavior(
                    ref bestInitiativeBehavior,
                    ref bestInitiativeTargetParty,
                    ref bestInitiativeBehaviorScore,
                    ref averageEnemyVec);

                return;
            }
        }

        private static void ClearInitiativeBehavior(
            ref AiBehavior bestInitiativeBehavior,
            ref MobileParty bestInitiativeTargetParty,
            ref float bestInitiativeBehaviorScore,
            ref Vec2 averageEnemyVec)
        {
            bestInitiativeBehavior = AiBehavior.None;
            bestInitiativeTargetParty = null;
            bestInitiativeBehaviorScore = 0f;
            averageEnemyVec = Vec2.Zero;
        }

        private static bool IsFleeBehavior(AiBehavior behavior)
        {
            return behavior == AiBehavior.FleeToPoint ||
                   behavior == AiBehavior.FleeToGate ||
                   behavior == AiBehavior.FleeToParty;
        }
    }
}