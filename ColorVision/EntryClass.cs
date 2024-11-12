// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ColorVision.Common.NativeMethods;
using ColorVision.UI.Shell;
using log4net;
using log4net.Config;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace ColorVision
{
    /// <summary>
    /// Main�����Ľ������ڳ���֮�У�Ϊ�˲�Ӱ��APP������������һ����
    /// </summary>
    public partial class App
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        private static Mutex mutex;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        const uint WM_USER = 0x0400; // �û��Զ�����Ϣ��ʼֵ

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort GlobalAddAtom(string lpString);

        [STAThread]
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            ArgumentParser.GetInstance().CommandLineArgs = args;
            log.Info("args��" + string.Join(", ", args));

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            if (Environment.CurrentDirectory.Contains("C:\\Program Files"))
            {
                var fileAppender = (log4net.Appender.FileAppender)LogManager.GetRepository().GetAppenders().FirstOrDefault(a => a is log4net.Appender.FileAppender);
                if (fileAppender != null)
                {
                    fileAppender.File = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ColorVision\\Log\\";
                    fileAppender.ActivateOptions();
                }
            }

            mutex = new Mutex(true, "ColorVision", out bool ret);
            if (!ret && !Debugger.IsAttached)
            {
                IntPtr hWnd = CheckAppRunning.Check("ColorVision");
                if (hWnd != IntPtr.Zero)
                {
                    if (args.Length > 0)  
                    {
                        char separator = '\u0001';
                        string combinedArgs = string.Join(separator.ToString(), args);
                        ushort atom = GlobalAddAtom(combinedArgs);
                        //���ﷴ�ˣ�����û��Ҫ���ˣ���һ��
                        SendMessage(hWnd, WM_USER + 1, IntPtr.Zero,(IntPtr)atom);  // ������Ϣ
                    }
                    log.Info("�����Ѿ���");
                    Environment.Exit(0);
                }
                ////д���������Avoid�����ж࿪��Ч��������û�а취���汾��ʵ��ͬ�汾������¸�������Ψһ
                //Environment.Exit(0);
            }
            //ɱ����ʬ����
            log.Info("�����");
            App app;
            app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static void KillZombieProcesses()
        {
            // ��ȡ��ǰ���̵����ƺ�ID
            Process currentProcess = Process.GetCurrentProcess();
            string processName = currentProcess.ProcessName;
            int currentProcessId = currentProcess.Id;

            // ��ȡ����ͬ������
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                // ������ǰ����
                if (process.Id == currentProcessId)
                    continue;
                
                try
                {
                    log.Info("��ֹδ��Ӧ�Ľ���");
                    // ��ֹδ��Ӧ�Ľ���
                    process.Kill();
                    process.WaitForExit();
                    log.Info($"����ֹδ��Ӧ�Ľ��̣�PID {process.Id}");
                }
                catch (Exception ex)
                {
                    // ������ܵ��쳣������Ȩ�޲���
                    log.Info($"�޷���ֹ���̣�PID {process.Id}������{ex.Message}");
                }
            }
        }
    }
}