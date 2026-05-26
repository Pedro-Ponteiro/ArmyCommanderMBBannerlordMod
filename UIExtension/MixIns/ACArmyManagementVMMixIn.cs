using ArmyCommander.Actions;
using ArmyCommander.HarmonyPatches;
using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Library;



namespace ArmyCommander.UIExtension.MixIns
{
    [ViewModelMixin("RefreshValues")]
    public class ArmyManagementVMMixIn : BaseViewModelMixin<ArmyManagementVM>
    {
        private bool _IsArmySelected = true;
        private string _ArmyBehaviorDescription;
        private bool _TargetSettlementEnabled;
        private string _TargetSettlementName;
        private bool _SendInfluenceEnabled;
        private bool _ArmyBehaviorEnabled;
        private string _ArmyBehaviorText;

        public ArmyManagementVMMixIn(ArmyManagementVM vm) : base(vm)
        {

            new ACArmyManagementUIContext(vm, this);

            ACArmyManagementUIContext.Instance.influenceSent = 0;

        }

        public override void OnRefresh()
        {
            RefreshUIState();
        }

        private void RefreshUIState()
        {
            base.OnRefresh();
        }


        public override void OnFinalize()
        {
            base.OnFinalize();
        }


        public void UpdateContextOnFirstPartyAdded(MobileParty new_main_party)
        {

            if (new_main_party.Army != null && new_main_party.Army.LeaderParty == new_main_party)
            {
                ACArmyManagementUIContext.Instance.mainPartyHasArmy = true;

                ACArmyManagementUIContext.Instance.armyBehavior = new_main_party.Army.ArmyType;

                if (new_main_party.IsMainParty)
                {
                    ACArmyManagementUIContext.Instance.targetSettlement = new_main_party.TargetSettlement;
                }
                else
                {
                    ACArmyManagementUIContext.Instance.targetSettlement = (new_main_party.Army.AiBehaviorObject is Settlement) ? (Settlement)new_main_party.Army.AiBehaviorObject : null;
                }


            }
            else
            {
                ACArmyManagementUIContext.Instance.mainPartyHasArmy = false;
                ACArmyManagementUIContext.Instance.armyBehavior = Army.ArmyTypes.Defender;
                ACArmyManagementUIContext.Instance.targetSettlement = Hero.MainHero.HomeSettlement;

            }

            UpdateWidgets();
        }

        public void UpdateContextOnLeaderPartyRemoved()
        {
            ACArmyManagementUIContext.Instance.mainPartyHasArmy = false;
            ACArmyManagementUIContext.Instance.targetSettlement = null;
            IsArmySelectedForNewWidgets = false;
            UpdateWidgets();
        }

        public void UpdateWidgets()
        {

            if (ACArmyManagementUIContext.Instance.movieIsLoaded != true)
            {
                return;
            }


            if (ACArmyManagementUIContext.Instance.currentMainParty != MobileParty.MainParty)
            {
                IsArmySelectedForNewWidgets = true;
            }
            else
            {
                IsArmySelectedForNewWidgets = false;
            }


            if (ACArmyManagementUIContext.Instance.currentMainParty == null)
            {
                // empty armybehaviorDescription

                ArmyBehaviorDescription = " ";

                // disable choose targetsettlement and armybehavior
                TargetSettlementEnabled = false;
                TargetSettlementName = "";

                ArmyBehaviorEnabled = false;
                ArmyBehaviorText = "";

                // disable send item, send influence, send troops


                SendInfluenceEnabled = false;

            }
            else if (ACArmyManagementUIContext.Instance.mainPartyHasArmy == false)
            {
                // empty armybehaviortext
                ArmyBehaviorDescription = "Army Commands";
                // enable choose targetsettlement and armybehavior

                TargetSettlementEnabled = true;
                TargetSettlementName = ACArmyManagementUIContext.Instance.targetSettlement?.Name?.ToString() ?? "";

                ArmyBehaviorEnabled = true;
                ArmyBehaviorText = ACArmyManagementUIContext.Instance.armyBehavior.ToString();

                // disable send item, send influence, send troops

                SendInfluenceEnabled = false;


            }
            else if (ACArmyManagementUIContext.Instance.mainPartyHasArmy == true)
            {
                // update armybehaviortext
                ArmyBehaviorDescription = ACArmyManagementUIContext.Instance.currentMainParty.Army.GetLongTermBehaviorText().ToString();
                // enable and update choose targetsettlement and armybehavior (if the army is not busy already)
                TargetSettlementName = ACArmyManagementUIContext.Instance.targetSettlement?.Name?.ToString() ?? "";
                ArmyBehaviorText = ACArmyManagementUIContext.Instance.armyBehavior.ToString();

                if (ACHelpers.IsArmyAvailableForOrders(ACArmyManagementUIContext.Instance.currentMainParty.Army))
                {
                    ArmyBehaviorEnabled = true;
                    TargetSettlementEnabled = true;
                }
                else
                {
                    ArmyBehaviorEnabled = false;
                    TargetSettlementEnabled = false;
                }

                // enable send item, send influence, send troops


                SendInfluenceEnabled = true;
            }

        }


