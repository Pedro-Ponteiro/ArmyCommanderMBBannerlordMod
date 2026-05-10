using ArmyCommander.UIExtension.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace ArmyCommander.UIExtension.MixIns.VMItems
{
    public class SelectableArmyPropertiesRow : ViewModel
    {


        private MBBindingList<SelectableArmyItemPropertyVM> _ArmyInfosRow;

        [DataSourceProperty]
        public MBBindingList<SelectableArmyItemPropertyVM> ArmyInfosRow
        {
            get
            {
                return _ArmyInfosRow;
            }
            set
            {
                if (value != _ArmyInfosRow)
                {
                    _ArmyInfosRow = value;
                    OnPropertyChangedWithValue(value, "ArmyInfosRow");
                }
            }
        }



        public SelectableArmyPropertiesRow(
            MBBindingList<SelectableArmyItemPropertyVM> armyInfosRow
            )
        {
            ArmyInfosRow = armyInfosRow;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void UpdateValues(ACArmyLineUIContext context)
        {
            foreach (var item in _ArmyInfosRow)
            {
                item.UpdateValues(context);
            }

            RefreshValues();
        }

    }
}
