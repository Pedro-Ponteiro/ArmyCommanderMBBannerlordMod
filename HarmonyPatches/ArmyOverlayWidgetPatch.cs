using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Menu.Overlay;

namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch(typeof(ArmyOverlayWidget), "OnArmyListPageCountChanged")]
    internal static class ArmyOverlayWidget_OnArmyListPageCountChanged_Patch
    {
        private static bool Prefix(ArmyOverlayWidget __instance)
        {
            if (__instance.Overlay.Id != "ACOverlayWidget")
            {
                return true;
            }
            else
            {
                __instance.Overlay.PositionXOffset = 40f;
                __instance.ExtendButton.PositionXOffset = 0;


                ACArmyOverlayUIContext.Instance.ShouldShowNextPageBtn = __instance.PageControlWidget.PageCount > 1;

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(ArmyOverlayWidget))]
    internal static class ArmyOverlayWidget_OnExtendButtonClick_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnExtendButtonClick")]
        private static void Postfix(
            ArmyOverlayWidget __instance,
            bool ____isOverlayExtended)
        {
            bool isOverlayExtended = ____isOverlayExtended;

            if (__instance.Overlay.Id != "ACOverlayWidget")
            {
                return;
            }

            ACArmyOverlayUIContext.Instance.IsExtended = isOverlayExtended;
        }
    }
}