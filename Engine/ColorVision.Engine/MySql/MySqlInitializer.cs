﻿using ColorVision.UI;
using System.Threading.Tasks;
using System.Windows;

namespace ColorVision.Engine.MySql
{

    public class MySqlInitializer : InitializerBase
    {
        private readonly IMessageUpdater _messageUpdater;

        public MySqlInitializer(IMessageUpdater messageUpdater)
        {
            _messageUpdater = messageUpdater;
        }
        public override string Name => nameof(MySqlInitializer);
        public override int Order => 1;

        public override async Task InitializeAsync()
        {
            if (MySqlSetting.Instance.IsUseMySql)
            {
                _messageUpdater.Update("正在检测MySql数据库连接情况");

                bool isConnect = await MySqlControl.GetInstance().Connect();

                _messageUpdater.Update($"MySql数据库连接{(MySqlControl.GetInstance().IsConnect ? Properties.Resources.Success : Properties.Resources.Failure)}");
                if (!isConnect)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MySqlConnect mySqlConnect = new() { Owner = Application.Current.MainWindow };
                        mySqlConnect.ShowDialog();
                    });
                }
            }
            else
            {
                _messageUpdater.Update("已经跳过数据库连接");
            }
        }
    }
}
