using ArmyCommander.UIExtension.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace ArmyCommander.UIExtension.MixIns.VMItems
{
    public class ACArmyOverlayArmyListVM : ViewModel
    {
        private MBBindingList<SelectableArmyLineVM> _ArmiesList;

        [DataSourceProperty]
        public MBBindingList<SelectableArmyLineVM> ArmiesList
        {
            get
            {
                return _ArmiesList;
            }
            set
            {
                if (value != _ArmiesList)
                {
                    _ArmiesList = value;
                    OnPropertyChangedWithValue(value, "ArmiesList");
                }
            }
        }



        public ACArmyOverlayArmyListVM(
            MBBindingList<SelectableArmyLineVM> armiesList
            )
        {
            ArmiesList = armiesList;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void UpdateValues()
        {
            foreach (var line in _ArmiesList)
            {
                ACArmyLineUIContext ui_context = new ACArmyLineUIContext();
                ui_context = ACArmyOverlayUIContext.Instance.CurrentArmyOverlayVMMixIn.UpdateLineContext(ui_context, line.LeaderParty.Army);
                line.UpdateValues(ui_context);
            }

            RefreshValues();
        }

        public void UpdateSelection()
        {
            foreach (var line in _ArmiesList)
            {
                line?.UpdateSelection();
            }

            RefreshValues();
        }

        public void AddLine(SelectableArmyLineVM new_line)
        {
            ArmiesList.Add(new_line);
            RefreshValues();
        }

        public void ClearLines()
        {
            ArmiesList.Clear();
            RefreshValues();
        }

        public int ArmiesCount()
        {
            return ArmiesList.Count;
        }

    }
}

