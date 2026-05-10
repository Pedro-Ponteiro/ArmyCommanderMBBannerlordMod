using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;


namespace ArmyCommander.UIExtension.VMContext
{
    public static class ACArmyManagementUIContext
    {

        // Keeps track of the Armies List Panel to be used by the Army Panel
        public static ArmyManagementVM CurrentArmyManagementVM { get; private set; }
        //public static ArmyManagementVMMixIn CurrentArmyManagementVMMixIn { get; private set; }

        

        public static void RegisterVM(ArmyManagementVM vm)
        {
            CurrentArmyManagementVM = vm;
        }

        public static void UnregisterVM(ArmyManagementVM vm)
        {
            if (ReferenceEquals(CurrentArmyManagementVM, vm))
            {
                CurrentArmyManagementVM = null;
            }
        }

        //public static void RegisterMixIn(ArmyManagementVMMixIn vmmixin)
        //{
        //    CurrentArmyManagementVMMixIn = vmmixin;
        //}

        //public static void UnregisterMixIn(ArmyManagementVMMixIn vmmixin)
        //{
        //    if (ReferenceEquals(CurrentArmyManagementVMMixIn, vmmixin))
        //    {
        //        CurrentArmyManagementVMMixIn = null;
        //    }
        //}

        public static MobileParty currentMainParty { get; set; }

        public static bool mainPartyHasArmy { get; set; }


        public static Settlement targetSettlement { get; set; }

        public static Army.ArmyTypes  armyBehavior { get; set; }

        public static string longTermBehaviorText { get; set; }

    }
}