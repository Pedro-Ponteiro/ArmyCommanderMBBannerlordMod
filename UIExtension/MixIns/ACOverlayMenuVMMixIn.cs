using ArmyCommander.Helpers;
using ArmyCommander.HarmonyPatches;
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

            // RODA NO POSTFIX DE ARMYPATCH.CS

            // precisa verificar se é uma army do mesmo reino.
            // se sim, renova a lista de armies (vai sobrescrever a original)
            // verifica se a army selecionada era a que foi desbandada.
            // se foi, muda a army selecionada para a primeira da nova lista OU null E força o refresh da ui original

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

            // se a army for do mesmo reino
            // cria a linha, adiciona à lista de linhas (tem que sobrescrever ela com o novo obj)
            // Se é a única army da lista nova, seleciona ela e força refresh da ui original

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