using ArmyCommander.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using System.Reflection.Emit;

[HarmonyPatch(typeof(AiPartyThinkBehavior), "PartyHourlyAiTick")]
internal static class AiPartyThinkBehavior_PartyHourlyAiTick_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var originalGetter = AccessTools.PropertyGetter(
            typeof(Clan),
            nameof(Clan.IsUnderMercenaryService)
        );

        var replacementMethod = AccessTools.Method(
            typeof(AiPartyThinkBehavior_PartyHourlyAiTick_Patch),
            nameof(GetIsUnderMercenaryServiceForArmyGathering)
        );

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(originalGetter))
            {
                yield return new CodeInstruction(OpCodes.Call, replacementMethod)
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
}