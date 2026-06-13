using ArmyCommander.Helpers;
using ArmyCommander.Store;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(DisbandArmyAction))]
    internal static class DisbandArmyAction_ApplyInternal_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            "ApplyInternal",
            new Type[]
            {
                typeof(Army),
                typeof(Army.ArmyDispersionReason)
            })]
        private static bool Prefix(Army army, ref Army.ArmyDispersionReason reason)
        {
            if (army == null)
            {
                return true;
            }
            

            if (ArmyCommandsBehaviorStore.army_commands.ContainsKey(army))
            {
                if (reason == Army.ArmyDispersionReason.Inactivity || reason == Army.ArmyDispersionReason.ObjectiveFinished)
                {
                    return false;
                }
                else if (reason == Army.ArmyDispersionReason.CohesionDepleted)
                {
                    int costForBoostingCohesion = ACHelpers.GetLostCohesionCostValue(army);
                    if (army.LeaderParty.ActualClan.Influence >= costForBoostingCohesion)
                    {
                        army.BoostCohesionWithInfluence(100 - army.Cohesion, costForBoostingCohesion);
                        return false;
                    }
                }
            }


            return true;
        }
    }
}