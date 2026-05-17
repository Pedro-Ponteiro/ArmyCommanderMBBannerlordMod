using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
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

            MobileParty currentMainParty = ACArmyManagementUIContext.Instance?.currentMainParty;

            // TODO: VERIFICAR ONDE COLOCAR UM "?" AQUI EMBAIXO!
            Army currentArmy = ACArmyManagementUIContext.Instance?.currentMainParty?.Army;

            if (party == null) // OK
            {
                result = false;
                explanation = new TextObject("{=f6vTzVar}Does not have a mobile party.");
            }

            else if (ACHelpers.IsPartyBusy(party))
            {
                result = false;
                explanation = new TextObject("{=!}Party Unavailable.");
            }

            else if (ACHelpers.IsPlayerBusy())
            {
                result = false;
                explanation = new TextObject("{=!}Player unable to perform this action.");
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
            else if (__instance.GetPartySizeScore(party) <= Campaign.Current.Models.ArmyManagementCalculationModel.PlayerMobilePartySizeRatioToCallToArmy)
            {
                result = false;
                explanation = new TextObject($"{{=!}}Party has less men than {Campaign.Current.Models.ArmyManagementCalculationModel.PlayerMobilePartySizeRatioToCallToArmy:F0}% of it's party size limit.");
            }
            
            __result = result;

            return false;
        }
    }
}
