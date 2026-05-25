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
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultArmyManagementCalculationModel))]
    internal static class DefaultArmyManagementCalculationModel_CheckPartyEligibility_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CheckPartyEligibility")]
        private static bool Prefix(
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
                explanation = new TextObject($"{{=!}}Party has less men than 40% of it's party size limit.");
            }

            __result = result;

            return false;
        }
    }

    [HarmonyPatch(typeof(DefaultArmyManagementCalculationModel))]
    internal static class DefaultArmyManagementCalculationModel_CanLordCreateArmy_Patch
    {


        [HarmonyReversePatch]
        [HarmonyPatch("GetInfluenceBudgetWhileCreatingArmy", new Type[] { typeof(MobileParty) })]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float GetInfluenceBudgetWhileCreatingArmy(
            DefaultArmyManagementCalculationModel __instance,
            MobileParty mobileParty)
        {
            throw new NotImplementedException("Harmony reverse patch stub (GetInfluenceBudgetWhileCreatingArmy).");
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(DefaultArmyManagementCalculationModel.CanLordCreateArmy))]
        private static bool Prefix(
            DefaultArmyManagementCalculationModel __instance,
            MobileParty mobileParty,
            ref MBList<MobileParty> possibleArmyMembers,
            ref bool __result)
        {

            possibleArmyMembers = new MBList<MobileParty>();
            Kingdom kingdom = mobileParty.MapFaction as Kingdom;

            // Check party status:

            if (mobileParty.IsCurrentlyAtSea // at sea
                || mobileParty.LeaderHero.Clan.Influence <= 100f // not enough influence
                //|| mobileParty.LeaderHero.Clan.IsUnderMercenaryService // is merc (TODO!)
                || (float)mobileParty.GetNumDaysForFoodToLast() <= Campaign.Current.Models.MobilePartyAIModel.NeededFoodsInDaysThresholdForSiege // not enough food
                || !kingdom.FactionsAtWarWith.AnyQ((IFaction x) => x.Fiefs.Any()) // no possible target settlement for besiege
                || mobileParty.PartySizeRatio <= Campaign.Current.Models.ArmyManagementCalculationModel.AIMobilePartySizeRatioToCallToArmy // not enough men
                || !(mobileParty.LeaderHero.Clan.Leader == mobileParty.LeaderHero
                        || (mobileParty.LeaderHero.Clan.Leader.PartyBelongedTo == null && mobileParty.LeaderHero.Clan.WarPartyComponents != null && mobileParty.LeaderHero.Clan.WarPartyComponents.FirstOrDefault() == mobileParty.WarPartyComponent)
                    ) // Mobile party is not from clan leader and mobile party is not required to form an army if the clan leader is not available.
                )
            {
                __result = false;
                return false;
            }

            GetInfluenceBudgetWhileCreatingArmy(__instance, mobileParty);
            List<(MobileParty, float, int)> list = new List<(MobileParty, float, int)>();
            foreach (WarPartyComponent warPartyComponent in mobileParty.MapFaction.WarPartyComponents)
            {
                MobileParty mobileParty2 = warPartyComponent.MobileParty;
                Hero leaderHero = mobileParty2.LeaderHero;
                if (!mobileParty2.IsLordParty || mobileParty2.Army != null || mobileParty2 == mobileParty || leaderHero == null || mobileParty2.IsMainParty || leaderHero == leaderHero.MapFaction.Leader || mobileParty2.Ai.DoNotMakeNewDecisions || mobileParty2.CurrentSettlement?.SiegeEvent != null || mobileParty2.IsDisbanding || !((float)mobileParty2.GetNumDaysForFoodToLast() > Campaign.Current.Models.ArmyManagementCalculationModel.MinimumNeededFoodInDaysToCallToArmy) || !(mobileParty2.PartySizeRatio > Campaign.Current.Models.ArmyManagementCalculationModel.AIMobilePartySizeRatioToCallToArmy) || !leaderHero.CanLeadParty() || mobileParty2.IsInRaftState || mobileParty2.MapEvent != null || mobileParty2.BesiegedSettlement != null)
                {
                    continue;
                }
                IDisbandPartyCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>();
                if (campaignBehavior != null && campaignBehavior.IsPartyWaitingForDisband(mobileParty2))
                {
                    continue;
                }
                float maximumDistanceToCallToArmy = Campaign.Current.Models.ArmyManagementCalculationModel.MaximumDistanceToCallToArmy;
                if (!(DistanceHelper.GetDistanceBetweenMobilePartyToMobileParty(mobileParty2, mobileParty, mobileParty2.NavigationCapability, out var _) < maximumDistanceToCallToArmy))
                {
                    continue;
                }
                bool flag = false;
                foreach (var item2 in list)
                {
                    if (item2.Item1 == mobileParty2)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    int num = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(mobileParty, mobileParty2);
                    float estimatedStrength = mobileParty2.Party.EstimatedStrength;
                    float num2 = 1f - (float)mobileParty2.Party.MemberRoster.TotalWounded / (float)mobileParty2.Party.MemberRoster.TotalManCount;
                    float item = estimatedStrength / ((float)num + 0.1f) * num2;
                    list.Add((mobileParty2, item, num));
                }
            }
            
            list = list.OrderByQ(((MobileParty, float, int) x) => x.Item2).ToListQ();
            int KingdomWPCCount = kingdom.WarPartyComponents.Count;
            int numberOfPartiesInArmies = kingdom.Armies.SumQ((Army x) => x.Parties.Count);

            // TODO: adjust 70 percent using the MCM.
            int numberOfAvailableWPCsInKingdomStrategy = MathF.Ceiling((float)KingdomWPCCount * 0.7f - (float)numberOfPartiesInArmies);

            if (numberOfAvailableWPCsInKingdomStrategy > 0)
            {
                if (numberOfAvailableWPCsInKingdomStrategy < list.Count)
                {
                    list.RemoveRange(numberOfAvailableWPCsInKingdomStrategy, list.Count - numberOfAvailableWPCsInKingdomStrategy);
                }

                possibleArmyMembers = list.SelectQ(((MobileParty, float, int) x) => x.Item1).ToMBList();

                if (possibleArmyMembers.AnyQ())
                {
                    if (kingdom.Settlements.Count == 0)
                    {
                        __result = true;
                        return false;
                    }
                    float num5 = mobileParty.Party.GetCustomStrength(BattleSideEnum.Attacker, MapEvent.PowerCalculationContext.Siege);
                    foreach (MobileParty possibleArmyMember in possibleArmyMembers)
                    {
                        num5 += possibleArmyMember.Party.GetCustomStrength(BattleSideEnum.Attacker, MapEvent.PowerCalculationContext.Siege);
                    }

                    // TODO: MCM: adjust the strengh required
                    if (num5 < 1000f)
                    {
                        possibleArmyMembers.Clear();
                        __result = false;
                        return false;
                    }

                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;

        }
    }

}
