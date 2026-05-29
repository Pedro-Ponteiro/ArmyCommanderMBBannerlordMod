using ArmyCommander.Store;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using Messages.FromClient.ToLobbyServer;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch]
    internal static class SetPartyAiActionOriginal
    {
        private static MethodBase TargetMethod()
        {
            Type detailType = AccessTools.Inner(
                typeof(SetPartyAiAction),
                "SetPartyAiActionDetail"
            );

            return AccessTools.Method(
                typeof(SetPartyAiAction),
                "ApplyInternal",
                new Type[]
                {
                    typeof(MobileParty),
                    typeof(Settlement),
                    typeof(MobileParty),
                    typeof(CampaignVec2),
                    detailType,
                    typeof(MobileParty.NavigationType),
                    typeof(bool),
                    typeof(bool)
                });
        }

        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ApplyInternal(
            MobileParty owner,
            Settlement settlement,
            MobileParty mobileParty,
            CampaignVec2 position,
            int detail,
            MobileParty.NavigationType navigationType,
            bool isFromPort,
            bool isTargetingPort)
        {
            throw new NotImplementedException("Harmony reverse patch stub (ApplyInternal).");
        }
    }


    [HarmonyPatch(typeof(SetPartyAiAction), "ApplyInternal")]
    internal static class SetPartyAiAction_ApplyInternal_Patch
    {
        private static bool Prefix(
            MobileParty owner,
            object[] __args)
        {

            if (!Hero.MainHero.IsKingdomLeader || Hero.MainHero.Clan.Kingdom != owner.ActualClan?.Kingdom)
            {
                return true;
            }

            if (owner.Army == null || owner.Army.IsWaitingForArmyMembers())
            {
                return true;
            }


            if (ArmyCommandsBehaviorStore.army_commands.TryGetValue(owner.Army, out var command)) 
            {
                Army.ArmyTypes c_armyType = command.ArmyType;
                Settlement c_settlement = command.Settlement;

                object detail = __args[4];
                string detailName = detail?.ToString();
                if (detailName == "GoToSettlement" 
                    || detailName == "PatrolAroundSettlement" 
                    || detailName == "PatrolAroundPoint" 
                    || detailName == "RaidSettlement"
                    || detailName == "BesiegeSettlement"
                    //|| detailName == "EngageParty"
                    //|| detailName == "GoAroundParty"
                    || detailName == "DefendParty" // this is actually "defend settlement"
                    || detailName == "EscortParty"
                    //|| detailName == "MoveToNearestLand"
                )
                {
                    if (c_armyType == Army.ArmyTypes.Besieger)
                    {

                        if (!command.Settlement.OwnerClan.Kingdom.IsAtWarWith(Hero.MainHero.Clan.Kingdom))
                        {
                            ArmyCommandsBehaviorStore.army_commands.Remove(owner.Army);
                            return true;
                        }
                        SetPartyAiActionOriginal.ApplyInternal(owner, c_settlement, null, CampaignVec2.Zero, 4, owner.DesiredAiNavigationType, owner.CurrentSettlement?.HasPort == true, isTargetingPort: false);
                        return false;
                    }
                    else if (c_armyType == Army.ArmyTypes.Defender)
                    {
                        if (command.Settlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
                        {
                            ArmyCommandsBehaviorStore.army_commands.Remove(owner.Army);
                            return true;
                        }
                        SetPartyAiActionOriginal.ApplyInternal(owner, c_settlement, null, CampaignVec2.Zero, 7,  owner.DesiredAiNavigationType, owner.CurrentSettlement?.HasPort == true, owner.IsCurrentlyAtSea);
                        return false;
                    }
                    else
                    {
                        throw new NotImplementedException($"Army Command Type is invalid (should be defender or besieger, is {c_armyType.ToString()})");
                    }
                }
            }

            return true;
        }
    }
}