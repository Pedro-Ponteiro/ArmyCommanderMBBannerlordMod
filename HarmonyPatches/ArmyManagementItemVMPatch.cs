using HarmonyLib;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Armies;

namespace ArmyCommander.HarmonyPatches
{

    [HarmonyPatch]
    internal static class ArmyManagementItemVMOriginal
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyManagementItemVM), "get_NameText")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetNameText(ArmyManagementItemVM instance)
        {
            throw new NotImplementedException("Reverse patch stub (get_NameText).");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyManagementItemVM), "get_Strength")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int GetStrength(ArmyManagementItemVM instance)
        {
            throw new NotImplementedException("Reverse patch stub (get_Strength).");
        }
    }

    [HarmonyPatch(typeof(ArmyManagementItemVM))]
    internal static class ArmyManagementItemVM_NameText_Getter_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArmyManagementItemVM.NameText), MethodType.Getter)]
        private static bool Prefix(ArmyManagementItemVM __instance, ref string __result)
        {
            string originalNameText = ArmyManagementItemVMOriginal.GetNameText(__instance);

            if (originalNameText == null)
            {
                return true;
            }

            if (__instance.IsAlreadyWithPlayer)
            {
                return true;
            }

            if (__instance.Party.Army != null && __instance.Party.Army.LeaderParty == __instance.Party)
            {
                __result = __instance.Party.Army.Name.ToString();

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ArmyManagementItemVM))]
    internal static class ArmyManagementItemVM_Strength_Getter_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArmyManagementItemVM.Strength), MethodType.Getter)]
        private static bool Prefix(ArmyManagementItemVM __instance, ref int __result)
        {
            int originalStrength = ArmyManagementItemVMOriginal.GetStrength(__instance);

            if (originalStrength == -1)
            {
                return true;
            }

            if (__instance.IsAlreadyWithPlayer)
            {
                return true;
            }

            if (__instance.Party.Army != null && __instance.Party.Army.LeaderParty == __instance.Party)
            {
                __result = __instance.Party.Army.Parties.Sum(mp => mp.Party.NumberOfHealthyMembers);

                return false;
            }

            return true;
        }
    }
}