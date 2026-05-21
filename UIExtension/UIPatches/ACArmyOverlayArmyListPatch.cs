using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.IO;
using System.Xml;
using TaleWorlds.ModuleManager;

namespace ArmyCommander.UIExtension.Patches
{
    [PrefabExtension("ArmyOverlay", @"//Window")]
    internal sealed class ArmyOverlayWindowPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type
        {
            get { return InsertType.Replace; }
        }

        [PrefabExtensionXmlNode]
        public XmlNode GetPrefabExtension()
        {
            XmlDocument originalDoc = new XmlDocument();
            originalDoc.PreserveWhitespace = false;

            string sandboxPath = ModuleHelper.GetModuleFullPath("SandBox");

            string originalArmyOverlayPath = Path.Combine(
                sandboxPath,
                "GUI",
                "Prefabs",
                "Map",
                "ArmyOverlay.xml"
            );

            originalDoc.Load(originalArmyOverlayPath);

            XmlNode originalArmyOverlayWidget =
                originalDoc.SelectSingleNode("//ArmyOverlayWidget");

            if (originalArmyOverlayWidget == null)
            {
                throw new XmlException("ArmyOverlayWidget não encontrado no ArmyOverlay.xml original.");
            }

            XmlDocument patchDoc = new XmlDocument();
            patchDoc.PreserveWhitespace = false;

            string myModPath = GetCurrentModulePath();

            string myPatchPath = Path.Combine(
                myModPath,
                "GUI",
                "ArmyOverlayWindow.xml"
            );

            patchDoc.Load(myPatchPath);

            XmlNode placeholder =
                patchDoc.SelectSingleNode("//ArmyCommanderOriginalArmyOverlayWidgetPlaceholder");

            if (placeholder == null)
            {
                throw new XmlException("Placeholder não encontrado no ArmyOverlayWindow.xml.");
            }

            XmlNode importedOriginal =
                patchDoc.ImportNode(originalArmyOverlayWidget, true);

            placeholder.ParentNode.ReplaceChild(importedOriginal, placeholder);

            return patchDoc.DocumentElement;
        }

        private static string GetCurrentModulePath()
        {
            string dllDirectory =
                Path.GetDirectoryName(typeof(ArmyOverlayWindowPatch).Assembly.Location);

            return Path.GetFullPath(Path.Combine(dllDirectory, "..", ".."));
        }
    }
}