﻿using MQTTMessageLib.Algorithm;
using System.Collections.Generic;
using System.Windows.Documents;

namespace ColorVision.Engine.Services.Devices.Algorithm.Views
{
    public interface IResultHandle
    {
        public List<AlgorithmResultType> CanHandle { get; set; }

        void Handle(AlgorithmView view, AlgorithmResult result);
    }
}
