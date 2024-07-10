﻿using ColorVision.Common.Utilities;
using ColorVision.Engine.Services.Msg;
using Panuon.WPF.UI;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ColorVision.Engine.Services
{

    internal static partial class ServicesHelper
    {

        public static async void SelectAndFocusFirstNode(TreeView treeView)
        {
            await Task.Delay(100);
            if (treeView.Items.Count > 0)
            {
                if (treeView.SelectedItem == null && treeView.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem firstNode)
                {
                    firstNode.IsSelected = true;
                    Application.Current.Dispatcher.Invoke(() => firstNode.Focus());
                }
            }
        }
        public static IPendingHandler SendCommand(MsgRecord msgRecord, string Msg,bool canCancel =true)
        {
            IPendingHandler handler = PendingBox.Show(Application.Current.MainWindow, Msg, canCancel);
            var temp = Application.Current.MainWindow.Cursor;
            Application.Current.MainWindow.Cursor = Cursors.Wait;
            MsgRecordStateChangedHandler msgRecordStateChangedHandler;
            msgRecordStateChangedHandler = (e) =>
            {
                try
                {
                    handler?.UpdateMessage(e.ToDescription());
                    if (e != MsgRecordState.Sended)
                    {
                        handler?.Close();
                    }
                }
                catch
                {
                }
                finally
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.MainWindow.Cursor = temp;
                    });
                }



            };
            msgRecord.MsgRecordStateChanged += msgRecordStateChangedHandler;
            handler.Cancelling += delegate
            {
                msgRecord.MsgRecordStateChanged -= msgRecordStateChangedHandler;
                handler.Close();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.MainWindow.Cursor = temp;
                });
            };
            return handler;
        }

        public static MsgRecord? SendCommandEx(object sender, Func<MsgRecord> action)
        {
            if (sender is Button button)
            {
                if (button.Content.ToString() == (Properties.Resources.ResourceManager.GetString(MsgRecordState.Sended.ToDescription(), CultureInfo.CurrentUICulture) ?? ""))
                {
                    MessageBox.Show(Application.Current.GetActiveWindow(), "已经发送,请耐心等待","ColorVison");
                    return null;
                }
                MsgRecord msgRecord = action.Invoke();
                SendCommand(button, msgRecord);
                return msgRecord;
            }
            return null;
        }



        public static void SendCommand(Button button, MsgRecord msgRecord, bool Reserve = true)
        {
            var temp = button.Content;
            button.Content = Properties.Resources.ResourceManager.GetString(msgRecord.MsgRecordState.ToDescription(), CultureInfo.CurrentUICulture) ?? "";

            MsgRecordStateChangedHandler msgRecordStateChangedHandler = null;
            msgRecordStateChangedHandler = async (e) =>
            {
                button.Content = Properties.Resources.ResourceManager.GetString(e.ToDescription(), CultureInfo.CurrentUICulture) ?? "";
                await Task.Delay(100);
                if (e != MsgRecordState.Sended)
                {
                    button.Content = temp;
                }
                msgRecord.MsgRecordStateChanged -= msgRecordStateChangedHandler;
            };
            if (Reserve)
                msgRecord.MsgRecordStateChanged += msgRecordStateChangedHandler;
        }

        public static bool IsInvalidPath(string Path, string Hint = "名称")
        {
            if (string.IsNullOrEmpty(Path))
            {
                MessageBox.Show(WindowHelpers.GetActiveWindow(),$"{Hint}不能为空", "ColorVision");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Path))
            {
                MessageBox.Show(WindowHelpers.GetActiveWindow(), $"{Hint}不能为空白", "ColorVision");
                return false;
            }
            if (Path.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(WindowHelpers.GetActiveWindow(), $"{Hint}不能包含特殊字符", "ColorVision");
                return false;
            }
            return true;
        }

    }
}
