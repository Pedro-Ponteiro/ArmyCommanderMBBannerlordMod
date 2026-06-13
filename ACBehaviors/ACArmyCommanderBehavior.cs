using ArmyCommander.Helpers;
using ArmyCommander.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace ArmyCommander.ACBehaviors
{
    public sealed class ACArmyCommanderBehavior : CampaignBehaviorBase
    {
        private const string ArmyCommandsDataKey = "ArmyCommander.ArmyCommands.v1";
        private const string BehaviorStringId = "ArmyCommander.ACArmyCommanderBehavior";

        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArmyCommander"
        );

        private static readonly string LogPath = Path.Combine(
            LogDirectory,
            "ArmyCommander_Behavior.log"
        );

        public ACArmyCommanderBehavior()
            : base(BehaviorStringId)
        {

        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.OnPeaceOfferResolvedEvent.AddNonSerializedListener(this, OnPeaceOfferResolved);
            CampaignEvents.PartyAttachedAnotherParty.AddNonSerializedListener(this, OnPartyAttachedAnotherParty);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, ACPartyHourlyAiTick);

        }

        private void ACPartyHourlyAiTick()
        {
            // UGLY CODE THAT SHOULD NOT EXIST

            List<Army> holdingArmies = ArmyCommandsBehaviorStore.army_commands.Keys
                    .Where(army => army?.LeaderParty?.DefaultBehavior == 0)
                    .ToList();

            foreach (var army in holdingArmies)
            {
                ACAIBehaviorHelpers.ReEnableAI(army.LeaderParty);
            }
        }

        private void OnPartyAttachedAnotherParty(MobileParty attached_party)
        {
            if (attached_party.Army != null &&
                ArmyCommandsBehaviorStore.army_commands.Keys.Contains(attached_party.Army))
            {
                ACAIBehaviorHelpers.ReEnableAI(attached_party.Army.LeaderParty);
            }
        }

        private void OnPeaceOfferResolved(IFaction faction)
        {
            RefreshArmyCommandsStore();
        }

        private void OnSettlementOwnerChanged(
            Settlement settlement,
            bool openToClaim,
            Hero newOwner,
            Hero oldOwner,
            Hero capturerHero,
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail
        )
        {
            if (Clan.PlayerClan?.Kingdom == null)
            {
                return;
            }

            if (!settlement.IsTown && !settlement.IsCastle)
            {
                return;
            }

            if (oldOwner.Clan?.Kingdom == Clan.PlayerClan.Kingdom)
            {
                if (newOwner.Clan?.Kingdom != Clan.PlayerClan.Kingdom)
                {
                    // Allied Settlement was captured or clan defected to another kingdom.
                    // TODO: Add penalties to Army Commander Points if capture.

                    RefreshArmyCommandsStore();
                }
            }
            else if (newOwner.Clan?.Kingdom == Clan.PlayerClan.Kingdom)
            {
                if (oldOwner.Clan?.Kingdom != Clan.PlayerClan.Kingdom)
                {
                    // Enemy Settlement was captured by the player kingdom
                    // or the owner clan joined player's kingdom
                    // TODO: Add bonuses to Army Commander Points if captured by the player commanded army.
                    RefreshArmyCommandsStore();
                }
            }
            else if (Clan.PlayerClan.Kingdom.IsAtWarWith(oldOwner.Clan.Kingdom))
            {
                // old settlement owner is at war with kingdom player.
                // new owner can be a friend, or not.
                // if the new owner is also at war with the player, the command remains intact.
                RefreshArmyCommandsStore();
            }
        }

        private static void RefreshArmyCommandsStore()
        {
            var army_command_kvps = ArmyCommandsBehaviorStore.army_commands.ToList();


            foreach (var army_command_kvp in army_command_kvps)
            {
                Army army = army_command_kvp.Key;

                bool isWaitingForArmyMembers = army.IsWaitingForArmyMembers();

                bool default_fallback_applied = ACAIBehaviorHelpers.ValidatePlayerCommandAndAskIfNeeded(army, isWaitingForArmyMembers);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {


            string serializedArmyCommands = null;

            if (dataStore.IsSaving)
            {
                serializedArmyCommands = SerializeArmyCommands();

            }

            dataStore.SyncData(ArmyCommandsDataKey, ref serializedArmyCommands);

            if (dataStore.IsLoading)
            {
                RestoreArmyCommands(serializedArmyCommands);

            }
        }

        private static string SerializeArmyCommands()
        {
            XElement root = new XElement("ArmyCommands",
                new XAttribute("version", "1"));

            foreach (KeyValuePair<Army, (Army.ArmyTypes ArmyType,
                Settlement TargetSettlement,
                Settlement GatherSettlement,
                bool CanEngageEnemyParties,
                bool CanHelpAlliedParties,
                bool CanResupply,
                bool CanRunFromDanger)> entry in ArmyCommandsBehaviorStore.army_commands.ToList())
            {
                Army army = entry.Key;
                Army.ArmyTypes armyType = entry.Value.ArmyType;
                Settlement targetSettlement = entry.Value.TargetSettlement;
                Settlement gatherSettlement = entry.Value.GatherSettlement;


                if (!TryGetPersistableCommand(army,
                    armyType,
                    targetSettlement,
                    gatherSettlement,
                    out string leaderHeroId,
                    out string targetSettlementId,
                    out string gatherSettlementId))
                {
                    continue;
                }

                root.Add(
                    new XElement(
                        "Command",
                        new XAttribute("leaderHeroId", leaderHeroId),
                        new XAttribute("armyType", (int)armyType),
                        new XAttribute("targetSettlementId", targetSettlementId),
                        new XAttribute("gatherSettlementId", gatherSettlementId),
                        new XAttribute("CanEngageEnemyParties", entry.Value.CanEngageEnemyParties),
                        new XAttribute("CanHelpAlliedParties", entry.Value.CanHelpAlliedParties),
                        new XAttribute("CanRunFromDanger", entry.Value.CanRunFromDanger),
                        new XAttribute("CanResupply", entry.Value.CanResupply)
                    )
                );
            }

            return root.ToString(SaveOptions.DisableFormatting);
        }

        private static bool TryGetPersistableCommand(
            Army army,
            Army.ArmyTypes armyType,
            Settlement targetSettlement,
            Settlement gatherSettlement,
            out string leaderHeroId,
            out string targetSettlementId,
            out string gatherSettlementId)
        {
            leaderHeroId = null;
            targetSettlementId = null;
            gatherSettlementId = null;

            if (army == null || army.LeaderParty == null || army.LeaderParty.LeaderHero == null || targetSettlement == null)
            {
                return false;
            }

            if (!IsSupportedCommandType(armyType))
            {
                return false;
            }

            leaderHeroId = army.LeaderParty.LeaderHero.StringId;
            targetSettlementId = targetSettlement.StringId;
            gatherSettlementId = gatherSettlement?.StringId ?? "";

            return !string.IsNullOrEmpty(leaderHeroId)
                && !string.IsNullOrEmpty(targetSettlementId);
        }

        private static void RestoreArmyCommands(string serializedArmyCommands)
        {
            Dictionary<Army, (Army.ArmyTypes ArmyType,
                Settlement TargetSettlement,
                Settlement GatherSettlement,
                bool CanEngageEnemyParties,
                bool CanHelpAlliedParties,
                bool CanResupply,
                bool CanRunFromDanger)> restoredCommands =
                new Dictionary<Army, (Army.ArmyTypes ArmyType,
                Settlement TargetSettlement,
                Settlement GatherSettlement,
                bool CanEngageEnemyParties,
                bool CanHelpAlliedParties,
                bool CanResupply,
                bool CanRunFromDanger)>();

            if (string.IsNullOrEmpty(serializedArmyCommands))
            {
                ArmyCommandsBehaviorStore.army_commands = restoredCommands;
                return;
            }

            try
            {
                XElement root = XElement.Parse(serializedArmyCommands);

                foreach (XElement commandElement in root.Elements("Command"))
                {
                    string leaderHeroId = (string)commandElement.Attribute("leaderHeroId");
                    string targetSettlementId = (string)commandElement.Attribute("targetSettlementId");
                    string gatherSettlementId = (string)commandElement.Attribute("gatherSettlementId") ?? "";
                    int armyTypeValue = (int?)commandElement.Attribute("armyType") ?? -1;
                    Army.ArmyTypes armyType = (Army.ArmyTypes)armyTypeValue;

                    bool canEngageEnemyParties = (bool?)commandElement.Attribute("CanEngageEnemyParties") ?? true;
                    bool canHelpAlliedParties = (bool?)commandElement.Attribute("CanHelpAlliedParties") ?? true;
                    bool canRunFromDanger = (bool?)commandElement.Attribute("CanRunFromDanger") ?? true;
                    bool canResupply = (bool?)commandElement.Attribute("CanResupply") ?? true;

                    if (!IsSupportedCommandType(armyType))
                    {
                        continue;
                    }

                    Army army = FindArmyByLeaderHeroId(leaderHeroId);
                    Settlement targetSettlement = Settlement.Find(targetSettlementId);
                    Settlement gatherSettlement = Settlement.Find(gatherSettlementId);


                    if (army == null || targetSettlement == null)
                    {
                        continue;
                    }

                    restoredCommands[army] = (armyType, targetSettlement, gatherSettlement, canEngageEnemyParties, canHelpAlliedParties, canResupply, canRunFromDanger);
                }
            }
            catch
            {
                restoredCommands.Clear();
            }

            ArmyCommandsBehaviorStore.army_commands = restoredCommands;
        }

        private static Army FindArmyByLeaderHeroId(string leaderHeroId)
        {
            if (string.IsNullOrEmpty(leaderHeroId))
            {
                return null;
            }

            Army army1 = Clan.PlayerClan.Kingdom.Armies.FirstOrDefault((army) => army.LeaderParty.LeaderHero.StringId == leaderHeroId);

            return army1;
        }

        private static bool IsSupportedCommandType(Army.ArmyTypes armyType)
        {
            return armyType == Army.ArmyTypes.Besieger
                || armyType == Army.ArmyTypes.Defender;
        }

        private static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(
                    LogPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + message + Environment.NewLine
                );
            }
            catch
            {
            }
        }
    }
}
