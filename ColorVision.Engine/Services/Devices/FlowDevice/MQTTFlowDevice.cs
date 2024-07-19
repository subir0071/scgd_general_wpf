﻿using ColorVision.Engine.Services.Msg;
using CVCommCore;
using System.Collections.Generic;
using System.Windows;

namespace ColorVision.Engine.Services.Devices.FlowDevice
{
    public class MQTTFlowDevice : MQTTDeviceService<ConfigFlowDevice>
    {
        public MQTTFlowDevice(ConfigFlowDevice config) : base(config)
        {
            MsgReturnReceived += ProcessingReceived;
            DeviceStatus = DeviceStatusType.Closed;
            DisConnected += (s, e) =>
            {
                DeviceStatus = DeviceStatusType.Closed;
            };
        }

        private void ProcessingReceived(MsgReturn msg)
        {
            if (msg.DeviceCode != Config.Code) return;

        }


    }
}
