using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace ArmyCommander.Helpers
{
    internal static class ACHelpers
    {


        public static bool IsSameTWObjectSafe(
            MBObjectBase a,
            MBObjectBase b,
            bool treatBothNullAsSame = true
        )
        {
            bool aIsNull = a is null;
            bool bIsNull = b is null;

            if (aIsNull && bIsNull)
            {
                return treatBothNullAsSame;
            }

            if (aIsNull || bIsNull)
            {
                return false;
            }

            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a.GetType() != b.GetType())
            {
                return false;
            }

            if (a.Id.Equals(b.Id))
            {
                return true;
            }

            return !string.IsNullOrEmpty(a.StringId)
                && string.Equals(a.StringId, b.StringId, StringComparison.Ordinal);
        }

        public static bool IsArmyAvailableForOrders(Army army)
        {

            MobileParty leader_party = army.LeaderParty;

            if (leader_party.MapEvent != null || leader_party.SiegeEvent != null || (leader_party.CurrentSettlement != null && leader_party.CurrentSettlement.IsUnderSiege))
            {
                return false;
            }

            if (PlayerSiege.PlayerSiegeEvent != null)
            {
                return false;
            }


            return true;
        }


        public static bool IsPlayerBusy()
        {

            if (
                (PlayerEncounter.Current != null && !IsSettlementOK(PlayerEncounter.EncounterSettlement)) ||
                MapEvent.PlayerMapEvent != null ||
                CampaignMission.Current != null ||
                PlayerSiege.PlayerSiegeEvent != null
                ) 
            {
                return true;
            }

            if (
               IsPartyBusy(MobileParty.MainParty)
            )
            {
                return true;
            }



            return false;
        }

        public static bool IsPartyBusy(MobileParty mp)
        {
            if (
                mp.LeaderHero.IsPrisoner ||
                mp.MapEvent != null ||
                mp.IsCurrentlyAtSea ||
                mp.IsInRaftState ||
                mp.SiegeEvent != null ||
                !IsSettlementOK(mp.CurrentSettlement) ||
                mp.IsDisbanding ||
                Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>()?.IsPartyWaitingForDisband(mp) == true
            )
            {
                return true;
            }

            return false;
        }

        public static bool IsSettlementOK(Settlement settlement)
        {

            if (settlement == null)
            {
                // settlement doesnt exist, is it possibly worse? Not OK!
                return false;
            }

            if (settlement.IsVillage)
            {
                return settlement.Village.VillageState != Village.VillageStates.BeingRaided;
            }

            return settlement.IsUnderSiege != true;

        }


        public static bool ShouldShowArmyOverlayForPlayer()
        {
            if (Clan.PlayerClan?.Kingdom != null)
            {
                return Clan.PlayerClan.Kingdom.Armies.Count > 0 && MobileParty.MainParty?.IsActive == true;
            }
            else
            {
                // In vanilla, always a prisoner if Army != null.
                return Hero.MainHero.PartyBelongedTo?.Army != null && MobileParty.MainParty?.IsActive == true;
            }
        }


        public static bool IsPlayerKingdomLeader(Army army)
        {
            return army.Kingdom.Leader == Hero.MainHero;
        }

        public static float get_days_distance(MobileParty mp_called, MobileParty mp_main_party, float mp_called_speed)
        {

            float total_hours = (DistanceHelper.FindClosestDistanceFromMobilePartyToMobileParty(mp_called, mp_main_party, mp_called.NavigationCapability) / mp_called_speed);
            return total_hours / CampaignTime.HoursInDay;
        }

        public static IEnumerable<MobileParty> get_all_parties_from_army(Army army)
        {
            return army.Kingdom.AllParties.Where((mp) => { return mp.Army != null && mp.Army == army; });
        }

        public static IEnumerable<MobileParty> get_parties_joining_today(Army army, IEnumerable<MobileParty> all_parties_from_army)
        {
            return all_parties_from_army.Where(mp => mp != army.LeaderParty && mp.AttachedTo == null && get_days_distance(mp, army.LeaderParty, mp.Speed) < 1);
        }


        // onpartyattached
        public static int GetAttachedPartiesCount(Army army)
        {
            return army.LeaderParty.AttachedParties.Count + 1;
        }

        public static int GetTotalAssignedPartiesCount(Army army, IEnumerable<MobileParty> all_parties_from_army)
        {
            return all_parties_from_army.Count();
        }

        // onpartyattached
        public static int GetPartiesWithinADayDistanceCount(Army army, IEnumerable<MobileParty> parties_joining_today)
        {
            return parties_joining_today.Count();
        }

        // onpartyattached
        public static int GetMenCount(Army army)
        {
            return army.TotalManCount;
        }

        public static int GetPotentialMenCount(Army army, IEnumerable<MobileParty> all_parties_from_army)
        {
            return all_parties_from_army.Sum(x => x.Party.NumberOfAllMembers);
        }

        // onpartyattached
        public static int GetMenJoiningToday(Army army, IEnumerable<MobileParty> parties_joining_today)
        {
            return parties_joining_today.Sum(x => x.Party.NumberOfAllMembers);
        }

        // onpartyattached e ondaytick
        public static float GetCurrentArmyFood(Army army)
        {
            return army.LeaderParty.AttachedParties.Sum(x => x.Food) + army.LeaderParty.Food;
        }

        // onpartyattached e ondaytick
        public static float GetTotalArmyFoodChange(Army army, IEnumerable<MobileParty> parties_joining_today)
        {
            return (parties_joining_today.Sum(x => x.Food + x.FoodChange)
                + army.LeaderParty.AttachedParties.Sum(x => x.FoodChange)
                + army.LeaderParty.FoodChange
                );
        }

        // ondaytick
        public static float GetTotalArmyFood(Army army, IEnumerable<MobileParty> all_parties_from_army)
        {
            return all_parties_from_army.Sum(mp => mp.Food);
        }

        // ondaytick
        public static float GetCurrentArmyInfluence(Army army)
        {
            return army.LeaderParty.LeaderHero != null
                ? army.LeaderParty.LeaderHero.Clan.Influence
                : 0f;
        }

        // ondaytick
        public static float GetDailyArmyInfluenceChange(Army army)
        {
            return army.LeaderParty.LeaderHero != null
                ? army.LeaderParty.LeaderHero.Clan.InfluenceChangeExplained.ResultNumber
                : 0f;
        }

        // ondaytick
        public static float GetCurrentCohesion(Army army)
        {
            return army.Cohesion;
        }

        // onpartyattached e ondaytick
        public static float GetDailyCohesionChange(Army army)
        {
            return army.DailyCohesionChange;
        }

        // ondaytick
        public static int GetLostCohesionCostValue(Army army)
        {
            return Campaign.Current.Models.ArmyManagementCalculationModel.CalculateTotalInfluenceCost(army, 100 - army.Cohesion);
        }

        // ondaytick
        public static int GetSendItemInfluenceCost(Army army)
        {
            return GetLostCohesionCostValue(army);
        }

        // ondaytick
        public static int GetDisbandInfluenceCost(Army army)
        {
            return 50;
        }

    }
}
