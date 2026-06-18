using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace ArmyCommander.Actions
{
    public static class ACActions
    {
        public static void SendItem(MobileParty from, MobileParty to, ItemRosterElement item, int quantity)
        {

            // BREAKPOINT HERE??
            ItemBarterable it_bar = new ItemBarterable(from.LeaderHero, to.LeaderHero, from.Party, to.Party, item, item.EquipmentElement.ItemValue);
            it_bar.CurrentAmount = quantity;
            it_bar.Apply();
        }

        public static void SendItemQuantityOneToOne(
            MobileParty from, 
            MobileParty to,  
            int influence_cost, 
            int quantity, 
            List<ItemRosterElement> sender_items_to_send, 
            bool prioritize_cheapest = true
        )
        {
            // Gets items from the sender and sends them to the receiver.
            // Please check whether quantity <= sender_items_to_send.amount.
            // TODO: YOU MUST DEFINE THE TYPE OF THE ITEM THAT WILL BE SENT. DEFINE PARAMETER

            int selected_items_count = 0;


            IEnumerable<ItemRosterElement> ordered_items = sender_items_to_send.OrderBy((item) => item.EquipmentElement.ItemValue);

            if (!prioritize_cheapest)
            {
                ordered_items = ordered_items.Reverse();
            }

            foreach (ItemRosterElement item_roster_element in ordered_items)
            {

                int amount_to_give = Math.Min(quantity, item_roster_element.Amount);
                selected_items_count += amount_to_give;
                SendItem(from, to, item_roster_element, quantity);

                if (selected_items_count == quantity) break;
            }

            SubtractInfluence(influence_cost, from.ActualClan);
        }

        //public static void SendItemQuantityOneToMany(
        //    MobileParty from,
        //    List<MobileParty> to,
        //    int influence_cost,
        //    int quantity,
        //    List<ItemRosterElement> sender_items_to_send,
        //    bool prioritize_poorest = true,
        //    bool prioritize_cheapest_item = true,
        //    Func<MobileParty, int> poorness_calculator = null
        //)
        //{
        //    // Please check whether quantity <= sender_items_to_send.amount.
        //    // TODO: YOU MUST DEFINE THE TYPE OF THE ITEM THAT WILL BE SENT. DEFINE PARAMETER (MOUNT? ARMOR? FOOD?)...

        //    if (prioritize_cheapest_item) 
        //    IEnumerable<ItemRosterElement> ordered_items = sender_items_to_send.OrderBy((item) => item.EquipmentElement.ItemValue);
        //    if (!prioritize_cheapest_item)
        //    {
        //        ordered_items = ordered_items.Reverse();
        //    }


        //    if (prioritize_poorest)
        //    {
        //        // Oh my god, how do you define who is the poorest? Quantity of items
        //        ACCalculationModel.DistributeToSmallestKeepOriginalOrder()
        //    }




        //}

        public static void SubtractInfluence(int amount, Clan clan)
        {
            clan.Influence -= amount;
        }

        public static void AddInfluence(int amount, Clan clan)
        {
            clan.Influence += amount;
        }

        public static void TransferInfluence(Clan sender_clan, Clan receiver_clan, int amount)
        {
            sender_clan.Influence -= amount;
            receiver_clan.Influence += amount;
        }

    }
}
