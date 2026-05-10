using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using ArmyCommander.UIExtension.MixIns.VMItems;
using ArmyCommander.UIExtension.Context;
using ArmyCommander.Helpers;
using System.Collections.Generic;
using System.Data;



namespace ArmyCommander.UIExtension.WidgetBuilders
{
    internal static class ACArmyLineWidgetBuilders
    {


        #region Line Builder

        public static SelectableArmyLineVM BuildArmyLine()
        {

            SelectableArmyItemPropertyVM widget_army_parties_text = BuildArmyPartiesWidget();

            SelectableArmyItemPropertyVM widget_army_men_text = BuildArmyMenWidget();

            SelectableArmyItemPropertyVM widget_army_food_text = BuildArmyFoodWidget();

            SelectableArmyItemPropertyVM widget_army_influence_text = BuildArmyInfluenceWidget();

            SelectableArmyItemPropertyVM widget_army_cohesion_text = BuildArmyCohesionWidget();

            SelectableArmyItemPropertyVM widget_army_cohesioncost_text = BuildLostCohesionCostWidget();


            SelectableArmyPropertiesRow first_row = new SelectableArmyPropertiesRow(
                new MBBindingList<SelectableArmyItemPropertyVM>()
                {
                        widget_army_parties_text,
                        widget_army_men_text,
                        widget_army_food_text
                }
            );



            SelectableArmyPropertiesRow second_row = new SelectableArmyPropertiesRow(
            
                new MBBindingList<SelectableArmyItemPropertyVM>()
                {
                        widget_army_influence_text,
                        widget_army_cohesion_text,
                        widget_army_cohesioncost_text
                }
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
                (c) => null
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
                (c) => null
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
                (c) => null
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
                (c) => null
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
                (c) => null
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
                (c) => null
                );
            return item;
        }

        #endregion
    }
}
