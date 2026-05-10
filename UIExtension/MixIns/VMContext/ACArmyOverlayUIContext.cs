using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Library;
using ArmyCommander.UIExtension.MixIns.VMItems;

namespace ArmyCommander.UIExtension.VMContext
{
    public static class ACArmyOverlayUIContext
    {
        

        public static ArmyMenuOverlayVM CurrentArmyOverlayVM { get; private set; }
        public static ArmyMenuOverlayVMMixin CurrentArmyOverlayVMMixIn { get; private set; }

        public static void RegisterVM(ArmyMenuOverlayVM vm)
        {
            CurrentArmyOverlayVM = vm;
        }

        public static void UnregisterVM(ArmyMenuOverlayVM vm)
        {
            if (ReferenceEquals(CurrentArmyOverlayVM, vm))
            {
                CurrentArmyOverlayVM = null;
            }
        }


        public static void RegisterMixIn(ArmyMenuOverlayVMMixin vm)
        {
            CurrentArmyOverlayVMMixIn = vm;
        }

        public static void UnregisterMixIn(ArmyMenuOverlayVMMixin vm)
        {
            if (ReferenceEquals(CurrentArmyOverlayVM, vm))
            {
                CurrentArmyOverlayVMMixIn = null;
            }
        }


        // currently selected army line context

        public static Army SelectedArmy;


        public static class ButtonPressStates
        {
            public static bool IsArmyCreation { get; set; }
            public static bool IsArmyManagement { get; set; }

        }


        public static class IterableWidgets
        {
            //public static MBBindingList<SelectableArmyItemPropertyVM> ArmyOverlayTopWidgets { get; set; }

            public static MBBindingList<SelectableArmyLineVM> ArmyOverlayArmyListWidgets { get; set; }

        }


    }
}