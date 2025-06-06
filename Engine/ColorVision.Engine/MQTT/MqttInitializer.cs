﻿using ColorVision.Common.Utilities;
using ColorVision.UI;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;

namespace ColorVision.Engine.MQTT
{
    public class MqttInitializer : InitializerBase
    {
        private readonly IMessageUpdater _messageUpdater;

        public MqttInitializer(IMessageUpdater messageUpdater)
        {
            _messageUpdater = messageUpdater;
        }
        public override string Name => nameof(MqttInitializer);
        public override int Order => 2;
        public override async Task InitializeAsync()
        {
            if (!MQTTSetting.Instance.IsUseMQTT)
            {
                _messageUpdater.Update("已经跳过MQTT服务器连接");
                return;
            }
            _messageUpdater.Update("正在检测MQTT服务器连接情况");


            bool isConnect = await MQTTControl.GetInstance().Connect();
            _messageUpdater.Update($"MQTT服务器连接{(MQTTControl.GetInstance().IsConnect ? Properties.Resources.Success : Properties.Resources.Failure)}");
            if (isConnect) return;

            if (MQTTControl.Config.Host == "127.0.0.1" || MQTTControl.Config.Host == "localhost")
            {
                _messageUpdater.Update("检测到配置本机服务，正在尝试查找本机服务mosquitto");
                try
                {
                    ServiceController serviceController = new ServiceController("Mosquitto Broker");
                    try
                    {
                        var status = serviceController.Status; // 会抛异常: 服务不存在
                        _messageUpdater.Update($"检测服务mosquitto，状态 {status}，正在尝试启动服务");

                        if (status == ServiceControllerStatus.Stopped || status == ServiceControllerStatus.Paused)
                        {
                            if (Tool.IsAdministrator())
                            {
                                serviceController.Start();
                            }
                            else
                            {
                                if (!Tool.ExecuteCommandAsAdmin("net start mosquitto"))
                                {
                                    _messageUpdater.Update("以管理员权限启动 mosquitto 服务失败。");
                                    return;
                                }
                            }
                        }
                        else if (status == ServiceControllerStatus.Running)
                        {
                            _messageUpdater.Update("mosquitto 服务已在运行。");
                        }

                        isConnect = await MQTTControl.GetInstance().Connect();
                        if (isConnect) return;
                    }
                    catch (InvalidOperationException)
                    {
                        _messageUpdater.Update("未检测到 Mosquitto Broker 服务，请确认已正确安装。");
                    }
                }
                catch(Exception ex)
                {
                    _messageUpdater.Update(ex.Message);
                }
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                MQTTConnect mQTTConnect = new() { Owner = Application.Current.GetActiveWindow() };
                mQTTConnect.ShowDialog();
            });
        }
    }
    }
