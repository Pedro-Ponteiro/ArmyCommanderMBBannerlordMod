using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.IO;
using System.Xml;
using TaleWorlds.ModuleManager;

namespace ArmyCommander.UIExtension.Patches
{
    [PrefabExtension(
        "ArmyManagementRightPanel",
        "//ButtonWidget[@Id='DisbandButton']"
    )]
    internal sealed class ACArmyManagementRightPanelDisbandButtonPatch : PrefabExtensionInsertPatch
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

            string originalRightPanelPath = Path.Combine(
                sandboxPath,
                "GUI",
                "Prefabs",
                "GatherArmy",
                "ArmyManagementRightPanel.xml"
            );

            originalDoc.Load(originalRightPanelPath);

            XmlNode originalDisbandButton =
                originalDoc.SelectSingleNode("//ButtonWidget[@Id='DisbandButton']");

            if (originalDisbandButton == null)
            {
                throw new XmlException(
                    "ButtonWidget com Id='DisbandButton' não encontrado no ArmyManagementRightPanel.xml original."
                );
            }

            XmlDocument patchDoc = new XmlDocument();
            patchDoc.PreserveWhitespace = false;

            string myModPath = GetCurrentModulePath();

            string myPatchPath = Path.Combine(
                myModPath,
                "GUI",
                "ArmyManagementRightPanelDisbandButtonWrapper.xml"
            );

            patchDoc.Load(myPatchPath);

            XmlNode placeholder =
                patchDoc.SelectSingleNode("//ArmyCommanderOriginalDisbandButtonPlaceholder");

            if (placeholder == null)
            {
                throw new XmlException(
                    "Placeholder não encontrado no ArmyManagementRightPanelDisbandButtonWrapper.xml."
                );
            }

            XmlNode importedOriginalDisbandButton =
                patchDoc.ImportNode(originalDisbandButton, true);

            XmlElement importedOriginalDisbandButtonElement =
                importedOriginalDisbandButton as XmlElement;

            if (importedOriginalDisbandButtonElement == null)
            {
                throw new XmlException(
                    "O DisbandButton importado não é um XmlElement."
                );
            }

            importedOriginalDisbandButtonElement.SetAttribute(
                "HorizontalAlignment",
                "Right"
            );

            placeholder.ParentNode.ReplaceChild(
                importedOriginalDisbandButton,
                placeholder
            );

            return patchDoc.DocumentElement;
        }

        private static string GetCurrentModulePath()
        {
            string dllDirectory =
                Path.GetDirectoryName(
                    typeof(ACArmyManagementRightPanelDisbandButtonPatch)
                        .Assembly
                        .Location
                );

            return Path.GetFullPath(Path.Combine(dllDirectory, "..", ".."));
        }
    }
}