﻿using System.Windows.Controls;
namespace ColorVision.UI.Configs
{
    public class ConfigSettingMetadata
    {
        public string Name { get; set; }
        /// <summary>
        /// 描述项，还是要看实现
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 如果需要变更顺序，可以通过Order来控制
        /// </summary>
        public int Order { get; set; } = 999;

        public string Group { get; set; } = "Deafult";

        public ConfigSettingType Type { get; set; }

        /// <summary>
        /// Bool
        /// </summary>
        public string BindingName { get; set; }
        public object Source { get; set; }
        /// <summary>
        /// UserControl,TabItem
        /// </summary>
        public UserControl UserControl { get; set; }
        
        /// <summary>
        /// ComboBox
        /// </summary>
        public ComboBox ComboBox { get; set; }
    }

    public interface IConfigSettingProvider
    {
        IEnumerable<ConfigSettingMetadata> GetConfigSettings();
    }
}
