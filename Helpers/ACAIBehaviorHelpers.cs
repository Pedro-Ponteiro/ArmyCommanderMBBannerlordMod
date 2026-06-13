using ArmyCommander.HarmonyPatches;
using ArmyCommander.Store;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using ArmyCommander.ACBehaviors.Context;

namespace ArmyCommander.Helpers
{
    public static class ACAIBehaviorHelpers
    {

        public static (
            Army.ArmyTypes ArmyType,
            Settlement TargetSettlement,
            Settlement GatherSettlement,
            bool CanEngageEnemyParties,
            bool CanHelpAlliedParties,
            bool CanResupply,
            bool CanRunFromDanger
        ) GetDefaultAiCommands(Army army)
        {
            return (
                ArmyType: army.ArmyType,
                TargetSettlement: army.AiBehaviorObject as Settlement,
                GatherSettlement: army.IsWaitingForArmyMembers()
                    ? army.LeaderParty.TargetSettlement
                    : null,
                CanEngageEnemyParties: true,
                CanHelpAlliedParties: true,
                CanResupply: true,
                CanRunFromDanger: true
            );
        }


        public static bool ValidatePlayerCommandAndAskIfNeeded(Army army, bool isGathering)
        {
            // TODO: REFACTOR

            if (army?.LeaderParty == null)
            {
                return false;
            }

            if (!ArmyCommandsBehaviorStore.army_commands.TryGetValue(army, out var playerCommands))
            {
                return false;
            }

            Kingdom playerKingdom = Clan.PlayerClan?.Kingdom;

            bool askWaitForNewCommands = false;
            Settlement settlementToWait = null;
            string explanationString = "";

            if (isGathering)
            {
                Kingdom gatherKingdom = playerCommands.GatherSettlement?.OwnerClan?.Kingdom;

                if (gatherKingdom != playerKingdom)
                {
                    playerCommands.GatherSettlement =
                        SettlementHelper.FindNearestFortificationToMobileParty(
                            army.LeaderParty,
                            army.LeaderParty.NavigationCapability,
                            (settlement) => settlement.OwnerClan.Kingdom == playerKingdom);

                    ArmyCommandsBehaviorStore.army_commands[army] = playerCommands;
                }
            }
            else if (playerCommands.ArmyType == Army.ArmyTypes.Besieger)
            {
                Settlement targetSettlement = playerCommands.TargetSettlement;
                Kingdom targetKingdom = targetSettlement?.OwnerClan?.Kingdom;

                if (targetKingdom == null || playerKingdom == null || !targetKingdom.IsAtWarWith(playerKingdom))
                {
                    askWaitForNewCommands = true;

                    if (targetKingdom == playerKingdom)
                    {
                        settlementToWait = targetSettlement;
                        explanationString = "because they have already captured it";
                    }
                    else if (targetKingdom != null)
                    {
                        explanationString = $"because the settlement is now part of {targetKingdom}";
                    }
                    else
                    {
                        explanationString = "because the settlement is no longer held by an enemy kingdom";
                    }
                }
            }
            else if (playerCommands.ArmyType == Army.ArmyTypes.Defender)
            {
                Kingdom targetKingdom = playerCommands.TargetSettlement?.OwnerClan?.Kingdom;

                if (targetKingdom != playerKingdom)
                {
                    askWaitForNewCommands = true;

                    if (playerKingdom != null)
                    {
                        explanationString = $"because the settlement is no longer part of {playerKingdom}";
                    }
                    else
                    {
                        explanationString = "because you are no longer part of a kingdom";
                    }
                }
            }

            if (askWaitForNewCommands)
            {
                settlementToWait = settlementToWait
                    ?? FindBestSettlementForWaiting(army);

                if (settlementToWait == null)
                {
                    ArmyCommandsBehaviorStore.army_commands.Remove(army);
                    ReEnableAI(army.LeaderParty);
                    return false;
                }

                string armyActionString =
                    playerCommands.ArmyType == Army.ArmyTypes.Besieger
                        ? "besiege"
                        : "defend";

                InformationManager.ShowInquiry(new InquiryData(
                        $"{army} Requires New Orders",
                        $"A messenger from {army.LeaderParty.LeaderHero}'s army has arrived. " +
                        $"The army can no longer {armyActionString} {playerCommands.TargetSettlement} {explanationString}, " +
                        $"and its leader now awaits your decision.",
                        isAffirmativeOptionShown: true,
                        isNegativeOptionShown: true,
                        $"Wait at {settlementToWait} until new orders are given.",
                        $"Leave the decision in {army.LeaderParty.LeaderHero}'s hands.",
                        () =>
                        {
                            ApplyDefaultFallBackBehavior(army, settlementToWait);
                        },
                        () =>
                        {
                            ArmyCommandsBehaviorStore.army_commands.Remove(army);
                            ReEnableAI(army.LeaderParty);
                        }
                    ),
                    pauseGameActiveState: true,
                    prioritize: false);

                return true;
            }

            ReEnableAI(army.LeaderParty);
            return false;
        }

