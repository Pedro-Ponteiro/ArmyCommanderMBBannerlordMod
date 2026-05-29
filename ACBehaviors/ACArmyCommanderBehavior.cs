using ArmyCommander.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem;
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
            Log("ACArmyCommanderBehavior constructed. StringId=" + BehaviorStringId);
        }

        public override void RegisterEvents()
        {
            Log("RegisterEvents called.");
        }

        public override void SyncData(IDataStore dataStore)
        {
            Log("SyncData called. IsSaving=" + dataStore.IsSaving + ", IsLoading=" + dataStore.IsLoading);

            string serializedArmyCommands = null;

            if (dataStore.IsSaving)
            {
                serializedArmyCommands = SerializeArmyCommands();
                Log("Serialized army commands length=" + (serializedArmyCommands?.Length ?? 0));
            }

            dataStore.SyncData(ArmyCommandsDataKey, ref serializedArmyCommands);

            if (dataStore.IsLoading)
            {
                RestoreArmyCommands(serializedArmyCommands);
                Log("Restored army commands count=" + ArmyCommandsBehaviorStore.army_commands.Count);
            }
        }

        private static string SerializeArmyCommands()
        {
            XElement root = new XElement("ArmyCommands",
                new XAttribute("version", "1"));

            foreach (KeyValuePair<Army, (Army.ArmyTypes ArmyType, Settlement Settlement)> entry in ArmyCommandsBehaviorStore.army_commands.ToList())
            {
                Army army = entry.Key;
                Army.ArmyTypes armyType = entry.Value.ArmyType;
                Settlement targetSettlement = entry.Value.Settlement;

                if (!TryGetPersistableCommand(army, armyType, targetSettlement, out string leaderHeroId, out string targetSettlementId))
                {
                    continue;
                }

                root.Add(new XElement("Command",
                    new XAttribute("leaderHeroId", leaderHeroId),
                    new XAttribute("targetSettlementId", targetSettlementId),
                    new XAttribute("armyType", (int)armyType)));
            }

            return root.ToString(SaveOptions.DisableFormatting);
        }

        private static bool TryGetPersistableCommand(
            Army army,
            Army.ArmyTypes armyType,
            Settlement targetSettlement,
            out string leaderHeroId,
            out string targetSettlementId)
        {
            leaderHeroId = null;
            targetSettlementId = null;

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

            return !string.IsNullOrEmpty(leaderHeroId)
                && !string.IsNullOrEmpty(targetSettlementId);
        }

        private static void RestoreArmyCommands(string serializedArmyCommands)
        {
            Dictionary<Army, (Army.ArmyTypes ArmyType, Settlement Settlement)> restoredCommands =
                new Dictionary<Army, (Army.ArmyTypes ArmyType, Settlement Settlement)>();

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
                    int armyTypeValue = (int?)commandElement.Attribute("armyType") ?? -1;
                    Army.ArmyTypes armyType = (Army.ArmyTypes)armyTypeValue;

                    if (!IsSupportedCommandType(armyType))
                    {
                        continue;
                    }

                    Army army = FindArmyByLeaderHeroId(leaderHeroId);
                    Settlement targetSettlement = Settlement.Find(targetSettlementId);

                    if (army == null || targetSettlement == null)
                    {
                        continue;
                    }

                    restoredCommands[army] = (armyType, targetSettlement);
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
