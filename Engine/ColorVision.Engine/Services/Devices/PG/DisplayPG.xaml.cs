﻿using ColorVision.Themes.Controls;
using ColorVision.UI;
using CVCommCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ColorVision.Engine.Services.Devices.PG
{
    /// <summary>
    /// DisplayPG.xaml 的交互逻辑
    /// </summary>
    public partial class DisplayPG : UserControl, IDisPlayControl
    {
        private MQTTPG PGService { get => DevicePG.DService; }
        private DevicePG DevicePG { get; set; }
        public string DisPlayName => DevicePG.Config.Name;


        public DisplayPG(DevicePG devicePG)
        {
            DevicePG = devicePG;
            InitializeComponent();
            DataContext = DevicePG;
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            PGService.DeviceStatusChanged += (e) =>
            {
                switch (e)
                {
                    case DeviceStatusType.Unknown:
                        break;
                    case DeviceStatusType.OffLine:
                        break;
                    case DeviceStatusType.Closed:
                        btn_open.Content = "打开";
                        break;
                    case DeviceStatusType.Closing:
                        btn_open.Content = "关闭中";
                        break;
                    case DeviceStatusType.Opened:
                        btn_open.Content = "关闭";
                        break;
                    case DeviceStatusType.Opening:
                        btn_open.Content = "打开中";
                        break;
                    case DeviceStatusType.Busy:
                        break;
                    case DeviceStatusType.Free:
                        break;
                    case DeviceStatusType.LiveOpened:
                        break;
                    default:
                        break;
                }
            };


            void UpdateUI(DeviceStatusType status)
            {
                void SetVisibility(UIElement element, Visibility visibility) { if (element.Visibility != visibility) element.Visibility = visibility; };

                void HideAllButtons()
                {
                    SetVisibility(ButtonUnauthorized, Visibility.Collapsed);
                    SetVisibility(TextBlockUnknow, Visibility.Collapsed);
                    SetVisibility(StackPanelContent, Visibility.Collapsed);
                    SetVisibility(TextBlockOffLine, Visibility.Collapsed);
                }

                HideAllButtons();
                switch (status)
                {

                    case DeviceStatusType.Unknown:
                        SetVisibility(TextBlockUnknow, Visibility.Visible);
                        break;
                    case DeviceStatusType.Unauthorized:
                        SetVisibility(ButtonUnauthorized, Visibility.Visible);
                        break;
                    case DeviceStatusType.OffLine:
                        SetVisibility(TextBlockOffLine, Visibility.Visible);
                        break;
                    case DeviceStatusType.UnInit:
                        SetVisibility(StackPanelContent, Visibility.Visible);
                        break;
                    case DeviceStatusType.Closed:
                        SetVisibility(StackPanelContent, Visibility.Visible);
                        break;
                    case DeviceStatusType.Opened:
                        SetVisibility(StackPanelContent, Visibility.Visible);
                        break;
                    default:
                        break;
                }
            }


            UpdateUI(PGService.DeviceStatus);
            PGService.DeviceStatusChanged += UpdateUI;



            if (PGService.Config.IsNet)
            {
                TextBlockPGIP.Text = "IP地址";
                TextBlockPGPort.Text = "端口";
            }
            else
            {
                TextBlockPGIP.Text = "串口";
                TextBlockPGPort.Text = "波特率";
            }
            this.ApplyChangedSelectedColor(DisPlayBorder);
        }

        public event RoutedEventHandler Selected;
        public event RoutedEventHandler Unselected;
        public event EventHandler SelectChanged;
        private bool _IsSelected;
        public bool IsSelected { get => _IsSelected; set { _IsSelected = value; SelectChanged?.Invoke(this, new RoutedEventArgs()); if (value) Selected?.Invoke(this, new RoutedEventArgs()); else Unselected?.Invoke(this, new RoutedEventArgs()); } }


        private void DoOpen(Button button)
        {
            string btnTitle = button.Content.ToString();
            if (btnTitle != null && btnTitle.Equals("打开", StringComparison.Ordinal))
            {
                button.Content = "打开中";
                int port;
                if (!int.TryParse(TextBoxPGPort.Text, out port))
                {
                    MessageBox1.Show(Application.Current.MainWindow, "端口配置错误");
                    return;
                }
                if (PGService.Config.IsNet) PGService.Open(CommunicateType.Tcp, TextBoxPGIP.Text, port);
                else PGService.Open(CommunicateType.Serial, TextBoxPGIP.Text, port);
            }
            else
            {
                button.Content = "关闭中";
                PGService.Close();
            }
        }

        private void PGOpen(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) DoOpen(button);
        }

        private void PGStartPG(object sender, RoutedEventArgs e) => PGService.PGStartPG();

        private void PGStopPG(object sender, RoutedEventArgs e) => PGService.PGStopPG();

        private void PGReSetPG(object sender, RoutedEventArgs e) => PGService.PGReSetPG();
        private void PGSwitchUpPG(object sender, RoutedEventArgs e) => PGService.PGSwitchUpPG();
        private void PGSwitchDownPG(object sender, RoutedEventArgs e) => PGService.PGSwitchDownPG();

        private void PGSwitchFramePG(object sender, RoutedEventArgs e) => PGService.PGSwitchFramePG(int.Parse(PGFrameText.Text));

        private void PGSendCmd(object sender, RoutedEventArgs e)
        {
            PGService.CustomPG(PGCmdMsg.Text);
        }


    }
}
