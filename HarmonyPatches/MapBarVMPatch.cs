using ArmyCommander.Helpers;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;

[HarmonyPatch(typeof(MapBarVM))]
internal static class MapBarVM_GetIsGatherArmyVisible_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch("GetIsGatherArmyVisible")]
    private static void Postfix(MapBarVM __instance, ref bool __result)
    {
        // provavelmente temos que desabilitar esse botão para vermos o de tras.
        if (__result == true && ACHelpers.ShouldShowArmyOverlayForPlayerKingdom())
        {
            __result = false;
        }
    }
}