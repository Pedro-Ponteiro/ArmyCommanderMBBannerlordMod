using HarmonyLib;
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
                return false;
            }
        }
    }
}