﻿using ColorVision.Services.Device.Camera.Configs;
using cvColorVision;
using Newtonsoft.Json;

namespace ColorVision.Services.Device.Motor
{
    public class ConfigMotor: DeviceServiceConfig
    {
        public FOCUS_COMMUN eFOCUSCOMMUN { get => _eFOCUSCOMMUN; set { _eFOCUSCOMMUN = value; NotifyPropertyChanged(); } }
        private FOCUS_COMMUN _eFOCUSCOMMUN;

        public string SzComName { get => _szComName; set { _szComName = value; NotifyPropertyChanged(); } }
        private string _szComName = "COM1";

        public int BaudRate { get => _BaudRate; set { _BaudRate = value; NotifyPropertyChanged(); } }
        private int _BaudRate = 115200;

        public AutoFocusConfig AutoFocusConfig { get; set; } = new AutoFocusConfig();

        [JsonIgnore]
        public int Position { get => _Position;set { _Position = value; NotifyPropertyChanged(); } }
        private int _Position;

        public int dwTimeOut { get => _dwTimeOut; set { _dwTimeOut = value; NotifyPropertyChanged(); } }
        private int _dwTimeOut = 5000;
    }
}
