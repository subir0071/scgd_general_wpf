﻿using ColorVision.Common.MVVM;
using ColorVision.Engine.Draw.Ruler;
using ColorVision.Engine.Draw.Special;
using ColorVision.Util.Draw.Special;
using Gu.Wpf.Geometry;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorVision.Engine.Draw
{

    public class EditModeChangedEventArgs : EventArgs
    {
        public EditModeChangedEventArgs() { }   
        public EditModeChangedEventArgs(bool isEditMode) { IsEditMode = isEditMode; }
        public bool IsEditMode { get; set; }
    }

    public class WindowStatus
    {
        public object Root { get; set; }
        public Panel Parent { get; set; }
        public ContentControl ContentParent { get; set; }
        public WindowStyle WindowStyle { get; set; }

        public WindowState WindowState { get; set; }

        public ResizeMode ResizeMode { get; set; }
    }

    public class ToolBarTop : ViewModelBase,IDisposable
    {
        public RelayCommand ZoomUniformToFill { get; set; }
        public RelayCommand ZoomUniform { get; set; }
        public RelayCommand ZoomIncrease { get; set; }
        public RelayCommand ZoomDecrease { get; set; }
        public RelayCommand ZoomNone { get; set; }
        public RelayCommand MaxCommand { get; set; }

        public RelayCommand RotateLeftCommand { get; set; }
        public RelayCommand RotateRightCommand { get; set; }

        public RelayCommand FlipHorizontalCommand { get; set; }
        public RelayCommand FlipVerticalCommand { get; set; }


        public RelayCommand SaveImageCommand { get; set; }
        public RelayCommand ExportImageCommand { get; set; }

        public RelayCommand ClearImageCommand { get; set; }

        public event EventHandler ClearImageEventHandler;

        public RelayCommand PrintImageCommand { get; set; }

        public RelayCommand OpenProperty { get; set; }

        private ZoomboxSub ZoomboxSub { get; set; }

        private DrawCanvas Image { get; set; }

        public MouseMagnifier MouseMagnifier { get; set; }

        public Crosshair Crosshair { get; set; }
        public Gridline Gridline { get; set; }

        private ToolBarMeasure ToolBarMeasure { get; set; }

        private FrameworkElement Parent { get; set; }

        public ToolBarScaleRuler ToolBarScaleRuler { get; set; }

        public ToolReferenceLine ToolConcentricCircle { get; set; }

        public ToolBarTop(FrameworkElement Parent,ZoomboxSub zoombox, DrawCanvas drawCanvas)
        {
            this.Parent = Parent;
            ZoomboxSub = zoombox ?? throw new ArgumentNullException(nameof(zoombox));
            Image = drawCanvas ?? throw new ArgumentNullException(nameof(drawCanvas));

            MouseMagnifier = new MouseMagnifier(zoombox, drawCanvas);
            Crosshair = new Crosshair(zoombox, drawCanvas);
            Gridline = new Gridline(zoombox, drawCanvas);
            ToolBarMeasure = new ToolBarMeasure(Parent, zoombox, drawCanvas);
            ToolBarScaleRuler = new ToolBarScaleRuler(Parent, zoombox, drawCanvas);

            ToolBarScaleRuler.ScalRuler.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ToolBarScaleRuler.ScalRuler.ActualLength))
                {

                }
                else if (e.PropertyName == nameof(ToolBarScaleRuler.ScalRuler.PhysicalUnit))
                {

                }

            };
            ToolConcentricCircle = new ToolReferenceLine(zoombox, drawCanvas);

            ZoomUniformToFill = new RelayCommand(a => ZoomboxSub.ZoomUniformToFill(), a => Image.Source != null);
            ZoomUniform = new RelayCommand(a => ZoomboxSub.ZoomUniform(),a => Image.Source != null);
            ZoomIncrease = new RelayCommand(a => ZoomboxSub.Zoom(1.25), a => Image.Source != null);
            ZoomDecrease = new RelayCommand(a => ZoomboxSub.Zoom(0.8), a => Image.Source != null);
            ZoomNone = new RelayCommand(a => ZoomboxSub.ZoomNone(), a => Image.Source != null);

            FlipHorizontalCommand = new RelayCommand(a => FlipHorizontal(), a => Image.Source != null);
            FlipVerticalCommand = new RelayCommand(a =>FlipVertical(), a => Image.Source != null);
            this.Parent.PreviewKeyDown += PreviewKeyDown;
            zoombox.Cursor = Cursors.Hand;

            SaveImageCommand = new RelayCommand(a => Save(),a=>Image.Source!=null);

            PrintImageCommand = new RelayCommand(a => Print(), a => Image.Source != null);

            ClearImageCommand = new RelayCommand(a => ClearImage(),a => Image.Source != null);
            MaxCommand = new RelayCommand(a => MaxImage());

            RotateLeftCommand = new RelayCommand(a => RotateLeft());
            RotateRightCommand = new RelayCommand(a => RotateRight());

            EditModeChanged += (s, e) =>
            {
                if (e.IsEditMode)
                {
                    ZoomboxSub.ContextMenu = null;
                }
                else
                {
                    AddContextMenu();
                }
            };
            AddContextMenu();
        }




        public void Print()
        {
            PrintDialog printDialog = new();
            if (printDialog.ShowDialog() == true)
            {
                // 创建一个可打印的区域
                Size pageSize = new(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                Image.Measure(pageSize);
                Image.Arrange(new Rect(5, 5, pageSize.Width, pageSize.Height));

                // 开始打印
                printDialog.PrintVisual(Image, "Printing");
            }

        }


        public void AddContextMenu()
        {
            ContextMenu contextMenu = new();
            MenuItem menuItemZoom = new() { Header = "缩放工具", Command = SaveImageCommand };
            menuItemZoom.Items.Add(new MenuItem() { Header = "放大", Command = ZoomIncrease });
            menuItemZoom.Items.Add(new MenuItem() { Header = "缩小", Command = ZoomIncrease });
            menuItemZoom.Items.Add(new MenuItem() { Header = "原始大小", Command = ZoomNone });
            menuItemZoom.Items.Add(new MenuItem() { Header = "适应屏幕", Command = ZoomUniform });

            contextMenu.Items.Add(menuItemZoom);


            contextMenu.Items.Add(new MenuItem() { Header = "左旋转", Command = RotateLeftCommand });
            contextMenu.Items.Add(new MenuItem() { Header = "右旋转", Command = RotateRightCommand });

            contextMenu.Items.Add(new MenuItem() { Header = "水平翻转", Command = FlipHorizontalCommand });
            contextMenu.Items.Add(new MenuItem() { Header = "垂直翻转", Command = FlipVerticalCommand });

            contextMenu.Items.Add(new MenuItem() { Header = "全屏", Command = MaxCommand, InputGestureText = "F11" });
            contextMenu.Items.Add(new MenuItem() { Header = "清空", Command = ClearImageCommand });

            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = "另存为", Command = SaveImageCommand });
            contextMenu.Items.Add(new MenuItem() { Header = "Print", Command = PrintImageCommand });

            contextMenu.Items.Add(new MenuItem() { Header = "清空", Command = ClearImageCommand });


            ZoomboxSub.ContextMenu = contextMenu;
        }

        public void FlipHorizontal()
        {
            if (Image.RenderTransform is TransformGroup transformGroup)
            {
                var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX *= -1;
                }
                else
                {
                    scaleTransform = new ScaleTransform { ScaleX = -1 };
                    transformGroup.Children.Add(scaleTransform);
                }
            }
            else
            {
                transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform { ScaleX = -1 });
                Image.RenderTransform = transformGroup;
                Image.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public void FlipVertical()
        {
            if (Image.RenderTransform is TransformGroup transformGroup)
            {
                var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleY *= -1;
                }
                else
                {
                    scaleTransform = new ScaleTransform { ScaleY = -1 };
                    transformGroup.Children.Add(scaleTransform);
                }
            }
            else
            {
                transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform { ScaleY = -1 });
                Image.RenderTransform = transformGroup;
                Image.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public void RotateRight()
        {
            if (Image.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle += 90;
            }
            else
            {
                RotateTransform rotateTransform1 = new() { Angle = 90 };
                Image.RenderTransform = rotateTransform1;
                Image.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public void RotateLeft()
        {
            if (Image.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle -= 90;
            }
            else
            {
                RotateTransform rotateTransform1 = new() { Angle = -90 };
                Image.RenderTransform = rotateTransform1;
                Image.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private WindowStatus OldWindowStatus { get; set; }
        public bool IsMax { get; set; }
        public void MaxImage()
        {
            void PreviewKeyDown(object s, KeyEventArgs e)
            {
                if (e.Key == Key.Escape || e.Key == Key.F11)
                {
                    if (IsMax)
                        MaxImage();
                }
            }

            var window = Window.GetWindow(Parent);
            if (!IsMax)
            {
                IsMax = true;
                if (Parent.Parent is Panel p)
                {
                    OldWindowStatus = new WindowStatus();
                    OldWindowStatus.Parent = p;
                    OldWindowStatus.WindowState = window.WindowState;
                    OldWindowStatus.WindowStyle = window.WindowStyle;
                    OldWindowStatus.ResizeMode = window.ResizeMode;
                    OldWindowStatus.Root = window.Content;
                    window.WindowStyle = WindowStyle.None;
                    window.WindowState = WindowState.Maximized;

                    OldWindowStatus.Parent.Children.Remove(Parent);
                    window.Content = Parent;

                    window.PreviewKeyDown -= PreviewKeyDown;
                    window.PreviewKeyDown += PreviewKeyDown;
                }
                else if (Parent.Parent is ContentControl content)
                {
                    OldWindowStatus = new WindowStatus();
                    OldWindowStatus.ContentParent = content;
                    OldWindowStatus.WindowState = window.WindowState;
                    OldWindowStatus.WindowStyle = window.WindowStyle;
                    OldWindowStatus.ResizeMode = window.ResizeMode;
                    OldWindowStatus.Root = window.Content;
                    window.WindowStyle = WindowStyle.None;
                    window.WindowState = WindowState.Maximized;

                    content.Content = null;
                    window.Content = Parent;
                    window.PreviewKeyDown -= PreviewKeyDown;
                    window.PreviewKeyDown += PreviewKeyDown;
                    
                    return;
                }
            }
            else
            {
                IsMax =false;
                if (OldWindowStatus.Parent != null)
                {
                    window.WindowStyle = OldWindowStatus.WindowStyle;
                    window.WindowState = OldWindowStatus.WindowState;
                    window.ResizeMode = OldWindowStatus.ResizeMode;

                    window.Content = OldWindowStatus.Root;
                    OldWindowStatus.Parent.Children.Add(Parent);
                }
                else
                {
                    window.WindowStyle = OldWindowStatus.WindowStyle;
                    window.WindowState = OldWindowStatus.WindowState;
                    window.ResizeMode = OldWindowStatus.ResizeMode;

                    OldWindowStatus.ContentParent.Content = Parent;
                }
                window.PreviewKeyDown -= PreviewKeyDown;
            }
        }

        public void ClearImage()
        {
            Image.Clear();
            Image.Source = null;
            Image.UpdateLayout();
            ToolBarScaleRuler.IsShow = false;
            ClearImageEventHandler?.Invoke(this, new EventArgs());
        }

        public void Save()
        {
            using var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "Png (*.png) | *.png";
            dialog.FileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            RenderTargetBitmap renderTargetBitmap = new((int)Image.ActualWidth, (int)Image.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(Image);

            // 创建一个PngBitmapEncoder对象来保存位图为PNG文件
            PngBitmapEncoder pngEncoder = new();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            // 将PNG内容保存到文件
            using FileStream fileStream = new(dialog.FileName, FileMode.Create);
            pngEncoder.Save(fileStream);
        }


        private void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (!IsMax)
                    MaxImage();
            }
            if (e.Key == Key.Add)
            {
                ZoomIncrease.RaiseExecute(e);
            }
            if (e.Key == Key.Subtract)
            {
                ZoomDecrease.RaiseExecute(e);
            }

            if (e.Key == Key.Left)
            {
                TranslateTransform translateTransform = new();
                Vector vector = new(-10, 0);
                translateTransform.SetCurrentValue(TranslateTransform.XProperty, vector.X);
                translateTransform.SetCurrentValue(TranslateTransform.YProperty, vector.Y);
                ZoomboxSub.SetCurrentValue(Zoombox.ContentMatrixProperty, Matrix.Multiply(ZoomboxSub.ContentMatrix, translateTransform.Value));
            }
            else if (e.Key == Key.Right)
            {
                TranslateTransform translateTransform = new();
                Vector vector = new(10, 0);
                translateTransform.SetCurrentValue(TranslateTransform.XProperty, vector.X);
                translateTransform.SetCurrentValue(TranslateTransform.YProperty, vector.Y);
                ZoomboxSub.SetCurrentValue(Zoombox.ContentMatrixProperty, Matrix.Multiply(ZoomboxSub.ContentMatrix, translateTransform.Value));
            }
            else if (e.Key == Key.Up)
            {
                TranslateTransform translateTransform = new();
                Vector vector = new(0, -10);
                translateTransform.SetCurrentValue(TranslateTransform.XProperty, vector.X);
                translateTransform.SetCurrentValue(TranslateTransform.YProperty, vector.Y);
                ZoomboxSub.SetCurrentValue(Zoombox.ContentMatrixProperty, Matrix.Multiply(ZoomboxSub.ContentMatrix, translateTransform.Value));
            }
            else if (e.Key == Key.Down)
            {
                TranslateTransform translateTransform = new();
                Vector vector = new(0, 10);
                translateTransform.SetCurrentValue(TranslateTransform.XProperty, vector.X);
                translateTransform.SetCurrentValue(TranslateTransform.YProperty, vector.Y);
                ZoomboxSub.SetCurrentValue(Zoombox.ContentMatrixProperty, Matrix.Multiply(ZoomboxSub.ContentMatrix, translateTransform.Value));
            }
            else if (e.Key == Key.Add)
            {
                ZoomboxSub.Zoom(1.1);
            }
            else if (e.Key == Key.Subtract)
            {
                ZoomboxSub.Zoom(0.9);
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // 确保imageControl已经加载了内容
                if (Image.Source == null)
                {
                    return;
                }
                if (Image.Source is BitmapSource bitmapSource)
                {
                    Clipboard.Clear();
                    Clipboard.SetImage(bitmapSource);
                }
                // 可选：强制垃圾回收
                // GC.Collect();
                // GC.WaitForPendingFinalizers();
                // 将图像复制到剪贴板
                MessageBox.Show("图像已经复制到粘贴板中,该操作目前存在内存泄露");
            }
        }

        public bool ScaleRulerShow
        { 
            get => ToolBarScaleRuler.IsShow;
            set
            {
                if (ToolBarScaleRuler.IsShow == value) return;
                ToolBarScaleRuler.IsShow = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 当前的缩放分辨率
        /// </summary>
        public double ZoomRatio
        {
            get => ZoomboxSub.ContentMatrix.M11;
            set => ZoomboxSub.Zoom(value);
        }

        private bool _Crosshair;
        public bool CrosshairFunction
        {
            get => _Crosshair;
            set
            {
                if (_Crosshair == value) return;
                _Crosshair = value;
                Crosshair.IsShow = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ShowImageInfo;
        public bool ShowImageInfo
        {
            get => _ShowImageInfo; set
            {
                if (_ShowImageInfo == value) return;
                if (value) ImageEditMode = false;
                _ShowImageInfo = value;

                MouseMagnifier.IsShow = value;
                NotifyPropertyChanged();
            }
        }

        public EventHandler<EditModeChangedEventArgs> EditModeChanged { get; set; }

        private bool _ImageEditMode;

        public bool ImageEditMode
        {
            get => _ImageEditMode;
            set
            {
                if (_ImageEditMode == value) return;
                if (value) ShowImageInfo = false;
                _ImageEditMode = value;

                EditModeChanged?.Invoke(this, new EditModeChangedEventArgs() { IsEditMode = value });

                if (_ImageEditMode)
                {
                    ZoomboxSub.ActivateOn = ModifierKeys.Control;
                    ZoomboxSub.Cursor = Cursors.Cross;
                }
                else
                {
                    ZoomboxSub.ActivateOn = ModifierKeys.None;
                    ZoomboxSub.Cursor = Cursors.Hand;

                    LastChoice = string.Empty;
                }
                NotifyPropertyChanged();
            }
        }

        private bool _DrawCircle;
        /// <summary>
        /// 是否画圆形
        /// </summary>
        public bool DrawCircle {  get => _DrawCircle;
            set
            {
                if (_DrawCircle == value) return;
                _DrawCircle = value;
                if (value)
                {
                    ImageEditMode = true;
                    LastChoice = nameof(DrawCircle);
                }
                NotifyPropertyChanged(); 
            }
        }

        private bool _DrawRect;
        /// <summary>
        /// 是否画圆形
        /// </summary>
        public bool DrawRect
        {
            get => _DrawRect;
            set
            {
                if (_DrawRect == value) return;
                _DrawRect = value;
                if (value)
                {
                    ImageEditMode = true;
                    LastChoice = nameof(DrawRect);
                }
                NotifyPropertyChanged();
            }
        }


        public bool Measure {
            get => _Measure;
            set 
                {
                if (_Measure == value) return;
                _Measure = value;
                if (value)
                {
                    ImageEditMode = true;
                    LastChoice = nameof(Measure);
                }
                ToolBarMeasure.Measure = value;
                NotifyPropertyChanged();
            }
        }
        private bool _Measure;



        private bool _DrawPolygon;

        public bool DrawPolygon
        {
            get => _DrawPolygon;
            set
            {
                if (_DrawPolygon == value) return;
                _DrawPolygon = value;
                if (value)
                {
                    ImageEditMode = true;
                    LastChoice = nameof(DrawPolygon);
                }

                NotifyPropertyChanged();
            }
        }

        private bool _ConcentricCircle;

        public bool ConcentricCircle
        {
            get => _ConcentricCircle;
            set
            {
                if (_ConcentricCircle == value) return;
                _ConcentricCircle = value;
                ToolConcentricCircle.IsShow = value;
                NotifyPropertyChanged();
            }
        }





        public string LastChoice { get => _LastChoice; set 
            {
                if (value == _LastChoice)
                    return;
                if (!string.IsNullOrWhiteSpace(_LastChoice))
                {
                    Type type = GetType();
                    PropertyInfo property = type.GetProperty(_LastChoice);
                    property?.SetValue(this, false);
                }
                _LastChoice = value;

            }
        }
        private string _LastChoice { get; set; }

        private bool _EraseVisual;
        private bool disposedValue;

        public bool EraseVisual {  get => _EraseVisual;
            set
            {
                if (_EraseVisual == value) return;
                    _EraseVisual = value;
                if (value)
                {
                    ZoomboxSub.Cursor = Input.Cursors.Eraser;
                }
                else
                {
                    ZoomboxSub.Cursor = Cursors.Arrow;
                }
                if (value)
                {
                    ImageEditMode = true;
                    LastChoice = nameof(EraseVisual);
                }


                NotifyPropertyChanged();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }



        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
