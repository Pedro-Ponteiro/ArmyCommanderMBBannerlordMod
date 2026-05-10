using ArmyCommander.UIExtension.VMContext;
using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch]
    internal static class DefaultArmyManagementCalculationModelPatches
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(DefaultArmyManagementCalculationModel),
                "CheckPartyEligibility",
                new Type[]
                {
                    typeof(MobileParty),
                    typeof(TextObject).MakeByRefType()
                });
        }

        [HarmonyPrefix]
        private static bool CheckPartyEligibilityPrefix(
            DefaultArmyManagementCalculationModel __instance,
            MobileParty party,
            ref TextObject explanation,
            ref bool __result)
        {

            bool result = true;

            MobileParty currentMainParty = ACArmyManagementUIContext.currentMainParty;

            // TODO: VERIFICAR ONDE COLOCAR UM "?" AQUI EMBAIXO!
            Army currentArmy = ACArmyManagementUIContext.currentMainParty?.Army;

            if (PlayerSiege.PlayerSiegeEvent != null)
            {
                result = false;
                explanation = GameTexts.FindText("str_action_disabled_reason_siege");
            }
            else if (party == null)
            {
                result = false;
                explanation = new TextObject("{=f6vTzVar}Does not have a mobile party.");
            }
            else if (party.LeaderHero == Hero.MainHero.MapFaction?.Leader && (currentMainParty != null || !Hero.MainHero.IsKingdomLeader))
            {
                result = false;
                explanation = new TextObject("{=ipLqVv1f}You cannot invite the ruler's party to your army.");
            }
            else if (party.Army != null && party.Army != currentArmy)
            {
                if (!Hero.MainHero.IsKingdomLeader)
                { 
                    // Comportamento normal
                    result = false;
                    explanation = new TextObject("{=aROohsat}Already in another army.");
                }
                else if (currentMainParty == null)
                {
                    if (party.Army.LeaderParty != party)
                    {
                        // Só pode selecionar um lider para popular a direita
                        result = false;
                        explanation = new TextObject("{=aROohsat}Already in another army as a member.");
                    }
                }
                else
                {
                    result = false;
                    explanation = new TextObject("{=aROohsat}Already in another army.");
                }
            }
            else if (party.Army != null && party.Army == currentArmy)
            {
                result = false;
                explanation = new TextObject("{=Vq8yavES}Already in army.");
            }
            else if (party.MapEvent != null || party.SiegeEvent != null || (party.CurrentSettlement != null && party.CurrentSettlement.IsUnderSiege))
            {
                result = false;
                explanation = new TextObject("{=pkbUiKFJ}Currently fighting an enemy.");
            }
            else if (__instance.GetPartySizeScore(party) <= Campaign.Current.Models.ArmyManagementCalculationModel.PlayerMobilePartySizeRatioToCallToArmy)
            {
                result = false;
                explanation = new TextObject("{=SVJlOYCB}Party has less men than 40% of it's party size limit.");
            }
            else
            {
                if (!party.IsDisbanding)
                {
                    IDisbandPartyCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>();
                    if (campaignBehavior == null || !campaignBehavior.IsPartyWaitingForDisband(party))
                    {
                        float landRatio;
                        if (currentMainParty?.IsCurrentlyAtSea == true)
                        {
                            result = false;
                            explanation = ((!party.HasNavalNavigationCapability) ? new TextObject("{=nqq84Dzq}Party cannot reach your army since it has no ships.") : new TextObject("{=gFixGQsr}You cannot call a party to your army while your party is at sea."));
                        }
                        else if (party.IsInRaftState)
                        {
                            result = false;
                            explanation = new TextObject("{=TbXDmh3t}This party is lost at sea.");
                        }
                        else if (currentMainParty != null && DistanceHelper.FindClosestDistanceFromMobilePartyToMobileParty(party, currentMainParty, party.NavigationCapability, out landRatio) > Campaign.Current.Models.ArmyManagementCalculationModel.MaximumDistanceToCallToArmy)
                        {
                            result = false;
                            explanation = new TextObject("{=UINgZDN5}You can not call a party that is far away.");
                        }
                        else
                        {
                            explanation = null;
                        }
                        goto IL_0201;
                    }
                }
                result = false;
                explanation = new TextObject("{=tFGM0yav}This party is disbanding.");
            }
            goto IL_0201;


        IL_0201:
            __result = result;



            return false;
        }
    }
}
