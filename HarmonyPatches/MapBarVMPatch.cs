using ArmyCommander.Helpers;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;

namespace ArmyCommander.HarmonyPatches
{ 
    [HarmonyPatch(typeof(MapBarVM))]
    internal static class MapBarVM_GetIsGatherArmyVisible_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetIsGatherArmyVisible")]
        private static void Postfix(MapBarVM __instance, ref bool __result)
        {
            // We need to hide this button so we can see the one behind it.
            if (__result == true && ACHelpers.ShouldShowArmyOverlayForPlayer())
            {
                __result = false;
            }
        }
    }
}
