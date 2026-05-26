using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Library;
using ArmyCommander.UIExtension.MixIns;

namespace ArmyCommander.UIExtension.Context
{
    public class ACArmyOverlayUIContext
    {
        

        public ArmyMenuOverlayVM CurrentArmyOverlayVM { get; private set; }
        public ArmyMenuOverlayVMMixin CurrentArmyOverlayVMMixIn { get; private set; }

        public static ACArmyOverlayUIContext Instance { get; private set; }

        public void UnregisterInstance()
        {
            Instance = null;
        }

        public ACArmyOverlayUIContext(ArmyMenuOverlayVM currentArmyOverlayVM, ArmyMenuOverlayVMMixin currentArmyOverlayVMMixIn)
        {
            Instance = this;
            CurrentArmyOverlayVM = currentArmyOverlayVM;
            CurrentArmyOverlayVMMixIn = currentArmyOverlayVMMixIn;
        }

        // currently selected army line context

        private Army _selectedArmy;

        public Army SelectedArmy
        {
            get
            {
                return _selectedArmy;
            }
            set
            {

                if (object.ReferenceEquals(_selectedArmy, value))
                {
                    return;
                }
                _selectedArmy = value;

                CurrentArmyOverlayVMMixIn.UpdateLineSelection();
            }
        }

        public int ArmiesCount = 0;

        public int PartiesInArmiesCount = 0;

        public int PartiesInKingdomCount = 0;

        public int MenInArmiesCount = 0;

        public int MenInKingdomCount = 0;

        private bool _shouldShowNextPageBtn;

        public bool ShouldShowNextPageBtn
        {
            get
            {
                return _shouldShowNextPageBtn;
            }
            set
            {

                if (_shouldShowNextPageBtn == value)
                {
                    return;
                }

                _shouldShowNextPageBtn = value;

                CurrentArmyOverlayVMMixIn.UpdateNextButtonVisibility(value);
            }
        }

    }
}