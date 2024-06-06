﻿using ColorVision.Common.MVVM;
using ColorVision.Engine.Services.PhyCameras;
using System;
using System.Windows;
using System.Windows.Input;


namespace ColorVision.Engine.Services.Devices.Calibration
{
    /// <summary>
    /// EditCalibration.xaml 的交互逻辑
    /// </summary>
    public partial class EditCalibration : Window
    {
        public DeviceCalibration DeviceCalibration { get; set; }

        public MQTTCalibration Service { get => DeviceCalibration.DeviceService; }

        public ConfigCalibration EditConfig { get; set; }
        public EditCalibration(DeviceCalibration  deviceCalibration)
        {
            DeviceCalibration = deviceCalibration;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Common.NativeMethods.Keyboard.PressKey(0x09);
                e.Handled = true;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            DataContext = DeviceCalibration;

            CameraPhyID.ItemsSource = PhyCameraManager.GetInstance().PhyCameras;
            CameraPhyID.SelectedItem = PhyCameraManager.GetInstance().GetPhyCamera(DeviceCalibration.Config.CameraID);
            CameraPhyID.DisplayMemberPath = "Name";

            EditConfig = DeviceCalibration.Config.Clone();
            EditContent.DataContext = EditConfig;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DeviceCalibration.PhyCamera?.ReleaseCalibration();
            EditConfig.CopyTo(DeviceCalibration.Config);
            Close();
        }
    }
}
