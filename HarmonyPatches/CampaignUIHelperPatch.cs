using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;


namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(CampaignUIHelper))]
    internal static class CampaignUIHelper_GetCanManageCurrentArmyWithReason_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CampaignUIHelper.GetCanManageCurrentArmyWithReason))]
        private static bool GetCanManageCurrentArmyWithReasonPrefix(ref bool __result, ref TextObject disabledReason)
        {
            // This enables the gather army button from the ArmyOverlay


            disabledReason = TextObject.GetEmpty();

            if (ACHelpers.IsPlayerBusy())
            {
                disabledReason = new TextObject($"{{=!}}{Hero.MainHero.Name.ToString()} is busy.");
                __result = false;
            }
            else
            {
                if (Hero.MainHero.IsKingdomLeader)
                {
                    __result = true;
                }
                else if (Clan.PlayerClan.IsUnderMercenaryService && !ACHelpers.HasPlayerPermissionForMercenaryArmyLeadership())
                {
                    disabledReason = new TextObject("{=!}Cannot create or manage armies while at mercenary service and Mercenary Army Leaders Policy hasn't been enacted or the King hasn't given you permission.");
                    __result = false;
                }
                else if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
                {
                    disabledReason = new TextObject("{=!}Cannot create an army while already a member of one.");
                    __result = false;
                }
                else
                {
                    __result = true;
                }

            }

            return false;
        }
    }
}