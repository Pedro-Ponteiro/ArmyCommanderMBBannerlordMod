using ArmyCommander.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.Siege;


namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(AiPartyThinkBehavior), "PartyHourlyAiTick")]
    internal static class AiPartyThinkBehavior_PartyHourlyAiTick_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var originalMercenaryGetter = AccessTools.PropertyGetter(
                typeof(Clan),
                nameof(Clan.IsUnderMercenaryService)
            );

            var replacementMercenaryMethod = AccessTools.Method(
                typeof(AiPartyThinkBehavior_PartyHourlyAiTick_Patch),
                nameof(GetIsUnderMercenaryServiceForArmyGathering)
            );

            var originalFinalizeSiegeEvent = AccessTools.Method(
                typeof(SiegeEvent),
                nameof(SiegeEvent.FinalizeSiegeEvent)
            );

            var replacementFinalizeSiegeEvent = AccessTools.Method(
                typeof(AiPartyThinkBehavior_PartyHourlyAiTick_Patch),
                nameof(FinalizeSiegeEventIfAllowed)
            );

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(originalMercenaryGetter))
                {
                    yield return new CodeInstruction(OpCodes.Call, replacementMercenaryMethod)
                    {
                        labels = instruction.labels,
                        blocks = instruction.blocks
                    };

                    continue;
                }

                if (instruction.Calls(originalFinalizeSiegeEvent))
                {
                    yield return new CodeInstruction(OpCodes.Call, replacementFinalizeSiegeEvent)
                    {
                        labels = instruction.labels,
                        blocks = instruction.blocks
                    };

                    continue;
                }

                yield return instruction;
            }
        }

        private static bool GetIsUnderMercenaryServiceForArmyGathering(Clan clan)
        {
            if (ACHelpers.IsMercenaryArmyLeadersPolicyEnacted(clan))
            {
                return false;
            }

            return clan.IsUnderMercenaryService;
        }

        private static void FinalizeSiegeEventIfAllowed(SiegeEvent siegeEvent)
        {

            if (!ACAIBehaviorHelpers.ACShouldAttackerEndSiege(siegeEvent))
            {
                return;
            }

            siegeEvent.FinalizeSiegeEvent();
        }
    }
}