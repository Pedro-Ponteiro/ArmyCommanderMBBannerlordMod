using ArmyCommander.HarmonyPatches;
using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace ArmyCommander.UIExtension.MixIns.VMItems
{
    public class ACArmyOverlayArmyListVM : ViewModel
    {
        private MBBindingList<SelectableArmyLineVM> _ArmiesList;

        [DataSourceProperty]
        public MBBindingList<SelectableArmyLineVM> ArmiesList
        {
            get
            {
                return _ArmiesList;
            }
            set
            {
                if (value != _ArmiesList)
                {
                    _ArmiesList = value;
                    OnPropertyChangedWithValue(value, "ArmiesList");
                }
            }
        }



        public ACArmyOverlayArmyListVM(
            MBBindingList<SelectableArmyLineVM> armiesList
            )
        {
            ArmiesList = armiesList;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void UpdateValues()
        {


            Dictionary<Army, ACArmyLineUIContext> army_context = new Dictionary<Army, ACArmyLineUIContext>();

            foreach (var army in Clan.PlayerClan.Kingdom.Armies)
            {
                if (army.Kingdom != null)
                {
                    army_context.Add(army, new ACArmyLineUIContext());
                    ACArmyOverlayUIContext.Instance.ArmiesCount += 1;
                }
            }

            ACChatLogWidgetController.UpdateDesiredMarginBottom(ACArmyOverlayUIContext.Instance.IsExtended, ACArmyOverlayUIContext.Instance.ArmiesCount);


            foreach (var party in Clan.PlayerClan.Kingdom.AllParties)
            {

                if (party.IsCaravan || party.LeaderHero == null)
                {
                    continue;
                }


                if (party.Army != null && party.Army.Kingdom != null)
                {
                    army_context[party.Army].all_parties_from_army.Add(party);

                    if (party != party.Army.LeaderParty && party.AttachedTo == null && ACHelpers.get_days_distance(party, party.Army.LeaderParty, party.Speed) < 1)
                    {
                        army_context[party.Army].parties_joining_today.Add(party);
                    }

                    ACArmyOverlayUIContext.Instance.PartiesInArmiesCount += 1;
                    ACArmyOverlayUIContext.Instance.MenInArmiesCount += party.Party.MemberRoster.TotalManCount;

                }

                ACArmyOverlayUIContext.Instance.PartiesInKingdomCount += 1;
                ACArmyOverlayUIContext.Instance.MenInKingdomCount += party.Party.MemberRoster.TotalManCount;
            }


            int armiesListIdx = 0;
            foreach (var ac in army_context)
            {

                ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVMMixIn.UpdateLineContext(ac.Value, ac.Key);
                _ArmiesList.ElementAtOrDefault(armiesListIdx)?.UpdateValues(ac.Value);
                armiesListIdx += 1;
            }

            RefreshValues();
        }

        public void UpdateSelection()
        {
            foreach (var line in _ArmiesList)
            {
                line?.UpdateSelection();
            }

            RefreshValues();
        }

        public void AddLine(SelectableArmyLineVM new_line)
        {
            ArmiesList.Add(new_line);
            RefreshValues();
        }

        public void ClearLines()
        {
            ArmiesList.Clear();
            RefreshValues();
        }

        public int ArmiesCount()
        {
            return ArmiesList.Count;
        }

    }
}

