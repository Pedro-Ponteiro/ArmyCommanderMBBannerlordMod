using ArmyCommander.Patches;
using ArmyCommander.UIExtension.Context;
using ArmyCommander.UIExtension.VMContext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace ArmyCommander.UIExtension.MixIns.VMItems
{
    public class SelectableArmyLineVM : ViewModel
    {
        private MobileParty _LeaderParty;
        private MBBindingList<SelectableArmyPropertiesRow> _ArmyInfoRows;
        private Func<ACArmyLineUIContext, MobileParty> _updateLeaderParty;
        private CharacterImageIdentifierVM _LeaderVisual;


        // ACESSADO PELO .XML (buttons)
        private void ExecuteClickFunction()
        {
            ClickFunction.Invoke();
        }

        private Action ClickFunction
        {
            get
            {
                Action click = () =>
                {

                    if (LeaderParty != null)
                    {
                        // Atualizar o Overlay de Armies Original
                        ACArmyOverlayUIContext.SelectedArmy = LeaderParty.Army;
                        CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
                    }
                };

                return click;
            }
        }

        [DataSourceProperty]
        public MobileParty LeaderParty
        {
            get
            {
                return _LeaderParty;
            }
            set
            {
                if (value != _LeaderParty)
                {
                    _LeaderParty = value;
                    OnPropertyChangedWithValue(this, "LeaderParty");
                }
            }
        }

        [DataSourceProperty]
        public CharacterImageIdentifierVM LeaderVisual
        {
            get
            {
                return _LeaderVisual;
            }
            set
            {
                if (value != _LeaderVisual)
                {
                    _LeaderVisual = value;
                    OnPropertyChangedWithValue(this, "LeaderVisual");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<SelectableArmyPropertiesRow> ArmyInfoRows
        {
            get
            {
                return _ArmyInfoRows;
            }
            set
            {
                if (value != _ArmyInfoRows)
                {
                    _ArmyInfoRows = value;
                    OnPropertyChangedWithValue(value, "ArmyInfoRows");
                }
            }
        }

        

        public SelectableArmyLineVM(
            Func<ACArmyLineUIContext ,MobileParty> updateLeaderParty,
            MBBindingList<SelectableArmyPropertiesRow> armyInfoRows
            )
        {
            _updateLeaderParty = updateLeaderParty;
            ArmyInfoRows = armyInfoRows;

            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void UpdateValues(ACArmyLineUIContext context)
        {

            MobileParty new_leader = _updateLeaderParty(context);

            if (new_leader != LeaderParty)
            {
                LeaderParty = new_leader;
                CharacterCode characterCode = CampaignUIHelper.GetCharacterCode(LeaderParty.LeaderHero.CharacterObject);
                LeaderVisual = new CharacterImageIdentifierVM(characterCode);
            }

            foreach (var item in ArmyInfoRows)
            {
                item.UpdateValues(context);
            }

            RefreshValues();
        } 

    }
}
