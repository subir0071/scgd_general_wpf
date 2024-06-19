﻿using cvColorVision;
using ColorVision.Common.MVVM;
using System.Windows.Documents;
using System.Collections.Generic;

namespace ColorVision.Engine.Services.PhyCameras.Configs
{
    public class CFWPORT : ViewModelBase
    {
        public bool IsCOM { get => _IsCOM; set { _IsCOM = value; NotifyPropertyChanged(); } }
        private bool _IsCOM;

        public int CFWNum { get => _CFWNum; set {
                if (_CFWNum == value) return;
                if (value > 3)
                {
                    _CFWNum = 3;
                    NotifyPropertyChanged();
                    return;
                }
                _CFWNum = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsCFWNum1)); 
                NotifyPropertyChanged(nameof(IsCFWNum2));
                NotifyPropertyChanged(nameof(IsCFWNum3)); 
                if (value == 2)
                {
                    if (ChannelCfgs.Count == 3)
                    {
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                    }

                }
                if (value == 3)
                {
                    if (ChannelCfgs.Count == 3)
                    {
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                    }
                    if (ChannelCfgs.Count == 6)
                    {
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                        ChannelCfgs.Add(new ChannelCfg());
                    }
                }
            } }
        private int _CFWNum = 1;

        public bool IsCFWNum1 => CFWNum == 1;
        public bool IsCFWNum2 => CFWNum >= 2;
        public bool IsCFWNum3 => CFWNum >= 3;


        public string SzComName { get => _szComName; set { _szComName = value; NotifyPropertyChanged(); } }
        private string _szComName = "COM1";

        public int BaudRate { get => _BaudRate; set { _BaudRate = value; NotifyPropertyChanged(); } }
        private int _BaudRate = 115200;

        public List<ChannelCfg> ChannelCfgs { get; set; } = new List<ChannelCfg>{
            new() { Cfwport =0,Chtype =ImageChannelType.Gray_Y }, new() {Cfwport =1,Chtype =ImageChannelType.Gray_X }, new(){ Cfwport =2,Chtype =ImageChannelType.Gray_Z}
        };
    }
}