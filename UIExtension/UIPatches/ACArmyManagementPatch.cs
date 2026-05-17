using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace ArmyCommander.UIExtension.Patches
{
    [PrefabExtension("ArmyManagement", @"//Window")]
    internal sealed class ArmyManagementPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type
        {
            get { return InsertType.Replace; }
        }

        [PrefabExtensionFileName]
        public string PatchFileName => "ArmyManagementWindow";

    }
}