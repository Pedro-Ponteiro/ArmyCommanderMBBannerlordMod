using ArmyCommander.HarmonyPatches;
using ArmyCommander.UIExtension.Context;
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
        public Func<ACArmyLineUIContext, MobileParty> _updateLeaderParty;
        private CharacterImageIdentifierVM _LeaderVisual;
        private bool _forceHovered;
        private bool _isSelected;


        // ACESSADO PELO .XML (buttons)
        public void ExecuteClickFunction()
        {
            ACArmyOverlayUIContext.Instance.SelectedArmy = LeaderParty.Army;

            // Atualizar o Overlay de Armies Original
            CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
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

        [DataSourceProperty]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool ForceHovered
        {
            get
            {
                return _forceHovered;
            }
            set
            {
                if (_forceHovered != value)
                {
                    _forceHovered = value;
                    OnPropertyChangedWithValue(value, nameof(ForceHovered));
                }
            }
        }

        public void ExecuteBeginHover()
        {
            ForceHovered = true;
        }

        public void ExecuteEndHover()
        {
            if (IsSelected != true)
            {
                ForceHovered = false;
            }
        }


        public SelectableArmyLineVM(
            Func<ACArmyLineUIContext, MobileParty> updateLeaderParty,
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

            context.registerLineVM(this);

            foreach (var item in ArmyInfoRows)
            {
                item.UpdateValues(context);
            }

            RefreshValues();
        }

        public void UpdateSelection()
        {
            if (LeaderParty == ACArmyOverlayUIContext.Instance.SelectedArmy?.LeaderParty)
            {
                IsSelected = true;
                ExecuteBeginHover();
            }
            else
            {
                IsSelected = false;
                ExecuteEndHover();
            }
        }

    }
}
