﻿using ColorVision.Engine.Properties;
using ColorVision.UI.Menus;

namespace ColorVision.Engine.Templates.Menus
{
    public class MenuTemplate : MenuItemBase
    {
        public override string Header => Resources.MenuTemplate;
        public override int Order => 2;
    }

}