        public static void ApplyDefaultFallBackBehavior(Army army, Settlement settlement_to_wait)
        {
            if (!ArmyCommandsBehaviorStore.army_commands.TryGetValue(army, out var player_commands))
            {
                return;
            }


            player_commands.TargetSettlement = settlement_to_wait;
            player_commands.ArmyType = Army.ArmyTypes.Defender;
            player_commands.CanEngageEnemyParties = false;
            player_commands.CanHelpAlliedParties = false;
            player_commands.CanResupply = false;
            player_commands.CanRunFromDanger = false;
            ArmyCommandsBehaviorStore.army_commands[army] = player_commands;
            OnPlayerArmyCommandChanged(army.LeaderParty);
            ReEnableAI(army.LeaderParty);
        }

        public static void ReEnableAI(MobileParty mp, bool rethinkAtNextHourlyTick = true)
        {
            mp.Ai.SetDoNotMakeNewDecisions(doNotMakeNewDecisions: false);
            mp.Ai.RethinkAtNextHourlyTick = rethinkAtNextHourlyTick;
            mp.Ai.EnableAi();
        }


        public static bool NewArmyCommandApplied(
            MobileParty owner,
            Army.ArmyTypes c_armyType,
            Settlement c_target_settlement,
            bool isWaitingForArmyMembers)
        {

            if (isWaitingForArmyMembers)
            {
                owner.Army.AiBehaviorObject = c_target_settlement;
                CampaignVec2 gatePosition = c_target_settlement.GatePosition;

                Army_SendLeaderPartyToReachablePointAroundPosition_ReversePatch.
                    SendLeaderPartyToReachablePointAroundPositionOriginal(owner.Army, gatePosition, owner.Army.GatheringPositionMaxDistanceToTheSettlement, owner.Army.GatheringPositionMinDistanceToTheSettlement);

                return true;
            }

            if (c_armyType == Army.ArmyTypes.Besieger)
            {
                SetPartyAiActionOriginal.ApplyInternal(owner, c_target_settlement, null, CampaignVec2.Zero, 4, owner.DesiredAiNavigationType, owner.CurrentSettlement?.HasPort == true, isTargetingPort: false);
                return true;
            }
            else if (c_armyType == Army.ArmyTypes.Defender)
            {

                if (c_target_settlement.SiegeEvent == null)
                {
                    if (owner.CurrentSettlement != c_target_settlement)
                    {
                        return MoveLeaderToSettlementDefendBehavior(owner.Army, c_target_settlement);
                    }
                    else
                    {
                        return true;
                    }
                }

                SetPartyAiActionOriginal.ApplyInternal(owner, c_target_settlement, null, CampaignVec2.Zero, 7, owner.DesiredAiNavigationType, owner.CurrentSettlement?.HasPort == true, owner.IsCurrentlyAtSea);
                return true;
            }
            else
            {
                throw new NotImplementedException($"Army Command Type is invalid (should be defender or besieger, is {c_armyType.ToString()})");
            }
        }