        [DataSourceProperty]
        public bool IsArmySelectedForNewWidgets
        {
            get { return _IsArmySelected; }
            set
            {

                _IsArmySelected = value;
                OnPropertyChangedWithValue(value, "ACIsValidArmySelected");

            }
        }


        [DataSourceProperty]
        public string ArmyBehaviorDescription
        {
            get { return _ArmyBehaviorDescription; }
            set
            {

                _ArmyBehaviorDescription = value;
                OnPropertyChangedWithValue(value, "ArmyBehaviorDescription");

            }
        }

        [DataSourceProperty]
        public bool TargetSettlementEnabled
        {
            get { return _TargetSettlementEnabled; }
            set
            {

                _TargetSettlementEnabled = value;
                OnPropertyChangedWithValue(value, "TargetSettlementEnabled");

            }
        }

        [DataSourceProperty]
        public string TargetSettlementName
        {
            get { return _TargetSettlementName; }
            set
            {

                _TargetSettlementName = value;
                OnPropertyChangedWithValue(value, "TargetSettlementName");

            }
        }

        [DataSourceProperty]
        public bool ArmyBehaviorEnabled
        {
            get { return _ArmyBehaviorEnabled; }
            set
            {

                _ArmyBehaviorEnabled = value;
                OnPropertyChangedWithValue(value, "ArmyBehaviorEnabled");

            }
        }

        [DataSourceProperty]
        public string ArmyBehaviorText
        {
            get { return _ArmyBehaviorText; }
            set
            {

                _ArmyBehaviorText = value;
                OnPropertyChangedWithValue(value, "ArmyBehaviorText");

            }
        }


        [DataSourceProperty]
        public bool SendInfluenceEnabled
        {
            get { return _SendInfluenceEnabled; }
            set
            {

                _SendInfluenceEnabled = value;
                OnPropertyChangedWithValue(value, "SendInfluenceEnabled");

            }
        }



        [DataSourceMethod]
        public void ExecuteSendInfluence()
        {
            // Sends 50 influence!

            if (Clan.PlayerClan.Influence >= 50)
            {
                ACActions.TransferInfluence(Clan.PlayerClan, ACArmyManagementUIContext.Instance.currentMainParty.ActualClan, 50);

                ACArmyManagementUIContext.Instance.influenceSent += 50;

                ACArmyManagementUIContext.Instance.CurrentArmyManagementVM.RefreshValues();

                ACArmyOverlayUIContext.Instance?.CurrentArmyOverlayVMMixIn.UpdateLeftArmyOverlay();

            }
        }

        [DataSourceMethod]
        public void ExecuteSelectTargetSettlement()
        {
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                "Target Settlement",
                "",
                GetAvailableSettlements(),
                isExitShown: true,
                minSelectableOptionCount: 1,
                maxSelectableOptionCount: 1,
                affirmativeText: "Select",
                negativeText: "Cancel",
                ConfirmTargetSettlementSelection,
                list => { },
                isSeachAvailable: true
                )
            );
        }

        private List<InquiryElement> GetAvailableSettlements()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();

            foreach (var settlement in Hero.MainHero.Clan.Kingdom.Settlements)
            {
                if (settlement.IsCastle || settlement.IsTown)
                {
                    InquiryElement settlement_inq = new InquiryElement(settlement, $"{settlement.Name.ToString()} (Defend)", new BannerImageIdentifier(Hero.MainHero.Clan.Kingdom.Banner));
                    inquiryElements.Add(settlement_inq);
                }
            }


            foreach (var faction in Hero.MainHero.Clan.Kingdom.FactionsAtWarWith)
            {
                if (faction.IsKingdomFaction)
                {
                    foreach (var settlement in faction.Settlements)
                    {
                        if (settlement.IsCastle || settlement.IsTown)
                        {
                            InquiryElement settlement_inq = new InquiryElement(settlement, $"{settlement.Name.ToString()} (Besiege)", new BannerImageIdentifier(faction.Banner));
                            inquiryElements.Add(settlement_inq);
                        }
                    }
                }
            }

            return inquiryElements;

        }

        private void ConfirmTargetSettlementSelection(List<InquiryElement> inquiry_element_list)
        {
            ACArmyManagementUIContext.Instance.targetSettlement = (Settlement)inquiry_element_list[0].Identifier;

            if (Hero.MainHero.Clan.Kingdom.Settlements.Contains(ACArmyManagementUIContext.Instance.targetSettlement))
            {
                ACArmyManagementUIContext.Instance.armyBehavior = Army.ArmyTypes.Defender;
            }
            else
            {
                ACArmyManagementUIContext.Instance.armyBehavior = Army.ArmyTypes.Besieger;
            }

            UpdateWidgets();

        }

    }
}