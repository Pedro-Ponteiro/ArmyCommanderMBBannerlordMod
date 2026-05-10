using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Armies;


namespace ArmyCommander.UIExtension.VMContext
{
    public static class bkp_ACArmyOverlayUIContext
    {

        // Keeps track of the Armies List Panel to be used by the Army Panel
        public static KingdomArmyVM CurrentArmyVM { get; private set; }

        public static void Register(KingdomArmyVM vm)
        {
            CurrentArmyVM = vm;
        }

        public static void Unregister(KingdomArmyVM vm)
        {
            if (ReferenceEquals(CurrentArmyVM, vm))
            {
                CurrentArmyVM = null;
            }
        }
    }
}