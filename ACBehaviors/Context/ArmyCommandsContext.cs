using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace ArmyCommander.ACBehaviors.Context
{
    public static class ArmyCommandsContext
    {
        public static Dictionary<Army, Settlement> ArmyLastVisitedSettlementCache = new Dictionary<Army, Settlement>();

        public static Dictionary<Army, (bool LookingForFood, bool LookingForTroops)> ArmyIsResupplyingDic = new Dictionary<Army, (bool LookingForFood, bool LookingForTroops)>();


        public static void Reset()
        {
            ArmyLastVisitedSettlementCache = new Dictionary<Army, Settlement>();
            ArmyIsResupplyingDic = new Dictionary<Army, (bool LookingForFood, bool LookingForTroops)>();
        }

    }
}
