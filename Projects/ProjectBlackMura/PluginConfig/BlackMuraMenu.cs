﻿using ColorVision.UI.Menus;
using System.Windows;

namespace ProjectBase.PluginConfig
{
    public class BlackMuraMenu : MenuItemBase
    {
        public override string OwnerGuid => MenuItemConstants.Tool;

        public override int Order => 100;
        public override string Header => "BlackMura检测";

        public override void Execute()
        {
            if (ProjectWindowInstance.WindowInstance == null)
            {
                ProjectWindowInstance.WindowInstance = new MainWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                ProjectWindowInstance.WindowInstance.Closed += (s, e) => ProjectWindowInstance.WindowInstance = null;
                ProjectWindowInstance.WindowInstance.Show();
            }
            else
            {
                ProjectWindowInstance.WindowInstance.Activate();
            }
        }
    }
}