        public static Settlement FindBestSettlementForResupplying(
            Army army,
            Settlement targetSettlement,
            bool shouldLookForFood,
            bool shouldLookForTroops)
        {
            ArmyCommandsContext.ArmyLastVisitedSettlementCache.TryGetValue(
                army,
                out Settlement penultimateVisitedSettlement);

            Settlement lastVisitedSettlement = army.LeaderParty.LastVisitedSettlement;

            Func<Settlement, bool> settlementAllowedBasedOnNecessity;

            if (shouldLookForTroops && !shouldLookForFood)
            {
                settlementAllowedBasedOnNecessity = settlement =>
                    settlement.IsTown || settlement.IsCastle || settlement.IsVillage;
            }
            else if (shouldLookForTroops && shouldLookForFood)
            {
                settlementAllowedBasedOnNecessity = settlement =>
                    settlement.IsTown || settlement.IsVillage || settlement.IsCastle;
            }
            else if (!shouldLookForTroops && shouldLookForFood)
            {
                settlementAllowedBasedOnNecessity = settlement =>
                    settlement.IsTown || settlement.IsVillage;
            }
            else
            {
                return null;
            }

            float maxDaysAwayFromTarget =
                ArmyCommandsBehaviorStore.army_commands[army].ArmyType == Army.ArmyTypes.Besieger
                    ? 10f
                    : 2f;


            Settlement bestSettlement = SettlementHelper.FindNearestSettlementToMobileParty(
                army.LeaderParty,
                army.LeaderParty.NavigationCapability,
                settlement =>
                    settlement.OwnerClan?.Kingdom?.IsAtWarWith(Clan.PlayerClan.Kingdom) != true &&
                    settlement != lastVisitedSettlement &&
                    settlement != penultimateVisitedSettlement &&
                    settlementAllowedBasedOnNecessity(settlement) &&
                    ACHelpers.IsSettlementOK(settlement) &&
                    IsSettlementCloseEnoughToTargetSettlement(army, settlement, targetSettlement, maxDaysAwayFromTarget)
            );

            return bestSettlement;
        }

        public static bool IsSettlementCloseEnoughToTargetSettlement(Army army, Settlement settlement, Settlement targetSettlement, float maxDaysAwayFromTarget)
        {
            if (targetSettlement == null)
            {
                return true;
            }

            if (army.LeaderParty.Speed <= 0f)
            {
                return false;
            }


            if (settlement == targetSettlement)
            {
                return true;
            }

            float distanceToTarget =
                DistanceHelper.FindClosestDistanceFromSettlementToSettlement(
                    settlement,
                    targetSettlement,
                    army.LeaderParty.NavigationCapability);

            float daysToTarget =
                distanceToTarget / 1.5f / CampaignTime.HoursInDay;

            return daysToTarget <= maxDaysAwayFromTarget;
        }

        public static Settlement FindBestSettlementForWaiting(Army army)
        {
            return SettlementHelper.FindNearestSettlementToMobileParty(army.LeaderParty,
                army.LeaderParty.NavigationCapability,
                (settlement) =>
                    settlement.OwnerClan?.Kingdom?.IsAtWarWith(Clan.PlayerClan.Kingdom) == false &&
                    (settlement.IsTown || settlement.IsCastle) &&
                    ACHelpers.IsSettlementOK(settlement)
            );
        }

        public static bool MoveLeaderToSettlementDefendBehavior(Army army, Settlement settlement)
        {
            SetPartyAiActionOriginal.ApplyInternal(army.LeaderParty,
                settlement,
                null,
                CampaignVec2.Zero,
                0,
                army.LeaderParty.DesiredAiNavigationType,
                army.LeaderParty.CurrentSettlement?.HasPort == true,
                army.LeaderParty.IsCurrentlyAtSea);

            return true;
        }

