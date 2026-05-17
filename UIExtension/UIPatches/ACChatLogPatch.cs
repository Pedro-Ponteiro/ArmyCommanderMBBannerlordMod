using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;


namespace ArmyCommander.UIExtension.Patches
{
    [PrefabExtension("SPChatLog", "//ChatLogWidget")]
    internal class SPChatLog_ChatLogWidget_MarginBottom_Patch : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes
        {
            get
            {
                return new List<Attribute>
                {
                    new Attribute("MarginBottom", "195")
                };
            }
        }
    }
}