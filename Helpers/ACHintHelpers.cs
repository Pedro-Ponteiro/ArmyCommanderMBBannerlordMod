using ArmyCommander.UIExtension.Context;
using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArmyCommander.Helpers
{
    public static class ACHintHelpers
    {

        #region Top Overlay Widgets Hints


        public static BasicTooltipViewModel GetKingdomPartiesTooltipVM(ACArmyOverlayUIContext context)
        {
            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = new List<TooltipProperty>
                {
                    new TooltipProperty(new TextObject("{=!}Kingdom Parties").ToString(), "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title),
                    new TooltipProperty("Parties in Armies", context.PartiesInArmiesCount.ToString(), 0),
                    new TooltipProperty("Parties Not in Armies", (context.PartiesInKingdomCount - context.PartiesInArmiesCount).ToString(), 0),
                    new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownSeperator),
                    new TooltipProperty("Total", context.PartiesInKingdomCount.ToString(), 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownResult)
                };

                return default_list;
            }
            );
        }

        public static BasicTooltipViewModel GetKingdomManCountTooltipVM(ACArmyOverlayUIContext context)
        {
            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = new List<TooltipProperty>
                {
                    new TooltipProperty(new TextObject("{=!}Kingdom Troops").ToString(), "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title)
                };


                Dictionary<FormationClass, (int TroopsInArmies, int TroopsInKingdom)> troopTypeCountDict = 
                    ACHelpers.GetTroopTypeCountDict(Clan.PlayerClan.Kingdom);


                default_list.Add(new TooltipProperty("Troops in Armies", context.MenInArmiesCount.ToString(), 0));
                default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));

                // List troop counts for each type.

                foreach (var troopTypeCount in troopTypeCountDict)
                {
                    FormationClass formationClass = troopTypeCount.Key;
                    int troopsInArmies = troopTypeCount.Value.TroopsInArmies;
                    if (troopsInArmies > 0)
                    {
                        default_list.Add(new TooltipProperty(GameTexts.FindText("str_troop_type_name", formationClass.GetName()).ToString(), troopsInArmies.ToString(), 0));
                    }
                }

                default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.DefaultSeperator));

                default_list.Add(new TooltipProperty("Troops Not in Armies", (context.MenInKingdomCount - context.MenInArmiesCount).ToString(), 0));
                default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));

                // List troop counts for each type.

                foreach (var troopTypeCount in troopTypeCountDict)
                {
                    FormationClass formationClass = troopTypeCount.Key;
                    int troopsNotInArmies = troopTypeCount.Value.TroopsInKingdom - troopTypeCount.Value.TroopsInArmies;
                    if (troopsNotInArmies > 0)
                    {
                        default_list.Add(new TooltipProperty(GameTexts.FindText("str_troop_type_name", formationClass.GetName()).ToString(), troopsNotInArmies.ToString(), 0));
                    }
                }


                default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
                default_list.Add(new TooltipProperty("Total", context.MenInKingdomCount.ToString(), 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownResult));

                return default_list;
            }
            );
        }

        public static BasicTooltipViewModel GetKingdomArmiesTooltipVM(ACArmyOverlayUIContext context)
        {
            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = new List<TooltipProperty>
                {
                    new TooltipProperty(new TextObject("{=!}Kingdom Armies").ToString(), "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title)
                };

                foreach (var army in Clan.PlayerClan.Kingdom.Armies)
                {
                    default_list.Add(new TooltipProperty(army.Name.ToString(), army.TotalManCount.ToString(), 0));
                    default_list.Add(new TooltipProperty("", army.GetLongTermBehaviorText().ToString(), 0));
                    default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.DefaultSeperator));
                }
                default_list.RemoveAt(default_list.Count - 1);


                default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
                default_list.Add(new TooltipProperty("Total", context.ArmiesCount.ToString(), 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownResult));

                return default_list;
            }
            );
        }

        #endregion

        #region Army Lines Hints

        public static BasicTooltipViewModel GetArmyPartiesTooltipVM(ACArmyLineUIContext context)
        {
            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = new List<TooltipProperty>()
                {
                    new TooltipProperty(new TextObject("{=!}Army Parties").ToString(), "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title)
                };

                foreach (var party in context.LeaderParty.AttachedParties)
                {
                    default_list.Add(new TooltipProperty(party.Name.ToString(), "", 0));
                }

                default_list.Add(new TooltipProperty("", "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));

                default_list.Add(new TooltipProperty("Total", context.AttachedPartiesCount.ToString(), 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownResult));

                default_list.Add(
                    new TooltipProperty(new TextObject("Expected Change from{newline} parties joining today").ToString(), context.PartiesWithinADayDistanceCount.ToString(), 0)
                    );
                return default_list;
                }
            );
        }

        public static BasicTooltipViewModel GetArmyManCountTooltipVM(ACArmyLineUIContext context)
        {
            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = CampaignUIHelper.GetArmyManCountTooltip(context.LeaderParty.Army);
                default_list.Add(
                    new TooltipProperty(new TextObject("Expected Change from{newline} parties joining today").ToString(), context.MenJoiningToday.ToString(), 0)
                    );
                return default_list;
                }
            );
        }

        public static BasicTooltipViewModel GetArmyFoodTooltipVM(ACArmyLineUIContext context)
        {

            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = CampaignUIHelper.GetArmyFoodTooltip(context.LeaderParty.Army);
                default_list.Add(
                    new TooltipProperty(new TextObject("Expected Change from consumption{newline} and parties joining today").ToString(), context.TotalArmyFoodChange.ToString(), 0)
                    );
                return default_list;
                }
            );
        }

        public static BasicTooltipViewModel GetCohesionTooltipVM(ACArmyLineUIContext context)
        {
            return new BasicTooltipViewModel(() => CampaignUIHelper.GetArmyCohesionTooltip(context.LeaderParty.Army));
        }


        public static BasicTooltipViewModel GetInfluenceTooltipVM(ACArmyLineUIContext context)
        {
            return new BasicTooltipViewModel(() => CampaignUIHelper.GetInfluenceTooltip(context.LeaderParty.ActualClan));
        }

        public static BasicTooltipViewModel GetLostCohesionCostTooltipVM(ACArmyLineUIContext context)
        {
            return new BasicTooltipViewModel(() => {
                List<TooltipProperty> default_list = new List<TooltipProperty>
                {
                    new TooltipProperty(new TextObject("{=!}Lost Cohesion Cost").ToString(), "", 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.Title),
                    new TooltipProperty("Total", context.LostCohesionCostValue.ToString(), 0, onlyShowWhenExtended: false, TooltipProperty.TooltipPropertyFlags.RundownResult)
                };

                return default_list;
            }
            );
        }
        #endregion

    }
}
