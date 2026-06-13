using ArmyCommander.Actions;
using ArmyCommander.HarmonyPatches;
using ArmyCommander.Helpers;
using ArmyCommander.Store;
using ArmyCommander.UIExtension.Context;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _IsValidArmySelected;
        private string _ArmyBehaviorDescription;
        private bool _TargetSettlementEnabled;
        private string _TargetSettlementName;
        private bool _SendInfluenceEnabled;
        private bool _ArmyBehaviorEnabled;
        private string _ArmyBehaviorText;

        private bool _CanEngageEnemyPartiesButtonEnabled;
        private string _CanEngageEnemyPartiesButtonText;

        private bool _CanHelpAlliedPartiesButtonEnabled;
        private string _CanHelpAlliedPartiesButtonText;

        private bool _CanResupplyButtonEnabled;
        private string _CanResupplyButtonText;

        private bool _CanRunFromDangerButtonEnabled;
        private string _CanRunFromDangerButtonText;
        private bool _GatherSettlementEnabled;
        private string _GatherSettlementName;
        private bool _RemoveOrdersButtonEnabled;
        private string _RemoveOrdersButtonText;

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

            Settlement possibleCapital = ACHelpers.GetPossibleCapital(Clan.PlayerClan.Kingdom);

            if (new_main_party.Army != null && new_main_party.Army.LeaderParty == new_main_party)
            {
                ACArmyManagementUIContext.Instance.mainPartyHasArmy = true;

                bool hasPlayerOrders = ArmyCommandsBehaviorStore.army_commands.TryGetValue(new_main_party.Army, out var commands);

                var defaultAiCommands = ACAIBehaviorHelpers.GetDefaultAiCommands(new_main_party.Army);

                ACArmyManagementUIContext.Instance.armyBehavior = hasPlayerOrders ? commands.ArmyType : defaultAiCommands.ArmyType;


                if (new_main_party.IsMainParty)
                {
                    ACArmyManagementUIContext.Instance.targetSettlement = new_main_party.TargetSettlement;
                    ACArmyManagementUIContext.Instance.gatherSettlement = possibleCapital;
                }
                else
                {
                    ACArmyManagementUIContext.Instance.targetSettlement = hasPlayerOrders ? commands.TargetSettlement : defaultAiCommands.TargetSettlement;
                    ACArmyManagementUIContext.Instance.gatherSettlement = hasPlayerOrders ? commands.GatherSettlement : defaultAiCommands.GatherSettlement;
                }

                if (hasPlayerOrders)
                {
                    ACArmyManagementUIContext.Instance.CanEngageEnemyParties = commands.CanEngageEnemyParties;
                    ACArmyManagementUIContext.Instance.CanHelpAlliedParties = commands.CanHelpAlliedParties;
                    ACArmyManagementUIContext.Instance.CanResupply = commands.CanResupply;
                    ACArmyManagementUIContext.Instance.CanRunFromDanger = commands.CanRunFromDanger;
                }
                else
                {
                    ACArmyManagementUIContext.Instance.CanEngageEnemyParties = defaultAiCommands.CanEngageEnemyParties;
                    ACArmyManagementUIContext.Instance.CanHelpAlliedParties = defaultAiCommands.CanHelpAlliedParties;
                    ACArmyManagementUIContext.Instance.CanResupply = defaultAiCommands.CanResupply;
                    ACArmyManagementUIContext.Instance.CanRunFromDanger = defaultAiCommands.CanRunFromDanger;
                }
            }
            else
            {
                ACArmyManagementUIContext.Instance.mainPartyHasArmy = false;
                ACArmyManagementUIContext.Instance.armyBehavior = Army.ArmyTypes.Defender;
                ACArmyManagementUIContext.Instance.targetSettlement = possibleCapital;
                ACArmyManagementUIContext.Instance.gatherSettlement = possibleCapital;
                ACArmyManagementUIContext.Instance.CanEngageEnemyParties = true;
                ACArmyManagementUIContext.Instance.CanHelpAlliedParties = true;
                ACArmyManagementUIContext.Instance.CanResupply = true;
                ACArmyManagementUIContext.Instance.CanRunFromDanger = true;
            }

            UpdateWidgets();
        }

        public void UpdateContextOnLeaderPartyRemoved()
        {
            ACArmyManagementUIContext.Instance.mainPartyHasArmy = false;
            ACArmyManagementUIContext.Instance.targetSettlement = null;
            ACArmyManagementUIContext.Instance.gatherSettlement = null;
            ACArmyManagementUIContext.Instance.CanEngageEnemyParties = false;
            ACArmyManagementUIContext.Instance.CanHelpAlliedParties = false;
            ACArmyManagementUIContext.Instance.CanResupply = false;
            ACArmyManagementUIContext.Instance.CanRunFromDanger = false;

            UpdateWidgets();
        }

        public void UpdateContextOnOrdersRemoved()
        {
            Army army = ACArmyManagementUIContext.Instance.currentMainParty.Army;

            var defaultAiCommands = ACAIBehaviorHelpers.GetDefaultAiCommands(army);

            ACArmyManagementUIContext.Instance.armyBehavior = defaultAiCommands.ArmyType;
            ACArmyManagementUIContext.Instance.targetSettlement = defaultAiCommands.TargetSettlement;
            ACArmyManagementUIContext.Instance.gatherSettlement = defaultAiCommands.GatherSettlement;

            ACArmyManagementUIContext.Instance.CanEngageEnemyParties = defaultAiCommands.CanEngageEnemyParties;
            ACArmyManagementUIContext.Instance.CanHelpAlliedParties = defaultAiCommands.CanHelpAlliedParties;
            ACArmyManagementUIContext.Instance.CanResupply = defaultAiCommands.CanResupply;
            ACArmyManagementUIContext.Instance.CanRunFromDanger = defaultAiCommands.CanRunFromDanger;

            UpdateWidgets();
        }



        public void UpdateWidgets()
        {

            if (ACArmyManagementUIContext.Instance.movieIsLoaded != true)
            {
                return;
            }


            IsArmySelectedForCommandWidgets = ACArmyManagementUIContext.Instance.currentMainParty != MobileParty.MainParty;


            if (ACArmyManagementUIContext.Instance.currentMainParty == null)
            {
                // empty armybehaviorDescription

                IsArmySelectedForCommandWidgets = false;

                ArmyBehaviorDescription = " ";

                // disable choose targetsettlement and armybehavior
                TargetSettlementEnabled = false;
                TargetSettlementName = "";

                ArmyBehaviorEnabled = false;
                ArmyBehaviorText = "";

                GatherSettlementEnabled = false;
                GatherSettlementName = "";

                CanEngageEnemyPartiesButtonEnabled = false;
                CanEngageEnemyPartiesButtonText = "";

                CanHelpAlliedPartiesButtonEnabled = false;
                CanHelpAlliedPartiesButtonText = "";

                CanResupplyButtonEnabled = false;
                CanResupplyButtonText = "";

                CanRunFromDangerButtonEnabled = false;
                CanRunFromDangerButtonText = "";

                RemoveOrdersButtonEnabled = false;
                RemoveOrdersButtonText = "Remove Orders";


                SendInfluenceEnabled = false;

            }
            else if (ACArmyManagementUIContext.Instance.mainPartyHasArmy == false)
            {
                // empty armybehaviortext

                IsArmySelectedForCommandWidgets = ACArmyManagementUIContext.Instance.currentMainParty != MobileParty.MainParty;

                ArmyBehaviorDescription = "Army Commands";
                // enable choose targetsettlement and armybehavior

                TargetSettlementEnabled = true;
                TargetSettlementName = ACArmyManagementUIContext.Instance.targetSettlement?.Name?.ToString() ?? "";

                ArmyBehaviorEnabled = true;
                ArmyBehaviorText = ACArmyManagementUIContext.Instance.armyBehavior.ToString();

                GatherSettlementEnabled = true;
                GatherSettlementName = ACArmyManagementUIContext.Instance.gatherSettlement?.Name?.ToString() ?? "";

                CanEngageEnemyPartiesButtonEnabled = true;
                CanEngageEnemyPartiesButtonText = ACArmyManagementUIContext.Instance.CanEngageEnemyParties == true ? "Engage Enemy Parties Enabled" : "Engage Enemy Parties Disabled";

                CanHelpAlliedPartiesButtonEnabled = ACArmyManagementUIContext.Instance.CanEngageEnemyParties != true;
                CanHelpAlliedPartiesButtonText = ACArmyManagementUIContext.Instance.CanHelpAlliedParties == true ? "Help Allied Parties Enabled" : "Help Allied Parties Disabled";

                CanResupplyButtonEnabled = true;
                CanResupplyButtonText = ACArmyManagementUIContext.Instance.CanResupply == true ? "Resupplying Enabled" : "Resupplying Disabled";

                CanRunFromDangerButtonEnabled = true;
                CanRunFromDangerButtonText = ACArmyManagementUIContext.Instance.CanRunFromDanger == true ? "Ignore Threats Disabled" : "Ignore Threats Enabled";

                RemoveOrdersButtonEnabled = false;
                RemoveOrdersButtonText = "Remove Orders";

                SendInfluenceEnabled = false;


            }
            else if (ACArmyManagementUIContext.Instance.mainPartyHasArmy == true)
            {

                IsArmySelectedForCommandWidgets = ACArmyManagementUIContext.Instance.currentMainParty != MobileParty.MainParty;

                // update armybehaviortext
                ArmyBehaviorDescription = ACArmyManagementUIContext.Instance.currentMainParty.Army.GetLongTermBehaviorText().ToString();
                // enable and update choose targetsettlement and armybehavior (if the army is not busy already)
                TargetSettlementName = ACArmyManagementUIContext.Instance.targetSettlement?.Name?.ToString() ?? "";
                ArmyBehaviorText = ACArmyManagementUIContext.Instance.armyBehavior.ToString();

                GatherSettlementName = ACArmyManagementUIContext.Instance.gatherSettlement?.Name?.ToString() ?? "";
                CanEngageEnemyPartiesButtonText = ACArmyManagementUIContext.Instance.CanEngageEnemyParties == true ? "Engage Enemy Parties Enabled" : "Engage Enemy Parties Disabled";
                CanHelpAlliedPartiesButtonText = ACArmyManagementUIContext.Instance.CanHelpAlliedParties == true ? "Help Allied Parties Enabled" : "Help Allied Parties Disabled";
                CanResupplyButtonText = ACArmyManagementUIContext.Instance.CanResupply == true ? "Resupplying Enabled" : "Resupplying Disabled";
                CanRunFromDangerButtonText = ACArmyManagementUIContext.Instance.CanRunFromDanger == true ? "Ignore Threats Disabled" : "Ignore Threats Enabled";
                RemoveOrdersButtonText = "Remove Orders";

                if (ACHelpers.IsArmyAvailableForOrders(ACArmyManagementUIContext.Instance.currentMainParty.Army))
                {
                    ArmyBehaviorEnabled = true;
                    TargetSettlementEnabled = true;

                    if (ACArmyManagementUIContext.Instance.currentMainParty.Army.IsWaitingForArmyMembers())
                    {
                        GatherSettlementEnabled = true;
                    }
                    else
                    {
                        GatherSettlementEnabled = false;
                    }
                    CanEngageEnemyPartiesButtonEnabled = true;
                    CanHelpAlliedPartiesButtonEnabled = ACArmyManagementUIContext.Instance.CanEngageEnemyParties != true;
                    CanResupplyButtonEnabled = true;
                    CanRunFromDangerButtonEnabled = true;
                    RemoveOrdersButtonEnabled = ArmyCommandsBehaviorStore.army_commands.ContainsKey(ACArmyManagementUIContext.Instance.currentMainParty.Army);
                }
                else
                {
                    ArmyBehaviorEnabled = false;
                    TargetSettlementEnabled = false;
                    GatherSettlementEnabled = false;
                    CanEngageEnemyPartiesButtonEnabled = false;
                    CanHelpAlliedPartiesButtonEnabled = false;
                    CanResupplyButtonEnabled = false;
                    CanRunFromDangerButtonEnabled = false;
                    RemoveOrdersButtonEnabled = ArmyCommandsBehaviorStore.army_commands.ContainsKey(ACArmyManagementUIContext.Instance.currentMainParty.Army);
                }


                SendInfluenceEnabled = true;
            }

        }


        [DataSourceProperty]
        public bool IsArmySelectedForCommandWidgets
        {
            get { return _IsValidArmySelected; }
            set
            {

                _IsValidArmySelected = value;
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
        public bool GatherSettlementEnabled
        {
            get { return _GatherSettlementEnabled; }
            set
            {

                _GatherSettlementEnabled = value;
                OnPropertyChangedWithValue(value, "GatherSettlementEnabled");

            }
        }

        [DataSourceProperty]
        public string GatherSettlementName
        {
            get { return _GatherSettlementName; }
            set
            {

                _GatherSettlementName = value;
                OnPropertyChangedWithValue(value, "GatherSettlementName");

            }
        }

        [DataSourceProperty]
        public bool CanEngageEnemyPartiesButtonEnabled
        {
            get { return _CanEngageEnemyPartiesButtonEnabled; }
            set
            {
                _CanEngageEnemyPartiesButtonEnabled = value;
                OnPropertyChangedWithValue(value, "CanEngageEnemyPartiesButtonEnabled");
            }
        }

        [DataSourceProperty]
        public string CanEngageEnemyPartiesButtonText
        {
            get { return _CanEngageEnemyPartiesButtonText; }
            set
            {
                _CanEngageEnemyPartiesButtonText = value;
                OnPropertyChangedWithValue(value, "CanEngageEnemyPartiesButtonText");
            }
        }

        [DataSourceProperty]
        public bool CanHelpAlliedPartiesButtonEnabled
        {
            get { return _CanHelpAlliedPartiesButtonEnabled; }
            set
            {
                _CanHelpAlliedPartiesButtonEnabled = value;
                OnPropertyChangedWithValue(value, "CanHelpAlliedPartiesButtonEnabled");
            }
        }

        [DataSourceProperty]
        public string CanHelpAlliedPartiesButtonText
        {
            get { return _CanHelpAlliedPartiesButtonText; }
            set
            {
                _CanHelpAlliedPartiesButtonText = value;
                OnPropertyChangedWithValue(value, "CanHelpAlliedPartiesButtonText");
            }
        }

        [DataSourceProperty]
        public bool CanResupplyButtonEnabled
        {
            get { return _CanResupplyButtonEnabled; }
            set
            {
                _CanResupplyButtonEnabled = value;
                OnPropertyChangedWithValue(value, "CanResupplyButtonEnabled");
            }
        }

        [DataSourceProperty]
        public string CanResupplyButtonText
        {
            get { return _CanResupplyButtonText; }
            set
            {
                _CanResupplyButtonText = value;
                OnPropertyChangedWithValue(value, "CanResupplyButtonText");
            }
        }

        [DataSourceProperty]
        public bool CanRunFromDangerButtonEnabled
        {
            get { return _CanRunFromDangerButtonEnabled; }
            set
            {
                _CanRunFromDangerButtonEnabled = value;
                OnPropertyChangedWithValue(value, "CanRunFromDangerButtonEnabled");
            }
        }

        [DataSourceProperty]
        public string CanRunFromDangerButtonText
        {
            get { return _CanRunFromDangerButtonText; }
            set
            {
                _CanRunFromDangerButtonText = value;
                OnPropertyChangedWithValue(value, "CanRunFromDangerButtonText");
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

        [DataSourceProperty]
        public bool RemoveOrdersButtonEnabled
        {
            get { return _RemoveOrdersButtonEnabled; }
            set
            {
                _RemoveOrdersButtonEnabled = value;
                OnPropertyChangedWithValue(value, "RemoveOrdersButtonEnabled");
            }
        }

        [DataSourceProperty]
        public string RemoveOrdersButtonText
        {
            get { return _RemoveOrdersButtonText; }
            set
            {
                _RemoveOrdersButtonText = value;
                OnPropertyChangedWithValue(value, "RemoveOrdersButtonText");
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
                GetAvailableSettlements(addEnemySettlements: true),
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

        [DataSourceMethod]
        public void ExecuteSelectGatherSettlement()
        {
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                "Gather Settlement",
                "",
                GetAvailableSettlements(),
                isExitShown: true,
                minSelectableOptionCount: 1,
                maxSelectableOptionCount: 1,
                affirmativeText: "Select",
                negativeText: "Cancel",
                ConfirmGatherSettlementSelection,
                list => { },
                isSeachAvailable: true
                )
            );
        }

        private List<InquiryElement> GetAvailableSettlements(bool addEnemySettlements=false)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();

            string command_action = addEnemySettlements ? "Defend" : "Gather";

            foreach (var settlement in Hero.MainHero.Clan.Kingdom.Settlements)
            {
                if (settlement.IsCastle || settlement.IsTown)
                {
                    InquiryElement settlement_inq = new InquiryElement(settlement, $"{settlement.Name.ToString()} ({command_action})", new BannerImageIdentifier(Hero.MainHero.Clan.Kingdom.Banner));
                    inquiryElements.Add(settlement_inq);
                }
            }

            if (!addEnemySettlements)
            {
                return inquiryElements.OrderBy((ie) => ie.Title).ToList();
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

            return inquiryElements.OrderBy((ie) => ie.ImageIdentifier.Id).ThenBy((ie) => ie.Title).ToList();

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

        private void ConfirmGatherSettlementSelection(List<InquiryElement> inquiry_element_list)
        {
            ACArmyManagementUIContext.Instance.gatherSettlement = (Settlement)inquiry_element_list[0].Identifier;

            UpdateWidgets();
        }

        [DataSourceMethod]
        public void ExecuteSelectCanEngageEnemyParties()
        {
            ShowBoolSelectionInquiry(
                "Engage Enemy Parties",
                selectedValue =>
                {
                    ACArmyManagementUIContext.Instance.CanEngageEnemyParties = selectedValue;

                    if (selectedValue == true)
                    {
                        ACArmyManagementUIContext.Instance.CanHelpAlliedParties = true;
                    }

                    UpdateWidgets();
                });
        }

        [DataSourceMethod]
        public void ExecuteSelectCanHelpAlliedParties()
        {
            ShowBoolSelectionInquiry(
                "Help Allied Parties",
                selectedValue =>
                {
                    ACArmyManagementUIContext.Instance.CanHelpAlliedParties = selectedValue;
                    UpdateWidgets();
                });
        }

        [DataSourceMethod]
        public void ExecuteSelectCanResupply()
        {
            ShowBoolSelectionInquiry(
                "Resupply",
                selectedValue =>
                {
                    ACArmyManagementUIContext.Instance.CanResupply = selectedValue;
                    UpdateWidgets();
                });
        }

        [DataSourceMethod]
        public void ExecuteSelectCanRunFromDanger()
        {
            ShowBoolSelectionInquiry(
                "Ignore Threats",
                selectedValue =>
                {
                    ACArmyManagementUIContext.Instance.CanRunFromDanger = !selectedValue;
                    UpdateWidgets();
                });
        }

        private void ShowBoolSelectionInquiry(string title, Action<bool> confirmSelection)
        {
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                title,
                "",
                GetBoolSelectionOptions(),
                isExitShown: true,
                minSelectableOptionCount: 1,
                maxSelectableOptionCount: 1,
                affirmativeText: "Select",
                negativeText: "Cancel",
                inquiryElementList =>
                {
                    if (inquiryElementList == null || inquiryElementList.Count == 0)
                    {
                        return;
                    }

                    bool selectedValue = (bool)inquiryElementList[0].Identifier;
                    confirmSelection(selectedValue);
                },
                list => { },
                isSeachAvailable: false
                )
            );
        }

        private List<InquiryElement> GetBoolSelectionOptions()
        {
            return new List<InquiryElement>
                {
                    new InquiryElement(true, "Enabled", null),
                    new InquiryElement(false, "Disabled", null)
                };
        }

        [DataSourceMethod]
        public void ExecuteRemoveOrders()
        {
            ArmyCommandsBehaviorStore.army_commands.Remove(ACArmyManagementUIContext.Instance.currentMainParty.Army);
            UpdateContextOnOrdersRemoved();
        }

    }
}