using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace ArmyCommander.Store
{
    public static class ArmyCommandsBehaviorStore
    {

        public static Dictionary<Army, (Army.ArmyTypes ArmyType,
            Settlement TargetSettlement,
            Settlement GatherSettlement,
            bool CanEngageEnemyParties,
            bool CanHelpAlliedParties,
            bool CanResupply,
            bool CanRunFromDanger
            )> army_commands = 
            new Dictionary<Army, (Army.ArmyTypes ArmyType, 
                Settlement TargetSettlement, 
                Settlement GatherSettlement, 
                bool CanEngageEnemyParties,
                bool CanHelpAlliedParties,
                bool CanResupply,
                bool CanRunFromDanger
                )>();



        public static void Reset()
        {
            army_commands =
            new Dictionary<Army, (Army.ArmyTypes ArmyType,
                Settlement TargetSettlement,
                Settlement GatherSettlement,
                bool CanEngageEnemyParties,
                bool CanHelpAlliedParties,
                bool CanResupply,
                bool CanRunFromDanger
                )>();
        }

    }
}
