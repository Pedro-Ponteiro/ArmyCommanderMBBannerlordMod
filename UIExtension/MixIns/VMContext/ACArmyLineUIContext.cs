using TaleWorlds.CampaignSystem.Party;


namespace ArmyCommander.UIExtension.Context
{
    public class ACArmyLineUIContext
    {

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

        // onpartyattached e ondaytick
        public float CurrentArmyFood { get; set; }

        // onpartyattached e ondaytick
        public float TotalArmyFoodChange { get; set; }

        // ondaytick
        public float TotalArmyFood { get; set; }

        // ondaytick
        public float CurrentArmyInfluence { get; set; }

        // ondaytick
        public float DailyArmyInfluenceChange { get; set; }

        // ondaytick
        public float CurrentCohesion { get; set; }

        // onpartyattached e ondaytick
        public float DailyCohesionChange { get; set; }

        // ondaytick
        public int LostCohesionCostValue { get; set; }

        // ondaytick
        public int SendItemInfluenceCost { get; set; }

        // ondaytick
        public int DisbandInfluenceCost { get; set; }

    }

}
