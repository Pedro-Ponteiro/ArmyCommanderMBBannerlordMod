using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmyCommander.UIExtension.Context
{
    internal class ACArmyButtonsWindowUIContext
    {

        public static class Widgets
        {
            public static SelectableArmyItemPropertyVM ButtonManageArmy { get; set; } // TODO: Mudar o tipo?

            public static SelectableArmyItemPropertyVM ButtonSendTroops2Army { get; set; } // TODO: Mudar o tipo?

            public static SelectableArmyItemPropertyVM ButtonSendItems2Army { get; set; } // TODO: Mudar o tipo?

        }
        public static class Variables
        {
            public static int SenderInfluenceCost { get; set; }

            public static int DisbandInfluenceCost { get; set; }
        }
    }
}
