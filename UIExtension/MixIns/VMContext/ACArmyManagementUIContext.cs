using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using ArmyCommander.UIExtension.MixIns;

namespace ArmyCommander.UIExtension.Context
{
    public class ACArmyManagementUIContext
    {
        public ArmyManagementVM CurrentArmyManagementVM { get; private set; }
        public ArmyManagementVMMixIn CurrentArmyManagementVMMixIn { get; private set; }

        public static ACArmyManagementUIContext Instance { get; private set; }

        public void UnregisterInstance()
        {
            Instance = null;
        }

        public ACArmyManagementUIContext(ArmyManagementVM currentArmyManagementVM, ArmyManagementVMMixIn currentArmyManagementVMMixIn) 
        {

            Instance = this;
            CurrentArmyManagementVM = currentArmyManagementVM;
            CurrentArmyManagementVMMixIn = currentArmyManagementVMMixIn;
        }


        private MobileParty _currentMainParty;
        internal Settlement gatherSettlement;
        internal bool CanEngageEnemyParties;
        internal bool CanHelpAlliedParties;
        internal bool CanResupply;
        internal bool CanRunFromDanger;

        public MobileParty currentMainParty { 
            get
            {
                return _currentMainParty;
            }
            set {
                if (!object.ReferenceEquals(_currentMainParty, value))
                {
                    _currentMainParty = value;
                }

                if (value != null)
                {
                    CurrentArmyManagementVMMixIn?.UpdateContextOnFirstPartyAdded(value);

                }
                else
                {
                    CurrentArmyManagementVMMixIn?.UpdateContextOnLeaderPartyRemoved();
                }
            } 
        }


        public bool mainPartyHasArmy { get; set; }


        public Settlement targetSettlement { get; set; }

        public Army.ArmyTypes  armyBehavior { get; set; }

        public int influenceSent { get; set; }

        public bool movieIsLoaded { get; set; }
    }
}