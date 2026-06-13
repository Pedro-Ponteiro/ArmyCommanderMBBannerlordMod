using ArmyCommander.UIExtension.Context;
using HarmonyLib;
using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Chat;
// ajuste o namespace do ChatLogWidget conforme o decompiled



namespace ArmyCommander.HarmonyPatches
{
    internal static class ACChatLogWidgetController
    {
        private static WeakReference<ChatLogWidget> _chatLogWidgetRef;

        private static float _desiredMarginBottom;

        private static readonly int _defaultMarginBottom = 32;


        public static void Register(ChatLogWidget widget)
        {
            _chatLogWidgetRef = new WeakReference<ChatLogWidget>(widget);

            if (ACArmyOverlayUIContext.Instance != null)
            {
                UpdateDesiredMarginBottom(ACArmyOverlayUIContext.Instance.IsExtended, ACArmyOverlayUIContext.Instance.ArmiesCount);
            }
        }

        public static void UpdateDesiredMarginBottom(bool isLeftArmyOverlayExtended, int numberOfArmiesInLeftArmyOverlay)
        {

            int marginToAdd;
            if (!isLeftArmyOverlayExtended)
            {
                marginToAdd = 25;
            }
            else
            {
                // 163 max.
                marginToAdd = 25 + 69 * Math.Min(numberOfArmiesInLeftArmyOverlay, 2);
            }
            

            _desiredMarginBottom = _defaultMarginBottom + marginToAdd;

            ApplyMarginBottom();
        }

        private static void ApplyMarginBottom()
        {
            if (_chatLogWidgetRef == null)
            {
                return;
            }

            if (!_chatLogWidgetRef.TryGetTarget(out ChatLogWidget widget) || widget == null)
            {
                return;
            }

            widget.MarginBottom = _desiredMarginBottom;

             //widget.SetMeasureAndLayoutDirty();
        }
    }

    [HarmonyPatch(typeof(ChatLogWidget))]
    internal static class ChatLogWidget_Constructor_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, typeof(UIContext))]
        private static void Postfix(ChatLogWidget __instance)
        {
            ACChatLogWidgetController.Register(__instance);
        }
    }
}