// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ColorVision.Media;
using ColorVision.NativeMethods;
using ColorVision.Net;
using ColorVision.Utils;
using log4net;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace ColorVision
{
    /// <summary>
    /// Main�����Ľ������ڳ���֮�У�Ϊ�˲�Ӱ��APP������������һ����
    /// </summary>
    public partial class App
    {
        private static Mutex mutex;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        const uint WM_USER = 0x0400; // �û��Զ�����Ϣ��ʼֵ

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort GlobalAddAtom(string lpString);

        public static string SolutionPath { get; set; } = string.Empty;

        static string[] Sysargs;
        [STAThread]
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            Sysargs = args;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            bool IsDebug = Debugger.IsAttached;
            if (args.Count() > 0)
            {
                for (int i = 0; i < args.Count(); i++)
                {
                    if (args[i].ToLower() == "-d" || args[i].ToLower() == "-debug")
                    {
                        IsDebug = true;
                    }
                    if (args[i].ToLower() == "-r" || args[i].ToLower() == "-restart")
                    {
                        App.IsReStart = true;
                    }
                    if (args[i].EndsWith("cvsln", StringComparison.OrdinalIgnoreCase))
                    {
                        SolutionPath = args[i];
                    }
                }
            }

            if (Environment.CurrentDirectory.Contains("C:\\Program Files"))
            {
                var fileAppender = (log4net.Appender.FileAppender)LogManager.GetRepository().GetAppenders().FirstOrDefault(a => a is log4net.Appender.FileAppender);
                if (fileAppender != null)
                {
                    fileAppender.File = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ColorVision\\Log";
                    fileAppender.ActivateOptions();
                }
            }

            mutex = new Mutex(true, "ElectronicNeedleTherapySystem", out bool ret);
            if (!ret)
            {
                IntPtr hWnd = CheckAppRunning.Check("ColorVision");
                if (hWnd != IntPtr.Zero)
                {
                    if (args.Length > 0)
                    {
                        ushort atom = GlobalAddAtom(args[0]);
                        SendMessage(hWnd, WM_USER + 1, (IntPtr)atom, IntPtr.Zero);  // ������Ϣ
                    }
                    log.Info("�����Ѿ���");
                    Environment.Exit(0);
                }
                ////д���������Avoid�����ж࿪��Ч��������û�а취���汾��ʵ��ͬ�汾������¸�������Ψһ
                //Environment.Exit(0);
            }

            log.Info("�����");
            App app;
            app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}