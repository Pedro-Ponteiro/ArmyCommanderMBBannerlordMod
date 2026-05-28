using ArmyCommander.UIExtension.Context;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace ArmyCommander.UIExtension.MixIns.VMItems
{
    public class SelectableArmyLeaderVisualVM : ViewModel
    {
        private MobileParty _leaderParty;
        private Func<ACArmyLineUIContext, MobileParty> _updateLeaderParty;
        private Func<ACArmyLineUIContext, CharacterImageIdentifierVM> _updateVisual;
        private Func<ACArmyLineUIContext, BannerImageIdentifierVM> _updateBanner;

        private CharacterImageIdentifierVM _visual;
        private BannerImageIdentifierVM _banner_9;

        [DataSourceProperty]
        public CharacterImageIdentifierVM Visual
        {
            get
            {
                return _visual;
            }
            set
            {
                if (value != _visual)
                {
                    _visual = value;
                    OnPropertyChangedWithValue(value, "Visual");
                }
            }
        }

        [DataSourceProperty]
        public BannerImageIdentifierVM Banner_9
        {
            get
            {
                return _banner_9;
            }
            set
            {
                if (value != _banner_9)
                {
                    _banner_9 = value;
                    OnPropertyChangedWithValue(value, "Banner_9");
                }
            }
        }

        public void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(_leaderParty.LeaderHero.EncyclopediaLink);
        }

        public void ExecuteBeginHint()
        {
            InformationManager.ShowTooltip(typeof(Hero), _leaderParty.LeaderHero, true);
        }

        public void ExecuteEndHint()
        {
            MBInformationManager.HideInformations();
        }


        public void ExecuteShowOnMap()
        {
            MapScreen.Instance.FastMoveCameraToPosition(_leaderParty.Position);
        }

        public SelectableArmyLeaderVisualVM(
            Func<ACArmyLineUIContext, MobileParty> updateLeaderParty,
            Func<ACArmyLineUIContext, CharacterImageIdentifierVM> updateVisual,
            Func<ACArmyLineUIContext, BannerImageIdentifierVM> updateBanner
            )
        {
            _updateLeaderParty = updateLeaderParty;
            _updateVisual = updateVisual;
            _updateBanner = updateBanner;

        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void UpdateValues(ACArmyLineUIContext context)
        {

            _leaderParty = _updateLeaderParty(context);
            Visual = _updateVisual(context);
            Banner_9 = _updateBanner(context);
            

            RefreshValues();
        }

    }
}
