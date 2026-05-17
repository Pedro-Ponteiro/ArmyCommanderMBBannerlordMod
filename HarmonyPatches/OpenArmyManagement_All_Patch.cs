using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

using SandBox.GauntletUI.Map;
using SandBox.GauntletUI;


namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch]
    internal static class OpenArmyManagement_All_Patch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GauntletMapBarGlobalLayer), "OpenArmyManagement", Type.EmptyTypes);
            yield return AccessTools.Method(typeof(GauntletMapOverlayView), "OpenArmyManagement", Type.EmptyTypes);
            yield return AccessTools.Method(typeof(GauntletKingdomScreen), "OpenArmyManagement", Type.EmptyTypes);
        }

        private static void Postfix()
        {

            ACArmyManagementUIContext.Instance.movieIsLoaded = true;
            ACArmyManagementUIContext.Instance.CurrentArmyManagementVMMixIn.UpdateWidgets();
            
        }
    }
}