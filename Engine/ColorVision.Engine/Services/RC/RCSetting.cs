﻿using ColorVision.Common.MVVM;
using ColorVision.Common.Utilities;
using ColorVision.UI;
using ColorVision.UI.Configs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace ColorVision.Engine.Services.RC
{
    public class RCSettingProvider : IConfigSettingProvider,IStatusBarIconProvider
    {
        public IEnumerable<ConfigSettingMetadata> GetConfigSettings()
        {
            return new List<ConfigSettingMetadata>
            {
                new ConfigSettingMetadata
                {
                    Name = "启用RC",
                    Description = "启用RC",
                    Type = ConfigSettingType.Bool,
                    Order =10,
                    BindingName = nameof(RCSetting.IsUseRCService),
                    Source = RCSetting.Instance
                },
            };
        }

        public IEnumerable<StatusBarIconMetadata> GetStatusBarIconMetadata()
        {
            Action action = new Action(() =>
            {
                new RCServiceConnect() { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
            });

            return new List<StatusBarIconMetadata>
            {
                new StatusBarIconMetadata()
                {
                    Name = "RC",
                    Description = "RC",
                    Order =3,
                    BindingName = "MQTTRCService.IsConnect",
                    VisibilityBindingName =nameof(RCSetting.IsUseRCService),
                    ButtonStyleName ="ButtonDrawingImageRCService",
                    Source = RCSetting.Instance,
                    Action =action
                }
            };
        }
    }


    public class RCSetting : ViewModelBase, IConfigSecure
    {
        public static RCSetting Instance => ConfigService.Instance.GetRequiredService<RCSetting>();

        public static MqttRCService MQTTRCService => MqttRCService.GetInstance();

        public RCServiceConfig Config { get; set; } = new RCServiceConfig();
        public bool IsUseRCService { get => _IsUseRCService; set { _IsUseRCService = value; NotifyPropertyChanged(); } }
        private bool _IsUseRCService = true;

        public ObservableCollection<RCServiceConfig> RCServiceConfigs { get; set; } = new ObservableCollection<RCServiceConfig>();
        public const string ConfigAESKey = "ColorVision";
        public const string ConfigAESVector = "ColorVision";

        public void Decrypt()
        {
            ///如果是初始值直接跳过
            if (Config.AppSecret != "123456")
            {
                Config.AppSecret = Cryptography.AESDecrypt(Config.AppSecret, ConfigAESKey, ConfigAESVector);
            }
        }

        public void Encryption()
        {
            Config.AppSecret = Cryptography.AESEncrypt(Config.AppSecret, ConfigAESKey, ConfigAESVector);
        }
    }
}
