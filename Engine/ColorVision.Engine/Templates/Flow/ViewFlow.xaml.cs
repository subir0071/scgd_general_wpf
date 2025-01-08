﻿#pragma warning disable CA1720
using ColorVision.Common.MVVM;
using ColorVision.Engine.Templates;
using ColorVision.Engine.Templates.Flow;
using ColorVision.UI.Views;
using ST.Library.UI.NodeEditor;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ColorVision.Engine.Services.Flow
{
    /// <summary>
    /// CVFlowView.xaml 的交互逻辑
    /// </summary>
    public partial class ViewFlow : UserControl,IView, INotifyPropertyChanged,IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public FlowEngineLib.FlowEngineControl FlowEngineControl { get; set; }
        public View View { get; set; }
        public ObservableCollection<FlowRecord> FlowRecords { get; set; } = new ObservableCollection<FlowRecord>();

        public RelayCommand AutoSizeCommand { get; set; }

        public event EventHandler RefreshFlow;
        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand ClearCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        public RelayCommand AutoAlignmentCommand { get; set; }

        public RelayCommand OpenFlowTemplateCommand { get; set; }

        public static FlowConfig FlowConfig => FlowConfig.Instance;

        public bool IsEditMode { get => _IsEditMode; set { _IsEditMode = value; NotifyPropertyChanged(); } }
        private bool _IsEditMode = true;

        public ViewFlow()
        {
            FlowEngineControl = new FlowEngineLib.FlowEngineControl(false);
            InitializeComponent();
            AutoSizeCommand = new RelayCommand(a => AutoSize());
            RefreshCommand = new RelayCommand(a => Refresh());
            ClearCommand = new RelayCommand(a => Clear());
            SaveCommand = new RelayCommand(a => Save());
            AutoAlignmentCommand = new RelayCommand(a => AutoAlignment());
            OpenFlowTemplateCommand = new RelayCommand(a => OpenFlowTemplate());
        }

        public void OpenFlowTemplate()
        {
            new TemplateEditorWindow(new TemplateFlow()) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog(); ;
            Refresh();
        }

        public void AutoAlignment()
        {
            STNodeEditorHelper.ApplyTreeLayout(startX: 100, startY: 100, horizontalSpacing: 250, verticalSpacing: 200);
            STNodeEditorHelper.AutoSize();
        }

        public void Save()
        {
            FlowParam.DataBase64 = Convert.ToBase64String(STNodeEditorMain.GetCanvasData());
            FlowParam.Save();
            MessageBox.Show("保存成功");
        }

        public void Refresh()
        {
            RefreshFlow?.Invoke(this, new EventArgs());
        }
        public void Clear()
        {
            STNodeEditorMain.Nodes.Clear();
        }

        public STNodeEditorHelper STNodeEditorHelper { get; set; }

        public FlowParam FlowParam { get; set; }

        public float CanvasScale { get => STNodeEditorHelper.CanvasScale; set { STNodeEditorMain.ScaleCanvas(value, STNodeEditorMain.CanvasValidBounds.X + STNodeEditorMain.CanvasValidBounds.Width / 2, STNodeEditorMain.CanvasValidBounds.Y + STNodeEditorMain.CanvasValidBounds.Height / 2); NotifyPropertyChanged(); } }

        STNodeTreeView STNodeTreeView1 = new STNodeTreeView();
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = this;
            listViewRecord.ItemsSource = FlowRecords;
            STNodeEditorMain.LoadAssembly("FlowEngineLib.dll");
            STNodeTreeView1.LoadAssembly("FlowEngineLib.dll");


            STNodeEditorMain.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Delete)
                {
                    if (STNodeEditorMain.ActiveNode != null)
                        STNodeEditorMain.Nodes.Remove(STNodeEditorMain.ActiveNode);

                    foreach (var item in STNodeEditorMain.GetSelectedNode())
                    {
                        STNodeEditorMain.Nodes.Remove(item);
                    }
                }
                if (e.KeyCode == System.Windows.Forms.Keys.S  && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    Save();
                }
                if (e.KeyCode == System.Windows.Forms.Keys.R && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    Refresh();
                }
                if (e.KeyCode == System.Windows.Forms.Keys.L && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    AutoAlignment();
                }
            };

            FlowEngineControl.AttachNodeEditor(STNodeEditorMain);

            View = new View();
            ViewGridManager.GetInstance().AddView(0, this);
            View.ViewIndexChangedEvent += (s, e) =>
            {
                if (e == -2)
                {
                    STNodeEditorMain.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                    STNodeEditorMain.ContextMenuStrip.Items.Add("还原到主窗口中", null, (s, e1) =>
                    {

                        if (ViewGridManager.GetInstance().IsGridEmpty(View.PreViewIndex))
                        {
                            View.ViewIndex = View.PreViewIndex;
                        }
                        else
                        {
                            View.ViewIndex = -1;
                        }
                    }
                    );
                }
            };
            STNodeEditorHelper = new STNodeEditorHelper(STNodeEditorMain, STNodeTreeView1, STNodePropertyGrid1, SignStackPannel);
        }

        public void AutoSize()
        {
            STNodeEditorHelper.AutoSize();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth > 200)
            {
                winf1.Height = (int)ActualHeight;
                winf1.Width = (int)ActualWidth;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                STNodeEditorMain.MoveCanvas(STNodeEditorMain.CanvasOffsetX + 100, STNodeEditorMain.CanvasOffsetY, bAnimation: true, CanvasMoveArgs.Left);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                STNodeEditorMain.MoveCanvas(STNodeEditorMain.CanvasOffsetX - 100, STNodeEditorMain.CanvasOffsetY, bAnimation: true, CanvasMoveArgs.Left);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                STNodeEditorMain.MoveCanvas(STNodeEditorMain.CanvasOffsetX, STNodeEditorMain.CanvasOffsetY + 100, bAnimation: true, CanvasMoveArgs.Top);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                STNodeEditorMain.MoveCanvas(STNodeEditorMain.CanvasOffsetX, STNodeEditorMain.CanvasOffsetY - 100, bAnimation: true, CanvasMoveArgs.Top);
                e.Handled = true;
            }
            else if (e.Key == Key.Add)
            {
                STNodeEditorMain.ScaleCanvas(STNodeEditorMain.CanvasScale + 0.1f, (STNodeEditorMain.Width / 2), (STNodeEditorMain.Height / 2));
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract)
            {
                STNodeEditorMain.ScaleCanvas(STNodeEditorMain.CanvasScale - 0.1f, (STNodeEditorMain.Width / 2), (STNodeEditorMain.Height / 2));
                e.Handled = true;
            }
        }

        private bool IsMouseDown;
        private System.Drawing.Point lastMousePosition;
        private void STNodeEditorMain_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            System.Drawing.PointF m_pt_down_in_canvas = new System.Drawing.PointF();
            m_pt_down_in_canvas.X = ((float)e.X - STNodeEditorMain.CanvasOffsetX) / STNodeEditorMain.CanvasScale;
            m_pt_down_in_canvas.Y = ((float)e.Y - STNodeEditorMain.CanvasOffsetY) / STNodeEditorMain.CanvasScale;
            NodeFindInfo nodeFindInfo = STNodeEditorMain.FindNodeFromPoint(m_pt_down_in_canvas);

            if (!string.IsNullOrEmpty(nodeFindInfo.Mark))
            {

            }
            else if (nodeFindInfo.Node != null)
            {

            }
            else if (nodeFindInfo.NodeOption != null)
            {

            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IsMouseDown = true;
            }
        }

        private void STNodeEditorMain_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            IsMouseDown = false;
        }

        private void STNodeEditorMain_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((!IsEditMode|| Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) && IsMouseDown)
            {        // 计算鼠标移动的距离
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;

                // 更新画布偏移
                STNodeEditorMain.MoveCanvas(
                    STNodeEditorMain.CanvasOffsetX + deltaX,
                    STNodeEditorMain.CanvasOffsetY + deltaY,
                    bAnimation: false,
                    CanvasMoveArgs.All
                );

                // 更新最后的鼠标位置
                lastMousePosition = e.Location;
            }
        }


        private void STNodeEditorMain_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var mousePosition = STNodeEditorMain.PointToClient(e.Location);

            if (e.Delta < 0)
            {
                STNodeEditorMain.ScaleCanvas(STNodeEditorMain.CanvasScale - 0.05f, mousePosition.X, mousePosition.Y);
            }
            else
            {
                STNodeEditorMain.ScaleCanvas(STNodeEditorMain.CanvasScale + 0.05f, mousePosition.X, mousePosition.Y);
            }
        }

        private void GridViewColumnSort(object sender, RoutedEventArgs e)
        {

        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }


        public void Dispose()
        {
            STNodeEditorMain?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
