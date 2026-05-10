using ArmyCommander.UIExtension.Context;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;


namespace ArmyCommander.UIExtension
{
    public class SelectableArmyItemPropertyVM : ViewModel
    {

        #region Sprite paths
        public static class PropertyTypeSprites
        {
            public const string influence_icon = @"General\Icons\Influence@2x";
            public const string food_icon = @"General\Icons\Food@2x";
            public const string troops_icon = @"General\Icons\Party@2x";
            public const string cohesion_icon = @"General\Icons\Prosperity";
            public const string cohesion_cost_icon = @"General\Icons\Influence@2x";
            public const string parties_icon = @"MapBar\mapbar_icon2";
        }

        #endregion


        #region Inner Attributes

        private string _sprite_path;
        private bool _isWarning;
        private string _name;
        private string _value;
        private BasicTooltipViewModel _hint;
        private string _colonText;

        private Func<ACArmyLineUIContext, bool> _updateIsWarning;
        private Func<ACArmyLineUIContext, string> _updateValue;
        private Func<ACArmyLineUIContext, int> _updateDailyChange;
        private Func<ACArmyLineUIContext, BasicTooltipViewModel> _updateHint;



        //private string _upperText;
        //private string _innerText;
        //private string _lowerText;
        //private bool _isEnabled;
        //private bool _isVisible;
        //private Action _clickFunction;

        #endregion


        #region DataProperty


        [DataSourceProperty]
        public string sprite_path
        {
            get
            {
                return _sprite_path;
            }
            set
            {
                if (value != _sprite_path)
                {
                    _sprite_path = value;
                    OnPropertyChangedWithValue(value, "sprite_path");
                }
            }
        }

        [DataSourceProperty]
        public bool IsWarning
        {
            get
            {
                return _isWarning;
            }
            set
            {
                if (value != _isWarning)
                {
                    _isWarning = value;
                    OnPropertyChangedWithValue(value, "IsWarning");
                }
            }
        }

        

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChangedWithValue(value, "Value");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel Hint
        {
            get
            {
                return _hint;
            }
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    OnPropertyChangedWithValue(value, "Hint");
                }
            }
        }

        [DataSourceProperty]
        public string ColonText
        {
            get
            {
                return _colonText;
            }
            set
            {
                if (value != _colonText)
                {
                    _colonText = value;
                    OnPropertyChangedWithValue(value, "ColonText");
                }
            }
        }


        private int _changeAmount;

        [DataSourceProperty]
        public int ChangeAmount
        {
            get
            {
                return _changeAmount;
            }
            set
            {
                if (value != _changeAmount)
                {
                    _changeAmount = value;
                    OnPropertyChangedWithValue(value, "ChangeAmount");
                }
            }
        }



        #endregion



        #region possible constructors

        public SelectableArmyItemPropertyVM(
            string property_type_sprite, 
            Func<ACArmyLineUIContext, bool> updateIsWarning,
            Func<ACArmyLineUIContext, string> updateValue,
            Func<ACArmyLineUIContext, int> updateDailyChange,
            Func<ACArmyLineUIContext, BasicTooltipViewModel> updateHint
            )
        {
            
            _updateIsWarning = updateIsWarning;
            _updateValue = updateValue;
            _updateDailyChange = updateDailyChange;
            _updateHint = updateHint;

            sprite_path = property_type_sprite;

            RefreshValues();

        }

        #endregion

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void UpdateValues(ACArmyLineUIContext context)
        {
            //LeaderParty = _updateLeaderParty(context);
            Value = _updateValue(context);
            IsWarning = _updateIsWarning(context);
            ChangeAmount = _updateDailyChange(context);
            Hint = _updateHint(context);
            RefreshValues();
        }
    }
}