        public static bool MoveLeaderToSettlementResupplyBehavior(Army army, Settlement settlement)
        {

            MoveLeaderToSettlementDefendBehavior(army, settlement);

            return true;
        }


        public static bool IsEngagedPartyFightingAlly(MobileParty engaged_party)
        {
            return engaged_party?.MapEvent?.InvolvedParties?.ContainsQ(
                (pb) =>
                {
                    if (pb.IsMobile)
                    {
                        return pb.MobileParty?.ActualClan?.Kingdom == Clan.PlayerClan.Kingdom ||
                            pb.MobileParty?.ActualClan?.Kingdom.IsAllyWith(Clan.PlayerClan.Kingdom) == true;
                    }
                    else if (pb.IsSettlement)
                    {
                        return pb.Settlement.OwnerClan?.Kingdom == Clan.PlayerClan.Kingdom ||
                            pb.Settlement.OwnerClan?.Kingdom?.IsAllyWith(Clan.PlayerClan.Kingdom) == true;
                    }
                    return false;
                }
                ) == true;
        }

        public static bool IsPartyFleeing(MobileParty owner)
        {
            return MobileParty.IsFleeBehavior(owner.ShortTermBehavior) || MobileParty.IsFleeBehavior(owner.DefaultBehavior);
        }

        public static bool AiBehaviorRecalculated(MobileParty owner, string detailName, Settlement original_target_settlement, MobileParty engaged_party)
        {
            if (owner.Army == null)
            {
                return false;
            }

            if (owner.Army.LeaderParty != owner)
            {
                return false;
            }

            if (!ArmyCommandsBehaviorStore.army_commands.TryGetValue(owner.Army, out var player_commands))
            {
                return false;
            }

            bool isWaitingForArmyMembers = owner.Army.IsWaitingForArmyMembers();
            Settlement player_order_target_settlement = isWaitingForArmyMembers ? player_commands.GatherSettlement : player_commands.TargetSettlement;


            if (IsPartyFleeing(owner))
            {
                if (player_commands.CanRunFromDanger)
                {
                    return false;
                }
                else
                {
                    return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                }
            }


            if (player_commands.CanResupply && !isWaitingForArmyMembers)
            {
                (bool ShouldLookForFood, bool ShouldLookForTroops) = ACShouldArmyContinueOrStartResupply(owner.Army);
                if (ShouldLookForFood || ShouldLookForTroops)
                {
                    Settlement best_resupplying_settlement = FindBestSettlementForResupplying(owner.Army, player_order_target_settlement, ShouldLookForFood, ShouldLookForTroops);

                    return MoveLeaderToSettlementResupplyBehavior(owner.Army, best_resupplying_settlement);
                }
            }


            if (
                detailName == "PatrolAroundSettlement"
                || detailName == "PatrolAroundPoint"
                || detailName == "RaidSettlement"
                || detailName == "BesiegeSettlement"
                || detailName == "GoAroundParty"
                || detailName == "DefendParty" // this is actually "defend settlement"
                || detailName == "EscortParty"
            )
            {
                return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
            }
            else if (detailName == "GoToSettlement")
            {
                if (!player_commands.CanResupply)
                {
                    return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                }
                else
                {
                    if (Clan.PlayerClan.Kingdom.IsAtWarWith(original_target_settlement.OwnerClan?.Kingdom) || !ACHelpers.IsSettlementOK(original_target_settlement))
                    {
                        // Not going to resupply.
                        return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                    }
                    else if (!isWaitingForArmyMembers)
                    {
                        Settlement best_resupplying_settlement = FindBestSettlementForResupplying(owner.Army, player_order_target_settlement, true, true);
                        return MoveLeaderToSettlementResupplyBehavior(owner.Army, best_resupplying_settlement);
                    }
                }
            }
            else if (detailName == "EngageParty")
            {

                if (!player_commands.CanEngageEnemyParties)
                {

                    if (!player_commands.CanHelpAlliedParties)
                    {
                        return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                    }

                    // "not my problem" behavior below.
                    if (!IsEngagedPartyFightingAlly(engaged_party))
                    {
                        return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                    }
                }
            }
            else if (detailName == "NewCommandSet")
            {
                if (owner.IsEngaging)
                {

                    if (!player_commands.CanEngageEnemyParties)
                    {
                        if (player_commands.CanHelpAlliedParties && !IsEngagedPartyFightingAlly(engaged_party))
                        {
                            return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                        }
                    }
                }
                else
                {
                    return NewArmyCommandApplied(owner, player_commands.ArmyType, player_order_target_settlement, isWaitingForArmyMembers);
                }
            }

            return false;
        }


