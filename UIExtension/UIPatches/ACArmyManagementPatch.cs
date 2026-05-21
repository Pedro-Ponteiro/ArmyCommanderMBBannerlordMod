using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace ArmyCommander.UIExtension.Patches
{
    [PrefabExtension("ArmyManagement", @"//Window/Widget/Children/BrushWidget")]
    internal sealed class ArmyManagementPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type
        {
            get { return InsertType.Child; }
        }

        [PrefabExtensionFileName]
        public string PatchFileName => "ACArmyManagementWidgets";

    }
}