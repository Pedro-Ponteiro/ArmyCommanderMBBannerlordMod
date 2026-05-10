using ArmyCommander.UIExtension.VMContext;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;

[HarmonyPatch(typeof(CampaignUIHelper))]
internal static class CampaignUIHelper_GetCanManageCurrentArmyWithReason_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CampaignUIHelper.GetCanManageCurrentArmyWithReason))]
    private static void Postfix(ref bool __result, ref TextObject disabledReason)
    {

        if (__result == false && disabledReason.IsEmpty() && Hero.MainHero.IsKingdomLeader && ACArmyOverlayUIContext.SelectedArmy != null)
        {
            __result = true;
        }
    }
}