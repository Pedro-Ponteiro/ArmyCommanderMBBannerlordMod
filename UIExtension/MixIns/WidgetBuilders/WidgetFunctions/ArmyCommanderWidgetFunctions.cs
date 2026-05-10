using ArmyCommander.Actions;
using ArmyCommander.UIExtension.VMContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ArmyCommander.UIExtension.WidgetFunctions
{
    internal static class ArmyCommanderWidgetFunctions
    {
        //public static void send_item_button_press()
        //{

        //BarterManager.Instance.StartBarterOffer(Hero.MainHero, 
        //    ACArmyOverlayUIContext.SelectedArmyLineContext.LeaderParty.LeaderHero, 
        //    PartyBase.MainParty,
        //    ACArmyOverlayUIContext.SelectedArmyLineContext.LeaderParty.Party);

        //// change this to select which items to send!
        ////Action click_function = () => InformationManager.ShowInquiry(
        ////        new InquiryData("SEND FOOD ACTION",
        ////        $"Sending food will cost you {ACArmyUIContext.Variables.SendFoodInfluenceCost} Influence. Are you sure?",
        ////        isAffirmativeOptionShown: true,
        ////        isNegativeOptionShown: true,
        ////        GameTexts.FindText("str_yes").ToString(),
        ////        GameTexts.FindText("str_no").ToString(),
        ////        send_item_confirm,
        ////        null
        ////        ),
        ////        pauseGameActiveState: true
        ////    );

        //}

        //public static void send_item_confirm()
        //{
        //    //int selected_total_food_to_send = 50;

        //    ACActions.SubtractInfluence(ACArmyOverlayUIContext.SelectedArmyLineContext.SendItemInfluenceCost, Clan.PlayerClan);

        //    // TODO: ATUALIZAR UI E COLOCAR LOG.

        //    //int food_for_each_party = total_food_to_send / ACArmyUIContext.AttachedPartiesCount;


        //    //ACActions.SendItemQuantityOneToOne(MobileParty.MainParty, 
        //    //    ACArmyUIContext.Variables.ArmyAttachedParties, 
        //    //    ACArmyUIContext.Variables.SendFoodInfluenceCost, 
        //    //    selected_total_food_to_send,
        //    //    ACArmyUIContext.Variables.PlayerFoodInventory);
        //}

        //public static void manage_army_button_press()
        //{

        //}

        //public static void disband_army_button_press()
        //{
        //    Action click_function = () => InformationManager.ShowInquiry(
        //        new InquiryData("Disband Army",
        //        "Are you sure you want to disband this army? This will result in relation loss.",
        //        isAffirmativeOptionShown: true,
        //        isNegativeOptionShown: true,
        //        GameTexts.FindText("str_yes").ToString(),
        //        GameTexts.FindText("str_no").ToString(),
        //        disband_army_confirm,
        //        null
        //        ),
        //        pauseGameActiveState: true
        //    );
        //}

        public static void disband_army_confirm()
        {
            //DisbandArmyAction.ApplyByReleasedByPlayerAfterBattle(ACArmyOverlayUIContext.SelectedArmyLineContext.LeaderParty.Army);
            ACArmyOverlayUIContext.CurrentArmyOverlayVMMixIn.RenewArmyList();
        }


    }
}
