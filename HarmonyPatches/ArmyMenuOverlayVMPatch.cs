using ArmyCommander.Helpers;
using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Localization;

namespace ArmyCommander.HarmonyPatches
{
    // ========================================================================
    // ArmyMenuOverlayVM Focused Harmony Patch
    // ------------------------------------------------------------------------
    // Patches replacing:
    // - ArmyToUse getter
    // - OnFrameTick
    // - ExecuteOpenArmyManagement
    // - OnPartyAttachedAnotherParty
    // ========================================================================

    public static class ArmyMenuOverlayVMAccess
    {
        // --------------------------------------------------------------------
        // Generic field/property/method helpers for any owner type
        // --------------------------------------------------------------------

        public static void SetFieldFromType<T>(object instance, Type ownerType, string fieldName, T value)
        {
            FieldInfo field = AccessTools.Field(ownerType, fieldName);

            if (field == null)
            {
                throw new MissingFieldException(ownerType.FullName, fieldName);
            }

            field.SetValue(instance, value);
        }

        public static void SetPropertyFromType<T>(object instance, Type ownerType, string propertyName, T value)
        {
            PropertyInfo property = AccessTools.Property(ownerType, propertyName);

            if (property == null)
            {
                throw new MissingMemberException(ownerType.FullName, propertyName);
            }

            property.SetValue(instance, value, null);
        }

        public static void InvokePrivateMethod(object instance, Type ownerType, string methodName, params object[] args)
        {
            MethodInfo method = AccessTools.Method(ownerType, methodName);

            if (method == null)
            {
                throw new MissingMethodException(ownerType.FullName, methodName);
            }

            method.Invoke(instance, args);
        }

        public static Army GetArmyToUse(ArmyMenuOverlayVM instance)
        {
            return GetProperty<Army>(instance, "ArmyToUse");
        }

        public static TDelegate CreatePrivateInstanceDelegate<TDelegate>(ArmyMenuOverlayVM instance, string methodName)
            where TDelegate : class
        {
            MethodInfo method = AccessTools.Method(typeof(ArmyMenuOverlayVM), methodName);

            if (method == null)
            {
                throw new MissingMethodException(typeof(ArmyMenuOverlayVM).FullName, methodName);
            }

            return Delegate.CreateDelegate(typeof(TDelegate), instance, method) as TDelegate;
        }

        public static void SetBaseProperty<T>(ArmyMenuOverlayVM instance, string propertyName, T value)
        {
            SetPropertyFromType(instance, typeof(GameMenuOverlay), propertyName, value);
        }

        public static void SetBaseField<T>(ArmyMenuOverlayVM instance, string fieldName, T value)
        {
            SetFieldFromType(instance, typeof(GameMenuOverlay), fieldName, value);
        }

        public static void SetCurrentOverlayType(ArmyMenuOverlayVM instance, int value)
        {
            SetBaseProperty(instance, "CurrentOverlayType", value);
        }

        public static void SetIsInitializationOver(ArmyMenuOverlayVM instance, bool value)
        {
            SetBaseProperty(instance, "IsInitializationOver", value);
        }

        public static void SetContextMenuItemToNull(ArmyMenuOverlayVM instance)
        {
            // Inherited field from GameMenuOverlay. If the name/type changes between versions,
            // this is one of the first places to check in dnSpy.
            SetBaseField<object>(instance, "_contextMenuItem", null);
        }

        // --------------------------------------------------------------------
        // Generic field helpers
        // --------------------------------------------------------------------

        public static T GetField<T>(ArmyMenuOverlayVM instance, string fieldName)
        {
            FieldInfo field = AccessTools.Field(typeof(ArmyMenuOverlayVM), fieldName);

            if (field == null)
            {
                throw new MissingFieldException(typeof(ArmyMenuOverlayVM).FullName, fieldName);
            }

            return (T)field.GetValue(instance);
        }

        public static void SetField<T>(ArmyMenuOverlayVM instance, string fieldName, T value)
        {
            FieldInfo field = AccessTools.Field(typeof(ArmyMenuOverlayVM), fieldName);

            if (field == null)
            {
                throw new MissingFieldException(typeof(ArmyMenuOverlayVM).FullName, fieldName);
            }

            field.SetValue(instance, value);
        }

        // --------------------------------------------------------------------
        // Generic property helpers
        // --------------------------------------------------------------------

        public static T GetProperty<T>(ArmyMenuOverlayVM instance, string propertyName)
        {
            PropertyInfo property = AccessTools.Property(typeof(ArmyMenuOverlayVM), propertyName);

            if (property == null)
            {
                throw new MissingMemberException(typeof(ArmyMenuOverlayVM).FullName, propertyName);
            }

            return (T)property.GetValue(instance, null);
        }

