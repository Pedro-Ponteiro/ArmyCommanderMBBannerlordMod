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
    }
}