        public static bool OnPlayerArmyCommandChanged(MobileParty army_leader)
        {
            return AiBehaviorRecalculated(army_leader, "NewCommandSet", null, null);
        }

        public static bool ACShouldAttackerEndSiege(SiegeEvent siegeEvent)
        {
            var attacker = siegeEvent.BesiegerCamp.LeaderParty;

            if (attacker.Army != null && ArmyCommandsBehaviorStore.army_commands.TryGetValue(attacker.Army, out var player_commands))
            {
                if (attacker.IsFleeing())
                {
                    if (!player_commands.CanRunFromDanger)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!ACShouldArmyContinueOrStartResupply(attacker.Army).ShouldLookForFood)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public static (bool ShouldLookForFood, bool ShouldLookForTroops) ACShouldArmyContinueOrStartResupply(Army army)
        {

            // histerese

            bool isAlreadyResupplying = ArmyCommandsContext.ArmyIsResupplyingDic.ContainsKey(army);

            (bool, bool) shouldResupply;

            if (army.LeaderParty.SiegeEvent == null)
            {
                bool isBesieger = army.ArmyType == Army.ArmyTypes.Besieger;

                int foodThreshold = isBesieger
                    ? isAlreadyResupplying ? 20 : 15
                    : isAlreadyResupplying ? 15 : 10;

                float troopsThreshold = isAlreadyResupplying
                    ? 0.75f
                    : 0.65f;

                shouldResupply = IsArmyRunningLowOnFoodOrTroops(
                    army,
                    foodThreshold,
                    troopsThreshold
                );
            }
            else if (army.ArmyType == Army.ArmyTypes.Besieger &&
                     army.LeaderParty.BesiegerCamp != null)
            {
                shouldResupply = IsArmyRunningLowOnFoodOrTroops(
                    army,
                    5,
                    0f
                );
            }
            else
            {
                shouldResupply = (false, false);
            }

            if (shouldResupply.Item1 || shouldResupply.Item2)
            {
                ArmyCommandsContext.ArmyIsResupplyingDic[army] = shouldResupply;
            }
            else
            {
                ArmyCommandsContext.ArmyIsResupplyingDic.Remove(army);
            }

            return shouldResupply;
        }

        public static (bool, bool) IsArmyRunningLowOnFoodOrTroops(Army army, int minimumThresholdDaysForFood, float minimumThresholdForTroopsRatio)
        {

            float number_of_days_for_food = ACHelpers.NumberOfDaysUntilFoodRunsOff(army);

            bool running_out_of_food = false;
            bool running_out_of_troops = false;

            if (number_of_days_for_food < minimumThresholdDaysForFood)
            {
                running_out_of_food = true;
            }

            if (minimumThresholdForTroopsRatio == 0)
            {
                return (running_out_of_food, false);
            }


            float current_total_troops = army.TotalManCount;
            float potential_total_troops = army.LeaderParty.AttachedParties.Sum(mp => mp.Party.PartySizeLimit) + army.LeaderParty.Party.PartySizeLimit;

            float troops_ratio = potential_total_troops > 0f
                ? current_total_troops / potential_total_troops
                : 0f;

            if (troops_ratio < minimumThresholdForTroopsRatio)
            {
                running_out_of_troops = true;
            }

            return (running_out_of_food, running_out_of_troops);
        }
    }
}