        // --------------------------------------------------------------------
        // Specific field helpers
        // --------------------------------------------------------------------

        public static bool GetIsVisualsDirty(ArmyMenuOverlayVM instance)
        {
            return GetField<bool>(instance, "_isVisualsDirty");
        }

        public static void SetIsVisualsDirty(ArmyMenuOverlayVM instance, bool value)
        {
            SetField(instance, "_isVisualsDirty", value);
        }
    }

    // ========================================================================
    // ReversePatch: ArmyMenuOverlayVM.Refresh
    // ------------------------------------------------------------------------
    // This patch creates a safe way to call the original body of:
    // ArmyMenuOverlayVM.Refresh()
    // even when you are inside another patch.
    // ========================================================================

    [HarmonyPatch]
    public static class ArmyMenuOverlayVM_Refresh_ReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmyMenuOverlayVM), "Refresh")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OriginalRefresh(ArmyMenuOverlayVM instance)
        {
            throw new NotImplementedException("Stub de ReversePatch (ArmyMenuOverlayVM_Refresh_ReversePatch)");
        }
    }


    // ========================================================================
    // 1. ArmyToUse getter
    // ========================================================================

    [HarmonyPatch]
    internal static class ArmyMenuOverlayVM_get_ArmyToUse_Patch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(ArmyMenuOverlayVM), "ArmyToUse");
        }

        private static bool Prefix(ArmyMenuOverlayVM __instance, ref Army __result)
        {

            // TODO: MAKE THIS PRETTIER AND MORE ELEGANT! (THE INSTANCE MAY NOT EXIST WHEN SOMETHING GETS IT HERE!)

            // Adjust based on ShouldShowArmyOverlayForPlayer?

            // First check whether an army is already selected (it may be set during disband or army creation)
            if (ACArmyOverlayUIContext.Instance?.SelectedArmy != null)
            {
                __result = ACArmyOverlayUIContext.Instance.SelectedArmy;
            }
            else if (MobileParty.MainParty.Army?.Kingdom != null)
            {
                if (ACArmyOverlayUIContext.Instance != null)
                {
                    ACArmyOverlayUIContext.Instance.SelectedArmy = MobileParty.MainParty.Army;
                }
                __result = MobileParty.MainParty.Army;
            }
            else
            {

                Army army = Clan.PlayerClan.Kingdom.Armies.FirstOrDefault();

                if (ACArmyOverlayUIContext.Instance != null)
                {
                    ACArmyOverlayUIContext.Instance.SelectedArmy = army;
                }
                __result = army;
            }
            

            return false;
        }
    }


    // ========================================================================
    // 3. OnFinalize
    // ========================================================================

    [HarmonyPatch(typeof(ArmyMenuOverlayVM), "OnFinalize")]
    internal static class ArmyMenuOverlayVM_OnFinalize_Patch
    {
        private static void Postfix(ArmyMenuOverlayVM __instance)
        {
            ACArmyOverlayUIContext.Instance?.CurrentArmyOverlayVMMixIn.OnFinalize();
            ACArmyOverlayUIContext.Instance?.UnregisterInstance();
        }
    }

    // ========================================================================
    // 4. ExecuteOpenArmyManagement
    // ========================================================================

    [HarmonyPatch(typeof(ArmyMenuOverlayVM), "ExecuteOpenArmyManagement")]
    internal static class ArmyMenuOverlayVM_ExecuteOpenArmyManagement_Patch
    {
        private static bool Prefix(ArmyMenuOverlayVM __instance)
        {

            __instance.OpenArmyManagement();

            return false;
        }
    }

    // ========================================================================
    // 5. OnPartyAttachedAnotherParty
    // ========================================================================

    [HarmonyPatch(typeof(ArmyMenuOverlayVM), "OnPartyAttachedAnotherParty")]
    internal static class ArmyMenuOverlayVM_OnPartyAttachedAnotherParty_Patch
    {
        private static bool Prefix(ArmyMenuOverlayVM __instance, MobileParty party)
        {
            if (party != null &&
                party.AttachedTo != null &&
                party.AttachedTo.Army != null &&
                MobileParty.MainParty != null &&
                party.AttachedTo.Army == ArmyMenuOverlayVMAccess.GetArmyToUse(__instance))
            {
                ArmyMenuOverlayVMAccess.SetIsVisualsDirty(__instance, true);
            }

            return false;
        }
    }

    // ========================================================================
    // 6. GetIsPlayerArmyLeader
    // ========================================================================

    [HarmonyPatch(typeof(ArmyMenuOverlayVM), "GetIsPlayerArmyLeader", new Type[] { typeof(Army) })]
    internal static class ArmyMenuOverlayVM_GetIsPlayerArmyLeader_Patch
    {
        private static bool Prefix(Army army, ref bool __result)
        {
            __result = true;

            return false;
        }
    }
}
