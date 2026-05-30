using ArmyCommander.Helpers;
using ArmyCommander.Store;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArmyCommander.ACBehaviors
{
    public class ACMercenaryArmyLeadershipDialogueBehavior : CampaignBehaviorBase
    {
        private const string KingdomIdThatAllowedPlayerMercenaryArmyLeadershipDataKey = "ArmyCommander.KingdomIdThatAllowedPlayerMercenaryArmyLeadership.v1";
        private const string BehaviorStringId = "ArmyCommander.MercenaryArmyLeadershipDialogueBehavior";

        
        private int minimumRequiredClanTier = 3;
        private int minimumRequiredLikableStat = 25;


        public static ACMercenaryArmyLeadershipDialogueBehavior Instance { get; private set; }


        public ACMercenaryArmyLeadershipDialogueBehavior(): base(BehaviorStringId)
        {
        }


        public override void RegisterEvents()
        {
            Instance = this;

            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(
                this,
                AddDialogs);

            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                this,
                OnNewGameCreated);


            CampaignEvents.OnMercenaryServiceEndedEvent.AddNonSerializedListener(
                this,
                OnMercenaryServiceEnded);

        }

        private void OnMercenaryServiceEnded(Clan clan, EndMercenaryServiceAction.EndMercenaryServiceActionDetails details)
        {
            if (clan.Leader.IsHumanPlayerCharacter)
            {
                ACPermissionsStore._acKingdomIdThatAllowedPlayerMercenaryArmyLeadership = null;
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData(
                KingdomIdThatAllowedPlayerMercenaryArmyLeadershipDataKey,
                ref ACPermissionsStore._acKingdomIdThatAllowedPlayerMercenaryArmyLeadership);
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            ACPermissionsStore._acKingdomIdThatAllowedPlayerMercenaryArmyLeadership = null;
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine(
                "ac_request_mercenary_army_leadership",
                "lord_talk_speak_diplomacy_2",
                "ac_request_mercenary_army_leadership_answer",
                "{=ACMercArmyRequest}As your hired commander, I ask permission to form and lead armies in your name.",
                CanShowMercenaryArmyLeadershipRequest,
                null,
                110,
                CanClickMercenaryArmyLeadershipRequest);

            starter.AddDialogLine(
                "ac_grant_mercenary_army_leadership",
                "ac_request_mercenary_army_leadership_answer",
                "lord_pretalk",
                "{=ACMercArmyGranted}Very well. I will trust you with that authority. Gather our banners wisely.",
                null,
                GrantMercenaryArmyLeadershipPermission,
                100);
        }

        private bool CanShowMercenaryArmyLeadershipRequest()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            Clan playerClan = Clan.PlayerClan;

            if (conversationHero == null || playerClan == null)
            {
                return false;
            }

            Kingdom playerKingdom = playerClan.Kingdom;

            if (playerKingdom == null)
            {
                return false;
            }

            if (!playerClan.IsUnderMercenaryService)
            {
                return false;
            }

            if (conversationHero != playerKingdom.Leader)
            {
                return false;
            }

            if (ACHelpers.HasPlayerPermissionForMercenaryArmyLeadership())
            {
                return false;
            }

            return true;
        }

        private bool CanClickMercenaryArmyLeadershipRequest(out TextObject explanation)
        {
            explanation = TextObject.GetEmpty();

            Clan playerClan = Clan.PlayerClan;
            Hero conversationHero = Hero.OneToOneConversationHero;
            Kingdom playerKingdom = playerClan?.Kingdom;

            if (playerClan == null || playerKingdom == null || conversationHero == null)
            {
                explanation = new TextObject("{=!}This request is not available right now.");
                return false;
            }

            if (!playerClan.IsUnderMercenaryService)
            {
                explanation = new TextObject("{=!}You must be serving as a mercenary.");
                return false;
            }

            if (conversationHero != playerKingdom.Leader)
            {
                explanation = new TextObject("{=!}Only the ruler who hired you can grant this permission.");
                return false;
            }

            if (ACHelpers.HasPlayerPermissionForMercenaryArmyLeadership())
            {
                explanation = new TextObject("{=!}You already have this permission.");
                return false;
            }

            // Should be shown if not met.
            if (conversationHero.GetRelationWithPlayer() < minimumRequiredLikableStat)
            {
                explanation = new TextObject("{=!}The King doesn't trust you for this role.");
                return false;
            }
            if (Clan.PlayerClan.Tier < minimumRequiredClanTier)
            {
                explanation = new TextObject("{=!}Your Clan Tier is not high enough.");
                return false;
            }

            return true;
        }

        private void GrantMercenaryArmyLeadershipPermission()
        {
            Kingdom playerKingdom = Clan.PlayerClan?.Kingdom;

            if (playerKingdom == null)
            {
                return;
            }

            ACPermissionsStore._acKingdomIdThatAllowedPlayerMercenaryArmyLeadership = playerKingdom.StringId;

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}You are now allowed to create and lead armies while serving this kingdom as a mercenary.").ToString()));
        }


    }
}