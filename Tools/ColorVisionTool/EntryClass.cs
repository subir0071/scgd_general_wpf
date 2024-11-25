// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ColorVision.UI.Shell;
using log4net;
using log4net.Config;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace ColorVisionTool
{
    /// <summary>
    /// Main�����Ľ������ڳ���֮�У�Ϊ�˲�Ӱ��APP������������һ����
    /// </summary>
    public partial class App
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        private static Mutex mutex;

        [STAThread]
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            ArgumentParser.GetInstance().Parse(args);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            mutex = new Mutex(true, "ColorVision", out bool ret);

            log.Info("�����");
            App app;
            app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}