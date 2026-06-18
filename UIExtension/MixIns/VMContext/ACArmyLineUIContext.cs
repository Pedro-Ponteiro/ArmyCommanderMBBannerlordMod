using ArmyCommander.UIExtension.MixIns.VMItems;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Party;


namespace ArmyCommander.UIExtension.Context
{
    public class ACArmyLineUIContext
    {

        private SelectableArmyLineVM _lineVM;
        public void registerLineVM(SelectableArmyLineVM vm)
        {
            _lineVM = vm;
        }


        public SelectableArmyLineVM LineVM()
        {
            return _lineVM;
        }

        public List<MobileParty> all_parties_from_army = new List<MobileParty>();

        public List<MobileParty> parties_joining_today = new List<MobileParty>();

        public MobileParty LeaderParty { get; set; }

        // onpartyattached
        public int AttachedPartiesCount { get; set; }

        public int TotalAssignedPartiesCount { get; set; }

        // onpartyattached
        public int PartiesWithinADayDistanceCount { get; set; }

        // onpartyattached
        public int MenCount { get; set; }

        public int PotentialMenCount { get; set; }

        // onpartyattached
        public int MenJoiningToday { get; set; }

        // onpartyattached and ondaytick
        public float CurrentArmyFood { get; set; }

        // onpartyattached and ondaytick
        public float TotalArmyFoodChange { get; set; }

        // ondaytick
        public float TotalArmyFood { get; set; }

        // ondaytick
        public float CurrentArmyInfluence { get; set; }

        // ondaytick
        public float DailyArmyInfluenceChange { get; set; }

        // ondaytick
        public float CurrentCohesion { get; set; }

        // onpartyattached and ondaytick
        public float DailyCohesionChange { get; set; }

        // ondaytick
        public int LostCohesionCostValue { get; set; }

        // ondaytick
        public int SendItemInfluenceCost { get; set; }

        // ondaytick
        public int DisbandInfluenceCost { get; set; }

    }

}
