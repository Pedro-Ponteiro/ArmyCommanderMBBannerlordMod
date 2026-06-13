using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using Helpers;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Armies;

namespace ArmyCommander.HarmonyPatches
{

    [HarmonyPatch]
    internal static class ArmyManagementItemVM_Distance_Getter_Patch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(
                typeof(ArmyManagementItemVM),
                "_distance"
            );
        }

        private static void Postfix(ArmyManagementItemVM __instance, ref float __result)
        {
            var context = ACArmyManagementUIContext.Instance;

            if (context?.currentMainParty != null &&
                context.currentMainParty.IsMainParty == false)
            {
                __result = DistanceHelper.FindClosestDistanceFromMobilePartyToMobileParty(
                    __instance.Party,
                    context.currentMainParty,
                    __instance.Party.NavigationCapability
                );
            }
        }
    }

    [HarmonyPatch(typeof(ArmyManagementItemVM))]
    internal static class ArmyManagementItemVM_DistInTime_Getter_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArmyManagementItemVM.DistInTime), MethodType.Getter)]
        private static void Postfix(ArmyManagementItemVM __instance, ref float __result)
        {
            if (__instance?.Party == null || __instance.Party.Speed <= 0f)
            {
                return;
            }

            __result = TaleWorlds.Library.MathF.Ceiling(
                __instance._distance / __instance.Party.Speed
            );
        }
    }

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