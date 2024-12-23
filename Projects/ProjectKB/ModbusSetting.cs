﻿using ColorVision.Common.MVVM;
using ColorVision.Common.Utilities;
using ColorVision.UI;
using System.Collections.ObjectModel;

namespace ProjectKB
{
    public class ModbusSetting : ViewModelBase , IConfigSecure
    {
        public static ModbusSetting Instance  => ConfigService.Instance.GetRequiredService<ModbusSetting>();

        public bool IsUseMySql { get => _IsUseMySql; set { _IsUseMySql = value; NotifyPropertyChanged(); } }
        private bool _IsUseMySql = true;
        /// <summary>
        /// MySql配置
        /// </summary>
        public ModbusConfig ModbusConfig { get; set; } = new ModbusConfig();
        public ObservableCollection<ModbusConfig> ModbusConfigs { get; set; } = new ObservableCollection<ModbusConfig>();


        public const string ConfigAESKey = "ColorVision";
        public const string ConfigAESVector = "ColorVision";

        public void Encryption()
        {
            ModbusConfig.UserPwd = Cryptography.AESEncrypt(ModbusConfig.UserPwd, ConfigAESKey, ConfigAESVector);
            foreach (var item in ModbusConfigs)
            {
                item.UserPwd = Cryptography.AESEncrypt(item.UserPwd, ConfigAESKey, ConfigAESVector);
            }
        }

        public void Decrypt()
        {
            ModbusConfig.UserPwd = Cryptography.AESDecrypt(ModbusConfig.UserPwd, ConfigAESKey, ConfigAESVector);
            foreach (var item in ModbusConfigs)
            {
                item.UserPwd = Cryptography.AESDecrypt(item.UserPwd, ConfigAESKey, ConfigAESVector);
            }
        }
    }
}
