using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmyCommander.Store
{
    public static class ACPermissionsStore
    {
        public static string _acKingdomIdThatAllowedPlayerMercenaryArmyLeadership;
        public static string _acKingdomIdThatAllowedPlayerVassalArmyCommand;

        public static void Reset()
        {
            _acKingdomIdThatAllowedPlayerMercenaryArmyLeadership = null;
            _acKingdomIdThatAllowedPlayerVassalArmyCommand = null;
        }
    }
}
