using ArmyCommander.Helpers;
using HarmonyLib;
using SandBox.View.Map;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;


namespace ArmyCommander.HarmonyPatches
{
    [HarmonyPatch]
    internal static class MapScreen_OnRefreshState_Patch
    {
        private static readonly FieldInfo ArmyOverlayField =
            AccessTools.Field(typeof(MapScreen), "_armyOverlay");

        private static readonly FieldInfo MapViewsContainerField =
            AccessTools.Field(typeof(MapScreen), "_mapViewsContainer");

        private static readonly MethodInfo AddArmyOverlayMethod =
            AccessTools.DeclaredMethod(
                typeof(MapScreen),
                "AddArmyOverlay",
                new Type[] { typeof(MapScreen.MapOverlayType) }
            );

        private static readonly MethodInfo OnArmyLeftMethod =
            AccessTools.DeclaredMethod(typeof(MapView), "OnArmyLeft", Type.EmptyTypes);

        private static readonly MethodInfo OnDispersePlayerLeadedArmyMethod =
            AccessTools.DeclaredMethod(typeof(MapView), "OnDispersePlayerLeadedArmy", Type.EmptyTypes);

        private static MethodBase TargetMethod()
        {
            InterfaceMapping map = typeof(MapScreen).GetInterfaceMap(typeof(IMapStateHandler));

            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.InterfaceMethods[i].Name == nameof(IMapStateHandler.OnRefreshState))
                {
                    return map.TargetMethods[i];
                }
            }

            throw new Exception("IMapStateHandler.OnRefreshState não encontrado em MapScreen.");
        }

        [HarmonyPrefix]
        private static bool Prefix(MapScreen __instance)
        {
            RefreshArmyOverlay(__instance);

            return false;
        }

        public static void RefreshArmyOverlay(MapScreen mapScreen = null)
        {
            mapScreen = mapScreen ?? MapScreen.Instance;

            if (mapScreen == null)
            {
                return;
            }

            if (!(Game.Current.GameStateManager.ActiveState is MapState))
            {
                return;
            }

            bool shouldShowArmyOverlay = ACHelpers.ShouldShowArmyOverlayForPlayer();

            MapView armyOverlay = (MapView)ArmyOverlayField.GetValue(mapScreen);

            if (shouldShowArmyOverlay && armyOverlay == null)
            {
                AddArmyOverlayMethod.Invoke(
                    mapScreen,
                    new object[] { MapScreen.MapOverlayType.Army }
                );
            }
            else if (!shouldShowArmyOverlay && armyOverlay != null)
            {
                MapViewsContainer mapViewsContainer =
                    (MapViewsContainer)MapViewsContainerField.GetValue(mapScreen);

                mapViewsContainer.ForeachReverse(delegate (MapView view)
                {
                    OnArmyLeftMethod.Invoke(view, null);
                });

                mapViewsContainer.ForeachReverse(delegate (MapView view)
                {
                    OnDispersePlayerLeadedArmyMethod.Invoke(view, null);
                });
            }
        }

    
    }
}