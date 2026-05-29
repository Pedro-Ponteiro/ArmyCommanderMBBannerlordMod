using ArmyCommander.Store;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultPolicies))]
    internal static class DefaultPolicies_InitializeAll_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("InitializeAll")]
        private static void Postfix(DefaultPolicies __instance)
        {
            PolicyObject mercPolicy = Create(__instance, "army_commander_mercenary_army_leaders");

            mercPolicy.Initialize(
                new TextObject("{=ac_policy_merc_army_leaders_name}Mercenary Army Leaders"),
                new TextObject("{=ac_policy_merc_army_leaders_desc}Mercenary clans are allowed to form and lead armies in service of the kingdom."),
                new TextObject("{=ac_policy_merc_army_leaders_log}granting mercenary clans the authority to command armies."),
                new TextObject(
                    "{=ac_policy_merc_army_leaders_impact}" +
                    "Mercenary clans can form and lead armies in service of the kingdom." +
                    "{newline}The ruling clan pays 100 influence whenever a mercenary army is formed."
                ),
                authoritarianWeight: -0.45f,
                oligarchyWeight: 0.35f,
                egalitarianWeight: 0f
            );

            ACPolicyStore.MercenaryArmyLeadersPolicy = mercPolicy;
        }

        [HarmonyReversePatch]
        [HarmonyPatch("Create")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static PolicyObject Create(DefaultPolicies instance, string stringId)
        {
            throw new NotImplementedException("Harmony reverse patch stub (PolicyObject.Create).");
        }
    }
}