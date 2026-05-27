using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using ArmyCommander.UIExtension.MixIns.VMItems;
using System;
using System.Collections.Generic;
using System.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;



namespace ArmyCommander.UIExtension.WidgetBuilders
{
    internal static class ACArmyLineWidgetBuilders
    {


        #region Line Builder

        public static SelectableArmyLineVM BuildArmyLine(bool conservativeLayout = false)
        {

            SelectableArmyItemPropertyVM widget_army_parties_text = BuildArmyPartiesWidget();

            SelectableArmyItemPropertyVM widget_army_men_text = BuildArmyMenWidget();

            SelectableArmyItemPropertyVM widget_army_food_text = BuildArmyFoodWidget();

            SelectableArmyItemPropertyVM widget_army_influence_text = BuildArmyInfluenceWidget();

            SelectableArmyItemPropertyVM widget_army_cohesion_text = BuildArmyCohesionWidget();

            SelectableArmyItemPropertyVM widget_army_cohesioncost_text = BuildLostCohesionCostWidget();


            MBBindingList<SelectableArmyItemPropertyVM> first_row_items = new MBBindingList<SelectableArmyItemPropertyVM>();
            MBBindingList<SelectableArmyItemPropertyVM> second_row_items = new MBBindingList<SelectableArmyItemPropertyVM>();
            first_row_items.Add(widget_army_parties_text);
            first_row_items.Add(widget_army_men_text);

            if (!conservativeLayout)
            {
                first_row_items.Add(widget_army_food_text);
            }
            else
            {
                second_row_items.Add(widget_army_food_text);
            }


            second_row_items.Add(widget_army_influence_text);
            second_row_items.Add(widget_army_cohesion_text);
            second_row_items.Add(widget_army_cohesioncost_text);

            SelectableArmyPropertiesRow first_row = new SelectableArmyPropertiesRow(
                first_row_items
            );



            SelectableArmyPropertiesRow second_row = new SelectableArmyPropertiesRow(
                second_row_items
            );

            MBBindingList<SelectableArmyPropertiesRow> rows = new MBBindingList<SelectableArmyPropertiesRow>()
                {
                    first_row,
                    second_row
                };

            SelectableArmyLineVM army_line = new SelectableArmyLineVM(
                (c) => c.LeaderParty,
                rows
                );

            return army_line;
        }

        #endregion




        #region Image Builder



        #endregion



        #region Text Builders

        public static SelectableArmyItemPropertyVM BuildArmyPartiesWidget()
        {
            SelectableArmyItemPropertyVM item = new SelectableArmyItemPropertyVM(
                SelectableArmyItemPropertyVM.PropertyTypeSprites.parties_icon,
                (c) => false,
                (c) => $"{c.AttachedPartiesCount}/{c.TotalAssignedPartiesCount}",
                (c) => c.PartiesWithinADayDistanceCount,
                (c) => ACHintHelpers.GetArmyPartiesTooltipVM(c)
                );

            return item;
        }

        public static SelectableArmyItemPropertyVM BuildArmyMenWidget()
        {
            SelectableArmyItemPropertyVM item = new SelectableArmyItemPropertyVM(
                SelectableArmyItemPropertyVM.PropertyTypeSprites.troops_icon,
                (c) => false,
                (c) => $"{c.MenCount}/{c.PotentialMenCount}",
                (c) => c.MenJoiningToday,
                (c) => ACHintHelpers.GetArmyManCountTooltipVM(c)
                );

            return item;
        }

        public static SelectableArmyItemPropertyVM BuildArmyFoodWidget()
        {
            SelectableArmyItemPropertyVM item = new SelectableArmyItemPropertyVM(
                SelectableArmyItemPropertyVM.PropertyTypeSprites.food_icon,
                (c) => c.CurrentArmyFood + c.TotalArmyFoodChange < 0,
                (c) => $"{c.CurrentArmyFood:F0}/{c.TotalArmyFood:F0}",
                (c) => (int)c.TotalArmyFoodChange,
                (c) => ACHintHelpers.GetArmyFoodTooltipVM(c)
                );

            return item;
        }

        public static SelectableArmyItemPropertyVM BuildArmyInfluenceWidget()
        {
            SelectableArmyItemPropertyVM item = new SelectableArmyItemPropertyVM(
                SelectableArmyItemPropertyVM.PropertyTypeSprites.influence_icon,
                (c) => c.CurrentArmyInfluence + c.DailyArmyInfluenceChange < 0,
                (c) => $"{c.CurrentArmyInfluence:F0}",
                (c) => (int)c.DailyArmyInfluenceChange,
                (c) => ACHintHelpers.GetInfluenceTooltipVM(c)
                );

            return item;
        }

        public static SelectableArmyItemPropertyVM BuildArmyCohesionWidget()
        {
            SelectableArmyItemPropertyVM item = new SelectableArmyItemPropertyVM(
                SelectableArmyItemPropertyVM.PropertyTypeSprites.cohesion_icon,
                (c) => c.CurrentCohesion + c.DailyCohesionChange < 0,
                (c) => $"{c.CurrentCohesion:F0}",
                (c) => (int)c.DailyCohesionChange,
                (c) => ACHintHelpers.GetCohesionTooltipVM(c)
                );

            return item;
        }

        public static SelectableArmyItemPropertyVM BuildLostCohesionCostWidget()
        {
            SelectableArmyItemPropertyVM item = new SelectableArmyItemPropertyVM(
                SelectableArmyItemPropertyVM.PropertyTypeSprites.cohesion_cost_icon,
                (c) => c.CurrentArmyInfluence - c.LostCohesionCostValue < 0,
                (c) => $"{c.LostCohesionCostValue}",
                (c) => 0,
                (c) => ACHintHelpers.GetLostCohesionCostTooltipVM(c)
                );
            return item;
        }

        #endregion
    }
}
