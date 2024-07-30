﻿using ColorVision.Engine.Services.Devices.PG;
using ColorVision.UI.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ColorVision.Engine.Services.Devices.Sensor
{
    /// <summary>
    /// InfoPG.xaml 的交互逻辑
    /// </summary>
    public partial class InfoSensor : UserControl
    {
        public DeviceSensor DeviceSensor { get; set; }
        public InfoSensor(DeviceSensor deviceSensor)
        {
            DeviceSensor = deviceSensor;
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            DataContext = DeviceSensor;
        }

        private void UniformGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is UniformGrid uniformGrid)
                uniformGrid.AutoUpdateLayout();

        }
    }
}
