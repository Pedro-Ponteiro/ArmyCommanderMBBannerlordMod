using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace ArmyCommander.Behavior
{
    public partial class ArmyCommanderBehavior
    {



        public void OnNewGameCreated(CampaignGameStarter gameStarter)
        {
            foreach (MobileParty item in MobileParty.All)
            {
                for (int i = 0; i < 6; i++)
                {
                    PartyHourlyAiTick(item);
                }
            }
        }

        public void PartyHourlyAiTick(MobileParty mobileParty)
        {

            //return;


            //if (mobileParty.ActualClan.IsUnderMercenaryService && mobileParty.MapFaction.IsKingdomFaction)
            //{
            //    GarantirExistenciaDoArquivo();
            //}


            if (mobileParty.Ai.IsDisabled || mobileParty.Ai.DoNotMakeNewDecisions)
            {
                return;
            }

            //  || mobileParty.LeaderHero.Clan.Kingdom == null
            if (mobileParty.LeaderHero?.Clan?.IsUnderMercenaryService != true)
            {
                return;
            }

            bool flag = mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty;
            bool flag2 = mobileParty.Army != null && mobileParty.AttachedTo == null;
            bool isTransitionInProgress = mobileParty.IsTransitionInProgress;
            int num = 6;
            if (flag || isTransitionInProgress || flag2 || mobileParty.Ai.RethinkAtNextHourlyTick || (mobileParty.MapEvent != null && (mobileParty.MapEvent.IsRaid || mobileParty.MapEvent.IsSiegeAssault)))
            {
                num = ((!flag2 || isTransitionInProgress) ? 1 : 3);
            }
            if (flag && MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty == mobileParty && (mobileParty.CurrentSettlement != null || (mobileParty.LastVisitedSettlement != null && mobileParty.MapEvent == null && mobileParty.LastVisitedSettlement.Position.Distance(mobileParty.Position) < 1f)))
            {
                num = 6;
            }
            if (mobileParty.Ai.HourCounter % num == 0 && mobileParty != MobileParty.MainParty && (mobileParty.MapEvent == null || (mobileParty.Party == mobileParty.MapEvent.AttackerSide.LeaderParty && (mobileParty.MapEvent.IsRaid || mobileParty.MapEvent.IsSiegeAssault))))
            {
                mobileParty.Ai.HourCounter = 0;
                AiBehavior aiBehavior = ((!flag) ? AiBehavior.None : mobileParty.Army.LeaderParty.DefaultBehavior);
                IMapPoint mapPoint = (flag ? mobileParty.Army.AiBehaviorObject : null);
                mobileParty.Ai.RethinkAtNextHourlyTick = false;
                PartyThinkParams thinkParamsCache = mobileParty.ThinkParamsCache;
                thinkParamsCache.Reset(mobileParty);
                CampaignEventDispatcher.Instance.AiHourlyTick(mobileParty, thinkParamsCache);
                AIBehaviorData aIBehaviorData = AIBehaviorData.Invalid;
                AIBehaviorData aIBehaviorData2 = AIBehaviorData.Invalid;
                float num2 = -1f;
                float num3 = -1f;
                foreach (var aIBehaviorScore in thinkParamsCache.AIBehaviorScores)
                {
                    float item = aIBehaviorScore.Item2;
                    if (item > num2)
                    {
                        num2 = item;
                        (aIBehaviorData, _) = aIBehaviorScore;
                    }
                    if (item > num3 && !aIBehaviorScore.Item1.WillGatherArmy)
                    {
                        num3 = item;
                        (aIBehaviorData2, _) = aIBehaviorScore;
                    }
                }
                if (aIBehaviorData != AIBehaviorData.Invalid)
                {
                    if (mobileParty.DefaultBehavior == AiBehavior.Hold || mobileParty.Ai.RethinkAtNextHourlyTick || thinkParamsCache.CurrentObjectiveValue < 0.05f)
                    {
                        num2 = 1f;
                    }
                    double num4 = ((aIBehaviorData.AiBehavior == AiBehavior.PatrolAroundPoint || aIBehaviorData.AiBehavior == AiBehavior.GoToSettlement) ? 0.03 : 0.1);
                    num4 *= (double)(aIBehaviorData.WillGatherArmy ? 2f : ((mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty) ? 0.33f : 1f));
                    bool flag3 = mobileParty.Army != null;
                    for (int i = 0; i < num; i++)
                    {
                        if (flag3)
                        {
                            break;
                        }
                        flag3 = MBRandom.RandomFloat < num2;
                    }
                    if (((double)num2 > num4 && flag3) || (num2 > 0.01f && mobileParty.MapEvent == null && mobileParty.Army == null && mobileParty.DefaultBehavior == AiBehavior.Hold))
                    {
                        if (mobileParty.MapEvent != null && mobileParty.Party == mobileParty.MapEvent.AttackerSide.LeaderParty && !thinkParamsCache.DoNotChangeBehavior && (aIBehaviorData.Party != mobileParty.MapEvent.MapEventSettlement || (aIBehaviorData.AiBehavior != AiBehavior.RaidSettlement && aIBehaviorData.AiBehavior != AiBehavior.BesiegeSettlement && aIBehaviorData.AiBehavior != AiBehavior.AssaultSettlement)))
                        {
                            if (PlayerEncounter.Current != null && PlayerEncounter.Battle == mobileParty.MapEvent)
                            {
                                PlayerEncounter.Finish();
                            }
                            if (mobileParty.MapEvent != null)
                            {
                                mobileParty.MapEvent.FinalizeEvent();
                            }
                            if (mobileParty.SiegeEvent != null)
                            {
                                mobileParty.SiegeEvent.FinalizeSiegeEvent();
                            }
                        }
                        if ((double)num2 <= num4)
                        {
                            aIBehaviorData = aIBehaviorData2;
                        }
                        bool flag4 = aIBehaviorData.AiBehavior == AiBehavior.RaidSettlement || aIBehaviorData.AiBehavior == AiBehavior.BesiegeSettlement || aIBehaviorData.AiBehavior == AiBehavior.DefendSettlement || aIBehaviorData.AiBehavior == AiBehavior.PatrolAroundPoint;
                        if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty && (mobileParty.CurrentSettlement == null || mobileParty.CurrentSettlement.SiegeEvent == null) && !(aIBehaviorData.AiBehavior == AiBehavior.GoAroundParty || aIBehaviorData.AiBehavior == AiBehavior.PatrolAroundPoint || aIBehaviorData.AiBehavior == AiBehavior.GoToSettlement || flag4))
                        {
                            DisbandArmyAction.ApplyByUnknownReason(mobileParty.Army);
                        }
                        if (flag4 && mobileParty.Army == null && aIBehaviorData.WillGatherArmy)
                        {
                            bool flag5 = MBRandom.RandomFloat < num2;
                            if (aIBehaviorData.AiBehavior == AiBehavior.DefendSettlement || flag5)
                            {
                                Army.ArmyTypes selectedArmyType = ((aIBehaviorData.AiBehavior != AiBehavior.BesiegeSettlement) ? ((aIBehaviorData.AiBehavior == AiBehavior.RaidSettlement) ? Army.ArmyTypes.Raider : Army.ArmyTypes.Defender) : Army.ArmyTypes.Besieger);
                                ((Kingdom)mobileParty.LeaderHero.Clan.Kingdom).CreateArmy(mobileParty.LeaderHero, aIBehaviorData.Party as Settlement, selectedArmyType, thinkParamsCache.PossibleArmyMembersUponArmyCreation);
                            }
                        }
                        else if (!thinkParamsCache.DoNotChangeBehavior)
                        {
                            if (aIBehaviorData.AiBehavior == AiBehavior.PatrolAroundPoint)
                            {
                                if (aIBehaviorData.Party != null)
                                {
                                    SetPartyAiAction.GetActionForPatrollingAroundSettlement(mobileParty, (Settlement)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort, aIBehaviorData.IsTargetingPort);
                                }
                                else
                                {
                                    SetPartyAiAction.GetActionForPatrollingAroundPoint(mobileParty, aIBehaviorData.Position, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort);
                                }
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.GoToSettlement)
                            {
                                if (MobilePartyHelper.GetCurrentSettlementOfMobilePartyForAICalculation(mobileParty) != aIBehaviorData.Party)
                                {
                                    SetPartyAiAction.GetActionForVisitingSettlement(mobileParty, (Settlement)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort, aIBehaviorData.IsTargetingPort);
                                }
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.EscortParty)
                            {
                                SetPartyAiAction.GetActionForEscortingParty(mobileParty, (MobileParty)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort, aIBehaviorData.IsTargetingPort);
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.RaidSettlement)
                            {
                                if (mobileParty.MapEvent == null || !mobileParty.MapEvent.IsRaid || mobileParty.MapEvent.MapEventSettlement != aIBehaviorData.Party)
                                {
                                    SetPartyAiAction.GetActionForRaidingSettlement(mobileParty, (Settlement)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort);
                                }
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.BesiegeSettlement)
                            {
                                if (mobileParty.MapEvent == null || !mobileParty.MapEvent.IsSiegeAssault || mobileParty.MapEvent.MapEventSettlement != aIBehaviorData.Party)
                                {
                                    SetPartyAiAction.GetActionForBesiegingSettlement(mobileParty, (Settlement)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort);
                                }
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.DefendSettlement && mobileParty.CurrentSettlement != aIBehaviorData.Party)
                            {
                                SetPartyAiAction.GetActionForDefendingSettlement(mobileParty, (Settlement)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort, aIBehaviorData.IsTargetingPort);
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.GoAroundParty)
                            {
                                SetPartyAiAction.GetActionForGoingAroundParty(mobileParty, (MobileParty)aIBehaviorData.Party, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort);
                            }
                            else if (aIBehaviorData.AiBehavior == AiBehavior.MoveToNearestLandOrPort)
                            {
                                SetPartyAiAction.GetActionForMovingToNearestLand(mobileParty, (Settlement)aIBehaviorData.Party);
                            }
                        }
                    }
                    else if (aIBehaviorData.AiBehavior != AiBehavior.None)
                    {
                        if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty && !mobileParty.Army.IsWaitingForArmyMembers())
                        {
                            DisbandArmyAction.ApplyByUnknownReason(mobileParty.Army);
                        }
                        else if (mobileParty.Army != null && mobileParty.CurrentSettlement == null && mobileParty != mobileParty.Army.LeaderParty && !thinkParamsCache.DoNotChangeBehavior)
                        {
                            SetPartyAiAction.GetActionForEscortingParty(mobileParty, mobileParty.Army.LeaderParty, aIBehaviorData.NavigationType, aIBehaviorData.IsFromPort, aIBehaviorData.IsTargetingPort);
                        }
                    }
                    if (MobileParty.MainParty.Army != null && mobileParty == MobileParty.MainParty.Army.LeaderParty && (aiBehavior != mobileParty.Army.LeaderParty.DefaultBehavior || mobileParty.Army.AiBehaviorObject != mapPoint))
                    {
                        CampaignEventDispatcher.Instance.OnPlayerArmyLeaderChangedBehavior();
                    }
                }
            }
            mobileParty.Ai.HourCounter++;
        }
    }
}
