using ArmyCommander.HarmonyPatches;
using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using ArmyCommander.UIExtension.MixIns.VMItems;
using ArmyCommander.UIExtension.WidgetBuilders;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;


namespace ArmyCommander.UIExtension.MixIns
{
    [ViewModelMixin("RefreshValues")]
    public sealed class ArmyMenuOverlayVMMixin : BaseViewModelMixin<ArmyMenuOverlayVM>
    {


        private ACArmyOverlayArmyListVM _ArmyOverlayArmiesList;
        private string _ArmiesCount;
        private string _MenCount;
        private string _PartiesCount;
        private bool _shouldShowNextPageButton;
        private BasicTooltipViewModel _ArmiesCountHint;
        private BasicTooltipViewModel _ManCountHint;
        private BasicTooltipViewModel _PartiesCountHint;


        #region Constructor

        public ArmyMenuOverlayVMMixin(ArmyMenuOverlayVM vm) : base(vm)
        {

            new ACArmyOverlayUIContext(vm, this);


            vm.IsInitializationOver = false;

            ArmyOverlayArmiesList = new ACArmyOverlayArmyListVM(new MBBindingList<SelectableArmyLineVM>());

            if (ACArmyOverlayUIContext.Instance.SelectedArmy != null)
            {
                // refresh
                ACArmyOverlayUIContext.Instance.SelectedArmy = Hero.Find(ACArmyOverlayUIContext.Instance.SelectedArmy.LeaderParty.LeaderHero.StringId).PartyBelongedTo.Army;
            }

            RenewLeftArmyOverlay(null, refreshRightOverlay: false);
            CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();

            // TODO: !!!!!!!!!!!!
            UpdateTopWidgets();

            vm.IsInitializationOver = true;

            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, UpdateLeftArmyOverlay);

        }

        #endregion

        #region Events

        public void OnArmyDisband(Army army)
        {

            // RUNS IN ARMYPATCH.CS POSTFIX

            // Check whether this is an army from the same kingdom.
            // If so, renew the army list (overwriting the original one).
            // Check whether the selected army was the one that got disbanded.
            // If it was, set the selected army to the first one in the new list or null, then force the original UI to refresh.

            if (army.LeaderParty.ActualClan.Kingdom != Clan.PlayerClan.Kingdom)
            {
                return;
            }
            ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVM.IsInitializationOver = false;

            if (army == ACArmyOverlayUIContext.Instance.SelectedArmy)
            {
                ACArmyOverlayUIContext.Instance.SelectedArmy = Clan.PlayerClan.Kingdom.Armies.FirstOrDefault();
            }

            RenewLeftArmyOverlay(new List<Army>() { army});
            UpdateTopWidgets();


            if (ArmyOverlayArmiesList.ArmiesCount() == 0)
            {
                OnFinalize();
            }


            ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVM.IsInitializationOver = true;

        }

        public void OnArmyGathered(Army army)
        {

            // If the army belongs to the same kingdom.
            // Create the row and add it to the row list (must overwrite it with the new object).
            // If this is the only army in the new list, select it and force the original UI to refresh.

            if (army.Kingdom != Clan.PlayerClan.Kingdom)
            {
                return;
            }

            ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVM.IsInitializationOver = false;

            RenewLeftArmyOverlay();
            UpdateTopWidgets();

            ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVM.IsInitializationOver = true;

        }

        #endregion

        #region Refresh


        public ACArmyLineUIContext UpdateLineContext(ACArmyLineUIContext context, Army army)
        {


            List<MobileParty> all_parties_from_army = context.all_parties_from_army;
            List<MobileParty> parties_joining_today = context.parties_joining_today;

            context.LeaderParty = army.LeaderParty;

            context.AttachedPartiesCount = ACHelpers.GetAttachedPartiesCount(army);
            context.TotalAssignedPartiesCount = ACHelpers.GetTotalAssignedPartiesCount(army, all_parties_from_army);
            context.PartiesWithinADayDistanceCount = ACHelpers.GetPartiesWithinADayDistanceCount(army, parties_joining_today);

            context.CurrentArmyFood = ACHelpers.GetCurrentArmyFood(army);
            context.TotalArmyFood = ACHelpers.GetTotalArmyFood(army, all_parties_from_army);
            context.TotalArmyFoodChange = ACHelpers.GetTotalArmyFoodChange(army, parties_joining_today);

            context.CurrentArmyInfluence = ACHelpers.GetCurrentArmyInfluence(army);
            context.DailyArmyInfluenceChange = ACHelpers.GetDailyArmyInfluenceChange(army);

            context.CurrentCohesion = ACHelpers.GetCurrentCohesion(army);
            context.DailyCohesionChange = ACHelpers.GetDailyCohesionChange(army);

            context.MenCount = ACHelpers.GetMenCount(army);
            context.PotentialMenCount = ACHelpers.GetPotentialMenCount(army, all_parties_from_army);
            context.MenJoiningToday = ACHelpers.GetMenJoiningToday(army, parties_joining_today);

            context.LostCohesionCostValue = ACHelpers.GetLostCohesionCostValue(army);

            context.SendItemInfluenceCost = ACHelpers.GetSendItemInfluenceCost(army);
            context.DisbandInfluenceCost = ACHelpers.GetDisbandInfluenceCost(army);

            return context;

        }


