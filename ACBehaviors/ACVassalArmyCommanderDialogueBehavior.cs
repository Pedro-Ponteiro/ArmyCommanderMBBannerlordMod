using ArmyCommander.Helpers;
using ArmyCommander.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArmyCommander.ACBehaviors
{
    internal class ACVassalArmyCommanderDialogueBehavior : CampaignBehaviorBase
    {
        private const string KingdomIdThatAllowedPlayerVassalArmyCommanderDataKey = "ArmyCommander.KingdomIdThatAllowedPlayerVassalArmyCommander.v1";
        private const string BehaviorStringId = "ArmyCommander.VassalArmyCommanderDialogueBehavior";


        private int minimumRequiredClanTier = 4;
        private int minimumRequiredLikableStat = 40;


        public static ACVassalArmyCommanderDialogueBehavior Instance { get; private set; }


        public ACVassalArmyCommanderDialogueBehavior() : base(BehaviorStringId)
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


            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(
                this,
                OnClanChangedKingdom);
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (clan.Leader.IsHumanPlayerCharacter &&
                oldKingdom != null &&
                oldKingdom.StringId == ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand &&
                oldKingdom != newKingdom
            )
            {
                ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand = null;
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData(
                KingdomIdThatAllowedPlayerVassalArmyCommanderDataKey,
                ref ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand);
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand = null;
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine(
                "ac_request_vassal_army_commander",
                "lord_talk_speak_diplomacy_2",
                "ac_request_vassal_army_commander_answer",
                "{=ACVassalArmyRequest}As your vassal commander, I ask permission to command armies in your name.",
                CanShowVassalArmyCommanderRequest,
                null,
                110,
                CanClickVassalArmyCommanderRequest);

            starter.AddDialogLine(
                "ac_grant_vassal_army_commander",
                "ac_request_vassal_army_commander_answer",
                "lord_pretalk",
                "{=ACMercArmyGranted}Very well, my noble commander. I will trust you with that authority. Gather our banners wisely.",
                null,
                GrantVassalArmyCommanderPermission,
                100);
        }

        private bool CanShowVassalArmyCommanderRequest()
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

            if (conversationHero != playerKingdom.Leader)
            {
                return false;
            }


            if (playerClan.IsUnderMercenaryService)
            {
                return false;
            }

            if (ACHelpers.HasPlayerPermissionForArmyCommand())
            {
                return false;
            }

            return true;
        }

        private bool CanClickVassalArmyCommanderRequest(out TextObject explanation)
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

            if (conversationHero != playerKingdom.Leader)
            {
                explanation = new TextObject("{=!}Only your ruler can grant this permission.");
                return false;
            }

            if (ACHelpers.HasPlayerPermissionForArmyCommand())
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

        private void GrantVassalArmyCommanderPermission()
        {
            Kingdom playerKingdom = Clan.PlayerClan?.Kingdom;

            if (playerKingdom == null)
            {
                return;
            }

            ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand = playerKingdom.StringId;

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}You are now allowed to command armies for " + playerKingdom.ToString() + ".").ToString()));
        }
    }
}
