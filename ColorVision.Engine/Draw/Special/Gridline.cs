﻿using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;
using System.Linq;

namespace ColorVision.Engine.Draw.Special
{
    public class Gridline
    {
        private ZoomboxSub ZoomboxSub { get; set; }
        private DrawCanvas DrawCanvas { get; set; }

        public DrawingVisual DrawVisualImage { get; set; }

        public Gridline(ZoomboxSub zombox, DrawCanvas drawCanvas)
        {
            ZoomboxSub = zombox;
            DrawCanvas = drawCanvas;
            DrawVisualImage = new DrawingVisual();
        }

        public bool IsShow
        {
            get => _IsShow; set
            {
                if (_IsShow == value) return;
                _IsShow = value;
                DrawVisualImageControl(_IsShow);
                if (value)
                {
                    DrawCanvas.MouseMove += MouseMove;
                    DrawCanvas.MouseEnter += MouseEnter;
                    DrawCanvas.MouseLeave += MouseLeave;
                    ZoomboxSub.LayoutUpdated += ZoomboxSub_LayoutUpdated;
                    DefalutTextAttribute.Defalut.PropertyChanged += Defalut_PropertyChanged;
                }
                else
                {
                    DrawCanvas.MouseMove -= MouseMove;
                    DrawCanvas.MouseEnter -= MouseEnter;
                    DrawCanvas.MouseLeave -= MouseLeave;
                    DefalutTextAttribute.Defalut.PropertyChanged -= Defalut_PropertyChanged;

                }
            }
        }

        private void Defalut_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Radio = ZoomboxSub.ContentMatrix.M11;
            DrawImage();
        }

        private void ZoomboxSub_LayoutUpdated(object? sender, System.EventArgs e)
        {
            if (Radio != ZoomboxSub.ContentMatrix.M11)
            {
                Radio = ZoomboxSub.ContentMatrix.M11;
                DrawImage();
            }

        }
        public static double ActualLength { get => DefalutTextAttribute.Defalut.IsUsePhysicalUnit ? DefalutTextAttribute.Defalut.ActualLength : 1; set { DefalutTextAttribute.Defalut.ActualLength = value; } }
        public static string PhysicalUnit { get => DefalutTextAttribute.Defalut.IsUsePhysicalUnit ? DefalutTextAttribute.Defalut.PhysicalUnit : "Px"; set { DefalutTextAttribute.Defalut.PhysicalUnit = value; } }

        private bool _IsShow;

        double Radio;
        public void DrawImage()
        {
            if (DrawCanvas.Source is BitmapSource bitmapSource)
            {


                Brush brush = Brushes.Red;
                FontFamily fontFamily = new("Arial");
                double ratio = 1 / ZoomboxSub.ContentMatrix.M11;
                Pen pen = new(brush, ratio);

                int lenindex = (int)(40 * ratio);
                double fontSize = 15 / ZoomboxSub.ContentMatrix.M11; 
                using DrawingContext dc = DrawVisualImage.RenderOpen();
                for (int i = 0; i < bitmapSource.Width; i += lenindex)
                {
                    string text = (i * ActualLength).ToString("F0") ;
                    FormattedText formattedText = new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), fontSize, brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                    if (DrawCanvas.RenderTransform is TransformGroup transformGroup && transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault() is ScaleTransform scaleTransform)
                    {
                        dc.PushTransform(scaleTransform);
                        dc.DrawText(formattedText, new Point(-40 / ZoomboxSub.ContentMatrix.M11, i - 10 / ZoomboxSub.ContentMatrix.M11));
                        dc.Pop();
                    }
                    else
                    {
                        dc.DrawText(formattedText, new Point(-40 / ZoomboxSub.ContentMatrix.M11, i - 10 / ZoomboxSub.ContentMatrix.M11));
                    }
                    dc.DrawLine(pen, new Point(i, 0), new Point(i, bitmapSource.Height));
                }

                for (int i = 0; i < bitmapSource.Height; i += lenindex)
                {
                    string text = (i * ActualLength).ToString("F0");
                    FormattedText formattedText = new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), fontSize, brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);

                    if (DrawCanvas.RenderTransform is TransformGroup transformGroup && transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault() is ScaleTransform scaleTransform)
                    {
                        dc.PushTransform(scaleTransform);
                        dc.DrawText(formattedText, new Point(-40 / ZoomboxSub.ContentMatrix.M11, i - 10 / ZoomboxSub.ContentMatrix.M11));
                        dc.Pop();
                    }
                    else
                    {
                        dc.DrawText(formattedText, new Point(-40 / ZoomboxSub.ContentMatrix.M11, i - 10 / ZoomboxSub.ContentMatrix.M11));
                    }
                    dc.DrawLine(pen, new Point(0, i), new Point(bitmapSource.Width, i));
                }
            }
        }

        public double Ratio { get; set;}

        public void MouseMove(object sender, MouseEventArgs e)
        {

        }

        public void MouseEnter(object sender, MouseEventArgs e) => DrawVisualImageControl(true);

        public void MouseLeave(object sender, MouseEventArgs e) => DrawVisualImageControl(true);

        public void DrawVisualImageControl(bool Control)
        {
            if (Control)
            {
                if (!DrawCanvas.ContainsVisual(DrawVisualImage))
                    DrawCanvas.AddVisual(DrawVisualImage);
            }
            else
            {
                if (DrawCanvas.ContainsVisual(DrawVisualImage))
                    DrawCanvas.RemoveVisual(DrawVisualImage);
            }
        }
    }
}
