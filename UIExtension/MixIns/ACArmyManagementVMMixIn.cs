using ArmyCommander.HarmonyPatches;
using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.VMContext;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.Library;



namespace ArmyCommander.UIExtension
{
    //[ViewModelMixin("RefreshValues")]
    public static class ArmyManagementVMMixIn
    {
        // : BaseViewModelMixin<ArmyManagementVM>


        private static string _ArmyBehaviorDescription;
        private static bool _TargetSettlementEnabled;
        private static bool _ArmyBehaviorEnabled;
        private static bool _SendItemEnabled;
        private static bool _SendInfluenceEnabled;
        private static string _TargetSettlementName;
        private static string _ArmyBehaviorText;

        //public ArmyManagementVMMixIn(ArmyManagementVM vm)
        //{
        //    //: base(vm)
        //    ACArmyManagementUIContext.RegisterVM(vm);
        //    ACArmyManagementUIContext.RegisterMixIn(this);



        //    //_originalOnRefresh();

        //}

        //public override void OnRefresh()
        //{
        //    RefreshUIState();
        //}

        //private void RefreshUIState()
        //{
        //    base.OnRefresh();
        //}

        public static void UpdateContextOnFirstPartyAdded(MobileParty new_main_party)
        {
            ACArmyManagementUIContext.currentMainParty = new_main_party;

            if (new_main_party.Army != null && new_main_party.Army.LeaderParty == new_main_party)
            {
                ACArmyManagementUIContext.mainPartyHasArmy = true;

                ACArmyManagementUIContext.armyBehavior = new_main_party.Army.ArmyType;

                if (new_main_party.IsMainParty)
                {
                    ACArmyManagementUIContext.targetSettlement = new_main_party.TargetSettlement;
                }
                else
                {
                    ACArmyManagementUIContext.targetSettlement = (new_main_party.Army.AiBehaviorObject is Settlement) ? (Settlement)new_main_party.Army.AiBehaviorObject : null;
                }


            }
            else
            {
                ACArmyManagementUIContext.mainPartyHasArmy = false;
                ACArmyManagementUIContext.armyBehavior = Army.ArmyTypes.Defender;
                ACArmyManagementUIContext.targetSettlement = Hero.MainHero.HomeSettlement;

            }

            UpdateWidgets();
        }

        public static void UpdateContextOnLeaderPartyRemoved()
        {
            ACArmyManagementUIContext.currentMainParty = null;
            ACArmyManagementUIContext.mainPartyHasArmy = false;
            //ACArmyManagementUIContext.armyBehavior = Army.ArmyTypes.NumberOfArmyTypes;
            ACArmyManagementUIContext.targetSettlement = null;

            UpdateWidgets();
        }

        private static void UpdateWidgets()
        {

            if (ACArmyManagementUIContext.currentMainParty == null)
            {
                // empty armybehaviorDescription

                ArmyBehaviorDescription = " ";

                // disable choose targetsettlement and armybehavior
                TargetSettlementEnabled = false;
                TargetSettlementName = "";

                ArmyBehaviorEnabled = false;
                ArmyBehaviorText = "";

                // disable send item, send influence, send troops

                SendItemEnabled = false;
                SendInfluenceEnabled = false;

            }
            else if (ACArmyManagementUIContext.mainPartyHasArmy == false)
            {
                // empty armybehaviortext
                ArmyBehaviorDescription = " ";
                // enable choose targetsettlement and armybehavior

                TargetSettlementEnabled = true;
                TargetSettlementName = ACArmyManagementUIContext.targetSettlement?.Name?.ToString() ?? "";

                ArmyBehaviorEnabled = true;
                ArmyBehaviorText = ACArmyManagementUIContext.armyBehavior.ToString();

                // disable send item, send influence, send troops
                SendItemEnabled = false;
                SendInfluenceEnabled = false;


            }
            else if (ACArmyManagementUIContext.mainPartyHasArmy == true)
            {
                // update armybehaviortext
                ArmyBehaviorDescription = ACArmyManagementUIContext.currentMainParty.Army.GetLongTermBehaviorText().ToString();
                // enable and update choose targetsettlement and armybehavior (if the army is not busy already)
                TargetSettlementName = ACArmyManagementUIContext.targetSettlement?.Name?.ToString() ?? "";
                ArmyBehaviorText = ACArmyManagementUIContext.armyBehavior.ToString();

                if (ACHelpers.IsArmyAvailableForOrders(ACArmyManagementUIContext.currentMainParty.Army))
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

                SendItemEnabled = true;
                SendInfluenceEnabled = true;
            }
        }

        [DataSourceProperty]
        public static string ArmyBehaviorDescription
        {
            get { return _ArmyBehaviorDescription; }
            set
            {
                if (_ArmyBehaviorDescription != value)
                {
                    _ArmyBehaviorDescription = value;
                    //OnPropertyChangedWithValue(value, "ArmyBehaviorDescription");
                }
            }
        }

        [DataSourceProperty]
        public static bool TargetSettlementEnabled
        {
            get { return _TargetSettlementEnabled; }
            set
            {
                if (_TargetSettlementEnabled != value)
                {
                    _TargetSettlementEnabled = value;
                    //OnPropertyChangedWithValue(value, "TargetSettlementEnabled");
                }
            }
        }

        [DataSourceProperty]
        public static string TargetSettlementName
        {
            get { return _TargetSettlementName; }
            set
            {
                if (_TargetSettlementName != value)
                {
                    _TargetSettlementName = value;
                    //OnPropertyChangedWithValue(value, "TargetSettlementName");
                }
            }
        }

        [DataSourceProperty]
        public static bool ArmyBehaviorEnabled
        {
            get { return _ArmyBehaviorEnabled; }
            set
            {
                if (_ArmyBehaviorEnabled != value)
                {
                    _ArmyBehaviorEnabled = value;
                    //OnPropertyChangedWithValue(value, "ArmyBehaviorEnabled");
                }
            }
        }

        [DataSourceProperty]
        public static string ArmyBehaviorText
        {
            get { return _ArmyBehaviorText; }
            set
            {
                if (_ArmyBehaviorText != value)
                {
                    _ArmyBehaviorText = value;
                    //OnPropertyChangedWithValue(value, "ArmyBehaviorText");
                }
            }
        }


        [DataSourceProperty]
        public static bool SendItemEnabled
        {
            get { return _SendItemEnabled; }
            set
            {
                if (_SendItemEnabled != value)
                {
                    _SendItemEnabled = value;
                    //OnPropertyChangedWithValue(value, "SendItemEnabled");
                }
            }
        }

        [DataSourceProperty]
        public static bool SendInfluenceEnabled
        {
            get { return _SendInfluenceEnabled; }
            set
            {
                if (_SendInfluenceEnabled != value)
                {
                    _SendInfluenceEnabled = value;
                    //OnPropertyChangedWithValue(value, "SendInfluenceEnabled");
                }
            }
        }

    }
}