﻿using ColorVision.Themes.Controls;
using ColorVision.UI;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ColorVision.Engine.Services.Devices.Calibration
{
    /// <summary>
    /// InfoSMU.xaml 的交互逻辑
    /// </summary>
    public partial class InfoCalibration : UserControl
    {
        public DeviceCalibration Device { get; set; }

        public MQTTCalibration DService { get => Device.DService; }
        public InfoCalibration(DeviceCalibration deviceCalibration)
        {
            Device = deviceCalibration;
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            DataContext = Device;
            PropertyEditorHelper.GenCommand(Device, CommandGrid);
        }


        private void ServiceCache_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox1.Show(Application.Current.GetActiveWindow(), "文件删除后不可找回", "ColorVision", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var MsgRecord = DService.CacheClear();
                MsgRecord.MsgRecordStateChanged += (s) =>
                {
                    MessageBox1.Show(Application.Current.GetActiveWindow(), "文件服务清理完成", "ColorVison");
                    MsgRecord.ClearMsgRecordSucessChangedHandler();
                };
            }
        }
    }
}
