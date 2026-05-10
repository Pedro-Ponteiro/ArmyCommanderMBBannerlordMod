using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace ArmyCommander.UIExtension.Patches
{
    [PrefabExtension("ArmyOverlay", @"//Window")]
    internal sealed class ArmiesPanelAddMarginBottom : PrefabExtensionInsertPatch
    {
        public override InsertType Type
        {
            get { return InsertType.Replace; }
        }

        [PrefabExtensionFileName]
        public string PatchFileName => "ArmyOverlayWindow";

    }
}