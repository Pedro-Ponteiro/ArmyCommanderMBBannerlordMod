using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace ArmyCommander.BehaviorStore
{
    public static class ArmyCommandsStore
    {

        public static Dictionary<Army, (Army.ArmyTypes ArmyType, Settlement Settlement)> army_commands = new Dictionary<Army, (Army.ArmyTypes ArmyType, Settlement Settlement)>();


    }
}
