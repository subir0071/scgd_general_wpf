﻿using ColorVision.Engine.Templates.Jsons;
using ColorVision.Engine.Templates.Validate;
using ColorVision.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorVision.Engine.Templates.BuzProduct
{
    public class EditTemplateBuzProductConfig : IConfig
    {
        public static EditTemplateBuzProductConfig Instance => ConfigService.Instance.GetRequiredService<EditTemplateBuzProductConfig>();

        public double Width { get; set; } = double.NaN;
    }

    /// <summary>
    /// EditTemplateBuzProduct.xaml 的交互逻辑
    /// </summary>
    public partial class EditTemplateBuzProduct : UserControl, ITemplateUserControl
    {
        public EditTemplateBuzProduct()
        {
            InitializeComponent();
            this.Width = EditTemplateJsonConfig.Instance.Width;
            this.SizeChanged += (s, e) =>
            {
                EditTemplateJsonConfig.Instance.Width = this.ActualWidth;
            };
        }
        public void SetParam(object param)
        {
            this.DataContext = param;
        }

        private void ComboBoxValidateCIE_Initialized(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.ItemsSource = TemplateComplyParam.CIEParams.SelectMany(kvp => kvp.Value) .ToList();
            }
        }

        private void GridViewColumnSort(object sender, RoutedEventArgs e)
        {

        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }
    }
}
