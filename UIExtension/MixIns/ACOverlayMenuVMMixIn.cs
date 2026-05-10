using ArmyCommander.Helpers;
using ArmyCommander.Patches;
using ArmyCommander.UIExtension.Context;
using ArmyCommander.UIExtension.MixIns.VMItems;
using ArmyCommander.UIExtension.VMContext;
using ArmyCommander.UIExtension.WidgetBuilders;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Library;


namespace ArmyCommander.UIExtension
{
    [ViewModelMixin("RefreshValues")]
    public sealed class ArmyMenuOverlayVMMixin : BaseViewModelMixin<ArmyMenuOverlayVM>
    {


        private ACArmyOverlayArmyList _ArmyOverlayArmiesList;
        private MBBindingList<SelectableArmyItemPropertyVM> _ArmyOverlayTopWidgets;


        #region Constructor

        public ArmyMenuOverlayVMMixin(ArmyMenuOverlayVM vm) : base(vm)
        {
            ACArmyOverlayUIContext.RegisterVM(vm);
            ACArmyOverlayUIContext.RegisterMixIn(this);

            vm.IsInitializationOver = false;
            ArmyOverlayArmiesList = new ACArmyOverlayArmyList(new MBBindingList<SelectableArmyLineVM>());
            RenewArmyList(null, refreshOverlay: false);
            vm.IsInitializationOver = true;

            //ArmyOverlayTopWidgets = new MBBindingList<SelectableArmyItemPropertyVM>();


            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, UpdateAllArmyLines);

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
            ACArmyOverlayUIContext.CurrentArmyOverlayVM.IsInitializationOver = false;

            RenewArmyList(new List<Army>() { army});

            if (army == ACArmyOverlayUIContext.SelectedArmy)
            {
                ACArmyOverlayUIContext.SelectedArmy = null;
            }


            if (ArmyOverlayArmiesList.ArmiesCount() == 0)
            {
                OnFinalizeMixIn();
            }


            ACArmyOverlayUIContext.CurrentArmyOverlayVM.IsInitializationOver = true;

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

            ACArmyOverlayUIContext.CurrentArmyOverlayVM.IsInitializationOver = false;

            RenewArmyList();

            ACArmyOverlayUIContext.CurrentArmyOverlayVM.IsInitializationOver = true;

        }

        #endregion

        #region Refresh


        public ACArmyLineUIContext UpdateLineContext(ACArmyLineUIContext context, Army army)
        {


            IEnumerable<MobileParty> all_parties_from_army = ACHelpers.get_all_parties_from_army(army);
            IEnumerable<MobileParty> parties_joining_today = ACHelpers.get_parties_joining_today(army, all_parties_from_army);

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


        public void RenewArmyList(List<Army> excluded_armies = null, bool refreshOverlay = true)
        {
            ArmyOverlayArmiesList.ClearLines();

            ACArmyLineUIContext ui_context;

            foreach (Army army in Clan.PlayerClan.Kingdom.Armies)
            {
                if (excluded_armies?.Contains(army) ?? false)
                {
                    continue;
                }

                ui_context = new ACArmyLineUIContext();
                ui_context = UpdateLineContext(ui_context, army);
                SelectableArmyLineVM army_line_widget = ACArmyLineWidgetBuilders.BuildArmyLine();
                army_line_widget.UpdateValues(ui_context);

                ArmyOverlayArmiesList.AddLine(army_line_widget);
            }

            if (refreshOverlay)
            {
                CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
                MapScreen_OnRefreshState_Patch.RefreshArmyOverlay();
            }

        }

        // on day tick
        public void UpdateAllArmyLines()
        {
            ACArmyOverlayUIContext.CurrentArmyOverlayVM.IsInitializationOver = false;

            ArmyOverlayArmiesList.UpdateValues();

            ACArmyOverlayUIContext.CurrentArmyOverlayVM.IsInitializationOver = true;
        }


        public override void OnRefresh()
        {
        }



        #endregion

        public void OnFinalizeMixIn()
        {
            CampaignEvents.HourlyTickEvent.ClearListeners(this);
        }


        #region DataSource Properties


        [DataSourceProperty]
        public MBBindingList<SelectableArmyItemPropertyVM> ArmyOverlayTopWidgets
        {
            get { return _ArmyOverlayTopWidgets; }
            set
            {
                if (!ReferenceEquals(_ArmyOverlayTopWidgets, value))
                {
                    _ArmyOverlayTopWidgets = value;
                    OnPropertyChangedWithValue(value, "ArmyOverlayTopWidgets");
                }
            }
        }

        [DataSourceProperty]
        public ACArmyOverlayArmyList ArmyOverlayArmiesList
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