        public void RenewLeftArmyOverlay(List<Army> excluded_armies = null, bool refreshRightOverlay = true)
        {
            ArmyOverlayArmiesList.ClearLines();

            ACArmyOverlayUIContext.Instance.ArmiesCount = 0;
            ACArmyOverlayUIContext.Instance.MenInArmiesCount = 0;
            ACArmyOverlayUIContext.Instance.MenInKingdomCount = 0;
            ACArmyOverlayUIContext.Instance.PartiesInArmiesCount = 0;
            ACArmyOverlayUIContext.Instance.PartiesInKingdomCount = 0;



            foreach (Army army in Clan.PlayerClan.Kingdom.Armies)
            {
                if (excluded_armies?.Contains(army) ?? false)
                {
                    continue;
                }

                bool conservativeLayout = ACHelpers.GetPotentialMenCount(army, ACHelpers.get_all_parties_from_army(army)) > 2000;


                SelectableArmyLineVM army_line_widget = ACArmyLineWidgetBuilders.BuildArmyLine(conservativeLayout);

                ArmyOverlayArmiesList.AddLine(army_line_widget);
            }

            ArmyOverlayArmiesList.UpdateValues();

            if (refreshRightOverlay)
            {
                MapScreen_OnRefreshState_Patch.RefreshArmyOverlay();
                CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
            }

            UpdateLineSelection();
            
        }

        private void UpdateTopWidgets()
        {
            ACArmiesCount = $"{ACArmyOverlayUIContext.Instance.ArmiesCount}";
            ACManCount = $"{ACArmyOverlayUIContext.Instance.MenInArmiesCount}/{ACArmyOverlayUIContext.Instance.MenInKingdomCount}";
            ACPartiesCount = $"{ACArmyOverlayUIContext.Instance.PartiesInArmiesCount}/{ACArmyOverlayUIContext.Instance.PartiesInKingdomCount}";

            ACArmiesCountHint = ACHintHelpers.GetKingdomArmiesTooltipVM(ACArmyOverlayUIContext.Instance);
            ACManCountHint = ACHintHelpers.GetKingdomManCountTooltipVM(ACArmyOverlayUIContext.Instance);
            ACPartiesCountHint = ACHintHelpers.GetKingdomPartiesTooltipVM(ACArmyOverlayUIContext.Instance);
        }

        // on day tick
        public void UpdateLeftArmyOverlay()
        {
            ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVM.IsInitializationOver = false;

            ACArmyOverlayUIContext.Instance.ArmiesCount = 0;
            ACArmyOverlayUIContext.Instance.MenInArmiesCount = 0;
            ACArmyOverlayUIContext.Instance.MenInKingdomCount = 0;
            ACArmyOverlayUIContext.Instance.PartiesInArmiesCount = 0;
            ACArmyOverlayUIContext.Instance.PartiesInKingdomCount = 0;

            ArmyOverlayArmiesList.UpdateValues();
            UpdateTopWidgets();

            ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVM.IsInitializationOver = true;
        }

        public void UpdateLineSelection()
        {
            ArmyOverlayArmiesList.UpdateSelection();
        }


        public override void OnRefresh()
        {
            UpdateLineSelection();
        }

        #endregion

        public override void OnFinalize()
        {
            CampaignEvents.HourlyTickEvent.ClearListeners(this);
        }

        public void UpdateNextButtonVisibility(bool show)
        {
            ShouldShowNextPageButton = show;
        }


        #region DataSource Properties

        [DataSourceProperty]
        public bool ShouldShowNextPageButton
        {
            get { return _shouldShowNextPageButton; }
            set
            {
                if (_shouldShowNextPageButton != value)
                {
                    _shouldShowNextPageButton = value;
                    OnPropertyChangedWithValue(value, "ShouldShowNextPageButton");
                }
            }
        }


        [DataSourceProperty]
        public BasicTooltipViewModel ACArmiesCountHint
        {
            get { return _ArmiesCountHint; }
            set
            {
                if (_ArmiesCountHint != value)
                {
                    _ArmiesCountHint = value;
                    OnPropertyChangedWithValue(value, "ACArmiesCountHint");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel ACManCountHint
        {
            get { return _ManCountHint; }
            set
            {
                if (_ManCountHint != value)
                {
                    _ManCountHint = value;
                    OnPropertyChangedWithValue(value, "ACManCountHint");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel ACPartiesCountHint
        {
            get { return _PartiesCountHint; }
            set
            {
                if (_PartiesCountHint != value)
                {
                    _PartiesCountHint = value;
                    OnPropertyChangedWithValue(value, "ACPartiesCountHint");
                }
            }
        }


        [DataSourceProperty]
        public string ACArmiesCount
        {
            get { return _ArmiesCount; }
            set
            {
                if (_ArmiesCount != value)
                {
                    _ArmiesCount = value;
                    OnPropertyChangedWithValue(value, "ACArmiesCount");
                }
            }
        }

        [DataSourceProperty]
        public string ACPartiesCount
        {
            get { return _PartiesCount; }
            set
            {
                if (_PartiesCount != value)
                {
                    _PartiesCount = value;
                    OnPropertyChangedWithValue(value, "ACPartiesCount");
                }
            }
        }

        [DataSourceProperty]
        public string ACManCount
        {
            get { return _MenCount; }
            set
            {
                if (_MenCount != value)
                {
                    _MenCount = value;
                    OnPropertyChangedWithValue(value, "ACManCount");
                }
            }
        }

        [DataSourceProperty]
        public ACArmyOverlayArmyListVM ArmyOverlayArmiesList
        {
            get { return _ArmyOverlayArmiesList; }
            set
            {
                if (!ReferenceEquals(_ArmyOverlayArmiesList, value))
                {
                    _ArmyOverlayArmiesList = value;
                    OnPropertyChangedWithValue(value, "ArmyOverlayArmiesList");
                }
            }
        }

        #endregion
    }
}
