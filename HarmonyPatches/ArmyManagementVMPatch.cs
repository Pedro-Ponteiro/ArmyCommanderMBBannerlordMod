using ArmyCommander.Store;
using ArmyCommander.Helpers;
using ArmyCommander.UIExtension;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.MountAndBlade.FormationAI;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.LinQuick;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch]
    internal static class ArmyManagementVMPatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyManagementVM), "OnRefresh")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OriginalOnRefresh(ArmyManagementVM instance)
        {
            throw new NotImplementedException("Stub de ReversePatch (ArmyManagementVM.OnRefresh)");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyManagementVM), "OnAddToCart", new Type[] { typeof(ArmyManagementItemVM) })]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OriginalOnAddToCart(ArmyManagementVM instance, ArmyManagementItemVM armyItem)
        {
            throw new NotImplementedException("Stub de ReversePatch (ArmyManagementVM.OnAddToCart)");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyManagementVM), "OnRemove", new Type[] { typeof(ArmyManagementItemVM) })]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OriginalOnRemove(ArmyManagementVM instance, ArmyManagementItemVM armyItem)
        {
            throw new NotImplementedException("Stub de ReversePatch (ArmyManagementVM.OnRemove)");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyManagementVM), "OnFocus", new Type[] { typeof(ArmyManagementItemVM) })]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OriginalOnFocus(ArmyManagementVM instance, ArmyManagementItemVM armyItem)
        {
            throw new NotImplementedException("Stub de ReversePatch (ArmyManagementVM.OriginalOnFocus)");
        }


        [HarmonyPatch(typeof(ArmyManagementVM.ManagementItemComparer), "Compare")]
        [HarmonyPrefix]
        private static bool ManagementItemComparerComparePrefix(
            ArmyManagementItemVM x,
            ArmyManagementItemVM y,
            ref int __result
        )
        {
            if (x.Party == ACArmyManagementUIContext.Instance?.currentMainParty)
            {
                __result = -1;
                return false;
            }

            __result = y.IsAlreadyWithPlayer.CompareTo(x.IsAlreadyWithPlayer);
            return false;
        }

        [HarmonyPatch(typeof(ArmyManagementVM), "set_PlayerHasArmy")]
        [HarmonyPrefix]
        private static bool PlayerHasArmySetterPrefix(ArmyManagementVM __instance, bool value)
        {

            if (ACArmyManagementUIContext.Instance?.currentMainParty?.IsMainParty == true && ACArmyManagementUIContext.Instance?.mainPartyHasArmy == true)
            {
                value = true;
            }
            else
            {
                value = false;
            }


            if (value != PlayerHasArmyRef(__instance))
            {
                PlayerHasArmyRef(__instance) = value;
                __instance.OnPropertyChangedWithValue(value, "PlayerHasArmy");
            }

            return false;
        }



        [HarmonyPatch(typeof(ArmyManagementVM), MethodType.Constructor, new Type[] { typeof(Action) })]
        [HarmonyPostfix]
        private static void ConstructorPostfix(ArmyManagementVM __instance, Action onClose)
        {
            __instance.PartyList = new MBBindingList<ArmyManagementItemVM>();
            __instance.PartiesInCart = new MBBindingList<ArmyManagementItemVM>();
            PartiesToRemoveRef(__instance) = new MBBindingList<ArmyManagementItemVM>();
            CurrentPartiesRef(__instance) = new List<MobileParty>();
            __instance.CohesionHint = new BasicTooltipViewModel();
            __instance.FoodHint = new HintViewModel();
            __instance.MoraleHint = new HintViewModel();
            __instance.BoostCohesionHint = new HintViewModel();
            __instance.DisbandArmyHint = new HintViewModel();
            __instance.DoneHint = new HintViewModel();
            __instance.TutorialNotification = new ElementNotificationVM();
            __instance.CanAffordInfluenceCost = true;
            Action<ArmyManagementItemVM> onAddToCart = CreateArmyManagementItemCallback(__instance, OnAddToCartMethod);
            Action<ArmyManagementItemVM> onRemove = CreateArmyManagementItemCallback(__instance, OnRemoveMethod);
            Action<ArmyManagementItemVM> onFocus = CreateArmyManagementItemCallback(__instance, OnFocusMethod);




            // If there is a selectedArmy, use it to populate this initial state.
            // Otherwise, leave all available parties on the left and
            // keep the right side empty.


            bool canPlayerCommandArmies = ACHelpers.HasPlayerPermissionForArmyCommand();

            // This updates the context when setting the values below
            if (!canPlayerCommandArmies)
            {
                ACArmyManagementUIContext.Instance.currentMainParty = Hero.MainHero.PartyBelongedTo;
            }
            else if (ACArmyOverlayUIContext.Instance?.SelectedArmy == null)
            {
                ACArmyManagementUIContext.Instance.currentMainParty = Hero.MainHero.PartyBelongedTo;
            }
            else
            {
                ACArmyManagementUIContext.Instance.currentMainParty = ACArmyOverlayUIContext.Instance.SelectedArmy.LeaderParty;
            }


            // This enables buttons like boost cohesion, etc.
            __instance.PlayerHasArmy = ACArmyManagementUIContext.Instance.currentMainParty.IsMainParty && ACArmyManagementUIContext.Instance.mainPartyHasArmy;



            // When selecting the first party from the left, set that party as the main party.

            // When removing the last party from the right, disable "Done" and say there are no parties.

            // OnReset will work from the context's currentlySelectedArmy, which is set when OnAdd runs on a party that is an army leader.


            MainPartyItemRef(__instance) = new ArmyManagementItemVM(onAddToCart, onRemove, onFocus, ACArmyManagementUIContext.Instance.currentMainParty)
            {
                IsAlreadyWithPlayer = true,
                IsMainHero = !canPlayerCommandArmies,
                IsInCart = true,
                IsTransferDisabled = PlayerSiege.PlayerSiegeEvent != null || !canPlayerCommandArmies
            };

            if (ACArmyManagementUIContext.Instance.mainPartyHasArmy)
            {
                MainPartyItemRef(__instance).Cost = 0;
            }
            else
            {
                MainPartyItemRef(__instance).Cost = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(Hero.MainHero.PartyBelongedTo, MainPartyItemRef(__instance).Party);
            }

            __instance.PartiesInCart.Add(MainPartyItemRef(__instance));



            foreach (MobileParty item in MobileParty.All)
            {
                if (item.LeaderHero != null && item.MapFaction == Hero.MainHero.MapFaction && item.LeaderHero != ACArmyManagementUIContext.Instance.currentMainParty.LeaderHero && !item.IsCaravan)
                {
                    __instance.PartyList.Add(new ArmyManagementItemVM(onAddToCart, onRemove, onFocus, item)
                    {
                        IsMainHero = false,
                        IsTransferDisabled = PlayerSiege.PlayerSiegeEvent != null
                    });
                }
            }


            if (ACArmyManagementUIContext.Instance.mainPartyHasArmy)
            {
                foreach (ArmyManagementItemVM party in __instance.PartyList)
                {
                    if (party.Party.Army == ACArmyManagementUIContext.Instance.currentMainParty.Army)
                    {
                        party.Cost = 0;
                        party.IsAlreadyWithPlayer = true;
                        party.IsInCart = true;
                        __instance.PartiesInCart.Add(party);
                    }
                    else
                    {
                        party.Cost = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(ACArmyManagementUIContext.Instance.currentMainParty, party.Party);
                    }
                }

            }


            if (canPlayerCommandArmies)
            {
                // Make it available on the left side.
                __instance.PartyList.Add(MainPartyItemRef(__instance));
            }



            if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army == ACArmyManagementUIContext.Instance.currentMainParty.Army)
            {
                __instance.CohesionBoostCost = Campaign.Current.Models.ArmyManagementCalculationModel.GetCohesionBoostInfluenceCost(MobileParty.MainParty.Army, 10);
            }

            InitialInfluenceRef(__instance) = Hero.MainHero.Clan.Influence;

            __instance.SortControllerVM = new ArmyManagementSortControllerVM(PartyListRef(__instance));

            OrderPartiesInPlace(__instance.PartiesInCart);
            OrderPartiesInPlace(__instance.PartyList);

            OriginalOnRefresh(__instance);
            __instance.RefreshValues();

        }


        [HarmonyPatch(typeof(ArmyManagementVM), "RefreshValues")]
        [HarmonyPostfix]
        private static void RefreshValuesPostfix(ArmyManagementVM __instance)
        {
            __instance.TitleText = ACArmyManagementUIContext.Instance?.mainPartyHasArmy == true ? __instance.TitleText : "Army Creation";
        }


        [HarmonyPatch(typeof(ArmyManagementVM), "GetCanDisbandArmyWithReason")]
        [HarmonyPrefix]
        private static bool GetCanDisbandArmyWithReasonPrefix(
            ArmyManagementVM __instance,
            ref bool __result,
            ref TextObject disabledReason)
        {
            if (ACArmyManagementUIContext.Instance?.mainPartyHasArmy != true)
            {
                disabledReason = new TextObject("{=iSZTOeYH}No army to disband.");
                __result = false;
                return false;
            }

            if (ACHelpers.IsPartyBusy(ACArmyManagementUIContext.Instance.currentMainParty) || ACHelpers.IsPlayerBusy())
            {
                disabledReason = new TextObject("{=uipNpzVw}Cannot disband the army right now.");
                __result = false;
                return false;
            }


            disabledReason = TextObject.GetEmpty();
            __result = true;
            return false;
        }


        private static void OrderPartiesInPlace(MBBindingList<ArmyManagementItemVM> party_list)
        {
            var orderedParties = party_list
                .OrderBy(item => GetPartySortPriority(item))
                .ThenByDescending(item => item.Party.Party.EstimatedStrength)
                .ToList();

            party_list.Clear();

            foreach (var item in orderedParties)
            {
                party_list.Add(item);
            }
        }

        private static int GetPartySortPriority(ArmyManagementItemVM item)
        {
            var party = item.Party;

            // 1st MainHero / MainParty


            if (item.IsInCart)
            {
                if (party == ACArmyManagementUIContext.Instance.currentMainParty)
                {
                    return 0;
                }
                return 1;
            }

            if (party == MobileParty.MainParty)
            {
                return 2;
            }

            // 2nd Army Leaders
            if (party.Army != null && party.Army.LeaderParty == party)
            {
                return 3;
            }

            // 3rd Parties that do not have an army
            if (item.IsEligible)
            {
                return 4;
            }

            // 4th Parties that have an army, but are not army leaders
            return 5;
        }

        private static void ResetSortController(ArmyManagementVM __instance)
        {
            __instance.SortControllerVM.DistanceState = 0;
            __instance.SortControllerVM.CostState = 0;
            __instance.SortControllerVM.StrengthState = 0;
            __instance.SortControllerVM.NameState = 0;
            __instance.SortControllerVM.ClanState = 0;
            __instance.SortControllerVM.ShipCountState = 0;
            __instance.SortControllerVM.IsDistanceSelected = false;
            __instance.SortControllerVM.IsCostSelected = false;
            __instance.SortControllerVM.IsNameSelected = false;
            __instance.SortControllerVM.IsClanSelected = false;
            __instance.SortControllerVM.IsStrengthSelected = false;
            __instance.SortControllerVM.IsShipCountSelected = false;
        }

        private static void OnFirstPartyAdded(ArmyManagementVM __instance, ArmyManagementItemVM armyItem)
        {


            MobileParty partySelected = armyItem.Party;
            Army armySelected = partySelected.Army;

            // This updates the context
            ACArmyManagementUIContext.Instance.currentMainParty = armyItem.Party;


            __instance.PlayerHasArmy = ACArmyManagementUIContext.Instance.mainPartyHasArmy && ACArmyManagementUIContext.Instance.currentMainParty?.IsMainParty == true;


            if (ACArmyManagementUIContext.Instance.mainPartyHasArmy == true)
            {

                foreach (var item in __instance.PartyList)
                {

                    if (item.Party.Army == armySelected)
                    {
                        item.Cost = 0;
                        item.IsAlreadyWithPlayer = true;
                        item.IsInCart = true;
                        item.CanJoinBackWithoutCost = false;

                        __instance.PartiesInCart.Add(item);
                        //OriginalOnRefresh(__instance);
                    }
                    else
                    {
                        item.Cost = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(partySelected, item.Party);
                    }
                    item.UpdateEligibility();
                }
            }
            else
            {

                armyItem.IsAlreadyWithPlayer = true;
                armyItem.IsInCart = true;
                armyItem.CanJoinBackWithoutCost = false;
                __instance.PartiesInCart.Add(armyItem);
                __instance.TotalCost += armyItem.Cost;
                armyItem.UpdateEligibility();

                foreach (var item in __instance.PartyList)
                {
                    item.Cost = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(partySelected, item.Party);
                    item.UpdateEligibility();
                }
            }


            ResetSortController(__instance);
            OrderPartiesInPlace(__instance.PartiesInCart);
            OrderPartiesInPlace(__instance.PartyList);
            __instance.RefreshValues();
        }

        private static void OnArmyLeaderRemoved(ArmyManagementVM __instance)
        {
            // This updates the context
            ACArmyManagementUIContext.Instance.currentMainParty = null;

            __instance.PlayerHasArmy = false;

            MBBindingList<ArmyManagementItemVM> partiesToRemove = PartiesToRemoveRef(__instance);

            partiesToRemove.Clear();

            foreach (var item in __instance.PartyList)
            {
                if (item.IsAlreadyWithPlayer || item.IsInCart)
                {
                    item.IsAlreadyWithPlayer = false;
                    item.CanJoinBackWithoutCost = false;

                    if (item.IsInCart)
                    {
                        item.IsInCart = false;
                        __instance.PartiesInCart.Remove(item);
                        //OriginalOnRefresh(__instance);
                    }

                }

                if (item.Party.Army?.LeaderParty == item.Party)
                {
                    item.Cost = 0;
                }
                else
                {
                    // INFLUENCE COST FOR SETTING UP AN ARMY LEADER
                    item.Cost = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(Hero.MainHero.PartyBelongedTo, item.Party) * 3;
                }
                item.UpdateEligibility();
            }

            __instance.TotalCost = InfluenceSpentForCohesionBoostingRef(__instance);
            ResetSortController(__instance);

            OrderPartiesInPlace(__instance.PartyList);
            __instance.RefreshValues();
        }

        private static void CustomOnAddToCart(ArmyManagementVM __instance, ArmyManagementItemVM armyItem)
        {
            MBBindingList<ArmyManagementItemVM> partiesToRemove = PartiesToRemoveRef(__instance);

            if (!__instance.PartiesInCart.Contains(armyItem))
            {

                // If this is the first party added, it becomes the main party.
                // partiesToRemove is cleared (IN ONREMOVE).
                // The properties on the left side need to be updated.
                // mainPartyHasArmy = true if the leader already has an army.
                // The right side is populated based on IsAlreadyWithPlayer.


                if (__instance.PartiesInCart.Count == 0)
                {
                    OnFirstPartyAdded(__instance, armyItem);
                }
                else
                {
                    __instance.PartiesInCart.Add(armyItem);
                    armyItem.IsInCart = true;
                    Game.Current.EventManager.TriggerEvent(new PartyAddedToArmyByPlayerEvent(armyItem.Party));

                    if (partiesToRemove.Contains(armyItem))
                    {
                        partiesToRemove.Remove(armyItem);
                    }

                    if (armyItem.IsAlreadyWithPlayer)
                    {
                        armyItem.CanJoinBackWithoutCost = false;
                    }

                    __instance.TotalCost += armyItem.Cost;

                }
            }

            OriginalOnRefresh(__instance);
        }



        [HarmonyPatch(typeof(ArmyManagementVM), "OnAddToCart", new Type[] { typeof(ArmyManagementItemVM) })]
        [HarmonyPrefix]
        private static bool OnAddToCartPrefix(ArmyManagementVM __instance, ArmyManagementItemVM armyItem)
        {

            CustomOnAddToCart(__instance, armyItem);

            return false;
        }


        private static void CustomOnRemove(ArmyManagementVM __instance, ArmyManagementItemVM armyItem)
        {
            MBBindingList<ArmyManagementItemVM> partiesToRemove = PartiesToRemoveRef(__instance);

            if (__instance.PartiesInCart.Contains(armyItem))
            {
                // If the removed party was the army leader, clear the right side.
                // Clear partiesToRemove.
                // Update the properties on the left side.
                // mainPartyHasArmy = false

                if (armyItem.Party == ACArmyManagementUIContext.Instance.currentMainParty)
                {
                    OnArmyLeaderRemoved(__instance);
                }
                else
                {
                    __instance.PartiesInCart.Remove(armyItem);
                    armyItem.IsInCart = false;
                    partiesToRemove.Add(armyItem);

                    if (armyItem.IsAlreadyWithPlayer)
                    {
                        armyItem.CanJoinBackWithoutCost = true;
                    }

                    __instance.TotalCost -= armyItem.Cost;
                }

            }
            OriginalOnRefresh(__instance);
        }

        [HarmonyPatch(typeof(ArmyManagementVM), "OnRemove", new Type[] { typeof(ArmyManagementItemVM) })]
        [HarmonyPrefix]
        private static bool OnRemovePrefix(ArmyManagementVM __instance, ArmyManagementItemVM armyItem)
        {
            CustomOnRemove(__instance, armyItem);
            return false;
        }

        [HarmonyPatch(typeof(ArmyManagementVM), "ExecuteDone")]
        [HarmonyPrefix]
        private static bool ExecuteDonePrefix(ArmyManagementVM __instance)
        {
            if (!__instance.CanAffordInfluenceCost)
            {
                return false;
            }

            if (__instance.NewCohesion > __instance.Cohesion)
            {
                ApplyCohesionChangeRef(__instance);
            }

            Army armyToUse = ACArmyManagementUIContext.Instance.currentMainParty?.Army;
            bool armyCreated = false;

            if (__instance.PartiesInCart.Count > 1 && MobileParty.MainParty.MapFaction.IsKingdomFaction)
            {
                IEnumerable<MobileParty> imbs = __instance.PartiesInCart.Select((item) => { return item.Party; }).Where((mb) => mb != ACArmyManagementUIContext.Instance.currentMainParty);
                MBReadOnlyList<MobileParty> mbs = new MBReadOnlyList<MobileParty>(imbs);
                if (armyToUse == null)
                {
                    // MBS IS NOT USED IF THIS IS A PLAYER-LED ARMY!
                    ((Kingdom)MobileParty.MainParty.MapFaction).CreateArmy(
                        ACArmyManagementUIContext.Instance.currentMainParty.LeaderHero,
                        ACArmyManagementUIContext.Instance.targetSettlement,
                        ACArmyManagementUIContext.Instance.armyBehavior,
                        mbs
                        );

                    if (!ACArmyManagementUIContext.Instance.currentMainParty.IsMainParty)
                    {
                        ArmyCommandsBehaviorStore.army_commands[ACArmyManagementUIContext.Instance.currentMainParty.Army] = 
                            (ACArmyManagementUIContext.Instance.armyBehavior,
                            ACArmyManagementUIContext.Instance.targetSettlement,
                            ACArmyManagementUIContext.Instance.gatherSettlement,
                            ACArmyManagementUIContext.Instance.CanEngageEnemyParties,
                            ACArmyManagementUIContext.Instance.CanHelpAlliedParties,
                            ACArmyManagementUIContext.Instance.CanResupply,
                            ACArmyManagementUIContext.Instance.CanRunFromDanger);
                    }

                    armyCreated = true;
                    armyToUse = ACArmyManagementUIContext.Instance.currentMainParty.Army;

                }

                if (!armyCreated)
                {
                    foreach (var mb in mbs)
                    {
                        mb.Army = ACArmyManagementUIContext.Instance.currentMainParty.Army;
                    }

                    (
                        Army.ArmyTypes ArmyType,
                        Settlement TargetSettlement,
                        Settlement GatherSettlement,
                        bool CanEngageEnemyParties,
                        bool CanHelpAlliedParties,
                        bool CanResupply,
                        bool CanRunFromDanger
                    ) new_commands = (
                        ACArmyManagementUIContext.Instance.armyBehavior,
                        ACArmyManagementUIContext.Instance.targetSettlement,
                        ACArmyManagementUIContext.Instance.gatherSettlement,
                        ACArmyManagementUIContext.Instance.CanEngageEnemyParties,
                        ACArmyManagementUIContext.Instance.CanHelpAlliedParties,
                        ACArmyManagementUIContext.Instance.CanResupply,
                        ACArmyManagementUIContext.Instance.CanRunFromDanger
                    );

                    var old_commands_exist = ArmyCommandsBehaviorStore.army_commands.TryGetValue(armyToUse, out var old_player_commands);

                    // TODO: CHECK THIS WITH AN AI CREATED ARMY!
                    var default_ai_commands = ACAIBehaviorHelpers.GetDefaultAiCommands(armyToUse);

                    if (!armyToUse.LeaderParty.IsMainParty && 
                        (
                            (!old_commands_exist && new_commands != default_ai_commands) || // for new player commands
                            (old_commands_exist && new_commands != old_player_commands) // for changing the old player command
                        )
                    )
                    {
                        ArmyCommandsBehaviorStore.army_commands[armyToUse] = new_commands;

                        if (ACHelpers.IsArmyAvailableForOrders(armyToUse))
                        {
                            ACAIBehaviorHelpers.OnPlayerArmyCommandChanged(armyToUse.LeaderParty);
                        }
                    }
                }
                else if (armyCreated && ACArmyManagementUIContext.Instance.currentMainParty.IsMainParty)
                {
                    // MANUAL ASSIGNMENT FOR PLAYER-LED ARMY!
                    foreach (var mb in mbs)
                    {
                        mb.Army = armyToUse;
                    }
                }
                else if (armyCreated)
                {
                    // New AI army created
                    ACAIBehaviorHelpers.OnPlayerArmyCommandChanged(armyToUse.LeaderParty);
                }


                if (ACArmyOverlayUIContext.Instance != null)
                {
                    ACArmyOverlayUIContext.Instance.SelectedArmy = armyToUse;
                }
            }

            int influenceSpentForCohesionBoosting = InfluenceSpentForCohesionBoostingRef(__instance);
            ChangeClanInfluenceAction.Apply(
                Clan.PlayerClan,
                -(__instance.TotalCost - influenceSpentForCohesionBoosting));


            MBBindingList<ArmyManagementItemVM> partiesToRemove = PartiesToRemoveRef(__instance);


            if (__instance.PartiesInCart.Count == 1 && partiesToRemove.Count > 0)
            {
                CustomDisbandArmy(__instance);
                OnCloseRef(__instance)?.Invoke();

                if (partiesToRemove.ContainsQ((item) => item.Party.IsMainParty))
                {
                    GameMenu.ExitToLast();
                }

                MapScreen_OnRefreshState_Patch.RefreshArmyOverlay();
                return false;
            }

            bool player_removed = false;
            if (!armyCreated && partiesToRemove.Count > 0)
            {
                foreach (ArmyManagementItemVM item in partiesToRemove)
                {
                    if (armyToUse.Parties.Contains(item.Party))
                    {
                        item.Party.Army = null;
                        if (item.Party.IsMainParty)
                        {
                            player_removed = true;
                        }
                    }
                }
                partiesToRemove.Clear();
            }

            OnCloseRef(__instance)?.Invoke();

            if (player_removed)
            {
                GameMenu.ExitToLast();
            }

            MapScreen_OnRefreshState_Patch.RefreshArmyOverlay();

            return false;
        }



        public static void CustomDisbandArmy(ArmyManagementVM __instance)
        {
            if (ACArmyManagementUIContext.Instance.currentMainParty.IsMainParty)
            {
                ACArmyManagementUIContext.Instance.currentMainParty.Army = null;
            }
            else
            {
                DisbandArmyAction.ApplyByReleasedByPlayerAfterBattle(ACArmyManagementUIContext.Instance.currentMainParty.Army);
            }

            PartiesToRemoveRef(__instance).Clear();

            __instance.PlayerHasArmy = false;

            OnCloseRef(__instance)?.Invoke();
            CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
        }

        [HarmonyPatch(typeof(ArmyManagementVM), "DisbandArmy")]
        [HarmonyPrefix]
        private static bool DisbandArmyPrefix(ArmyManagementVM __instance)
        {

            CustomDisbandArmy(__instance);

            return false;
        }


        [HarmonyPatch(typeof(ArmyManagementVM), "ExecuteReset")]
        [HarmonyPrefix]
        private static bool ExecuteResetPrefix(ArmyManagementVM __instance)
        {

            if (ACArmyManagementUIContext.Instance.currentMainParty != null)
            {
                CustomOnRemove(__instance, __instance.PartiesInCart.First((item) => item.Party == ACArmyManagementUIContext.Instance.currentMainParty));
            }


            CustomOnAddToCart(__instance, MainPartyItemRef(__instance));



            __instance.NewCohesion = __instance.Cohesion;
            ChangeClanInfluenceAction.Apply(Clan.PlayerClan, InitialInfluenceRef(__instance) - (Clan.PlayerClan.Influence + ACArmyManagementUIContext.Instance.influenceSent));
            __instance.TotalCost = 0;
            BoostedCohesionRef(__instance) = 0;
            InfluenceSpentForCohesionBoostingRef(__instance) = 0;
            PartiesToRemoveRef(__instance).Clear();
            OriginalOnRefresh(__instance);

            return false;
        }

        [HarmonyPatch(typeof(ArmyManagementVM), "ExecuteCancel")]
        [HarmonyPrefix]
        private static bool ExecuteCancelPrefix(ArmyManagementVM __instance)
        {

            ChangeClanInfluenceAction.Apply(Clan.PlayerClan, InitialInfluenceRef(__instance) - (Clan.PlayerClan.Influence + ACArmyManagementUIContext.Instance.influenceSent));
            OnCloseRef(__instance)?.Invoke();


            return false;
        }

        [HarmonyPatch(typeof(ArmyManagementVM), "OnFinalize")]
        [HarmonyPostfix]
        private static void OnFinalizePostfix(ArmyManagementVM __instance)
        {
            ACArmyManagementUIContext.Instance?.CurrentArmyManagementVMMixIn.OnFinalize();
            ACArmyManagementUIContext.Instance?.UnregisterInstance();
        }



        private static Action<ArmyManagementItemVM> CreateArmyManagementItemCallback(
            ArmyManagementVM instance,
            MethodInfo method)
        {
            return (Action<ArmyManagementItemVM>)Delegate.CreateDelegate(
                typeof(Action<ArmyManagementItemVM>),
                instance,
                method);
        }

        private static readonly Type ManagementItemComparerType =
            AccessTools.Inner(typeof(ArmyManagementVM), "ManagementItemComparer")
            ?? AccessTools.TypeByName("TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement.ManagementItemComparer");

        private static readonly FieldInfo ItemComparerField =
            AccessTools.Field(typeof(ArmyManagementVM), "_itemComparer");

        private static readonly MethodInfo OnAddToCartMethod =
            AccessTools.Method(typeof(ArmyManagementVM), "OnAddToCart", new Type[] { typeof(ArmyManagementItemVM) });

        private static readonly MethodInfo OnRemoveMethod =
            AccessTools.Method(typeof(ArmyManagementVM), "OnRemove", new Type[] { typeof(ArmyManagementItemVM) });

        private static readonly MethodInfo OnFocusMethod =
            AccessTools.Method(typeof(ArmyManagementVM), "OnFocus", new Type[] { typeof(ArmyManagementItemVM) });

        private static readonly MethodInfo OnTutorialNotificationElementIDChangeMethod =
            AccessTools.Method(typeof(ArmyManagementVM), "OnTutorialNotificationElementIDChange");

        private static readonly Action<ArmyManagementVM> ApplyCohesionChangeRef =
            AccessTools.MethodDelegate<Action<ArmyManagementVM>>(
                AccessTools.Method(typeof(ArmyManagementVM), "ApplyCohesionChange")
            );

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            int
        > InfluenceSpentForCohesionBoostingRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                int
            >("_influenceSpentForCohesionBoosting");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            Action
        > OnCloseRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                Action
            >("_onClose");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            MBBindingList<ArmyManagementItemVM>
        > PartyListRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                MBBindingList<ArmyManagementItemVM>
            >("_partyList");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            List<MobileParty>
        > CurrentPartiesRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                List<MobileParty>
            >("_currentParties");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            ArmyManagementItemVM
        > MainPartyItemRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                ArmyManagementItemVM
            >("_mainPartyItem");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            float
        > InitialInfluenceRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                float
            >("_initialInfluence");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            MBBindingList<ArmyManagementItemVM>
        > PartiesToRemoveRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                MBBindingList<ArmyManagementItemVM>
            >("_partiesToRemove");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            int
        > BoostedCohesionRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                int
            >("_boostedCohesion");

        private static readonly AccessTools.FieldRef<
            ArmyManagementVM,
            bool
        > PlayerHasArmyRef =
            AccessTools.FieldRefAccess<
                ArmyManagementVM,
                bool
            >("_playerHasArmy");

        public static MBBindingList<ArmyManagementItemVM> GetPartiesToRemove(ArmyManagementVM instance)
        {
            return PartiesToRemoveRef(instance);
        }

    }
}
