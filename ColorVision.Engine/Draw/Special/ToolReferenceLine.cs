﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;
using ColorVision.Engine.Draw;
using System;
using System.Collections.Generic;

namespace ColorVision.Util.Draw.Special
{
    public class ToolReferenceLine
    {
        private ZoomboxSub ZoomboxSub { get; set; }
        private DrawCanvas Image { get; set; }

        public DrawingVisual DrawVisualImage { get; set; }

        public int Mode { get; set; } = 2;

        public ToolReferenceLine(ZoomboxSub zombox, DrawCanvas drawCanvas)
        {
            ZoomboxSub = zombox;
            Image = drawCanvas;
            DrawVisualImage = new DrawingVisual();
        }
        public bool IsShow
        {
            get => _IsShow; set
            {
                if (_IsShow == value) return;
                _IsShow = value;
                DrawVisualImageControl(_IsShow);
                Image.ContextMenu = null;
                RMouseDownP = new Point(Image.ActualWidth / 2, Image.ActualHeight / 2);
                if (value)
                {
                    Image.MouseMove += MouseMove;
                    Image.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
                    Image.PreviewMouseRightButtonDown += Image_PreviewMouseRightButtonDown;
                    Image.PreviewMouseUp += PreviewMouseUp;
                    ZoomboxSub.LayoutUpdated += ZoomboxSub_LayoutUpdated;

                }
                else
                {
                    Image.MouseMove -= MouseMove;
                    Image.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
                    Image.PreviewMouseRightButtonDown -= Image_PreviewMouseRightButtonDown;
                    Image.PreviewMouseUp -= PreviewMouseUp;
                    ZoomboxSub.LayoutUpdated -= ZoomboxSub_LayoutUpdated;

                }
            }
        }

        private void ZoomboxSub_LayoutUpdated(object? sender, EventArgs e)
        {
            if (Radio != ZoomboxSub.ContentMatrix.M11)
            {
                Radio = ZoomboxSub.ContentMatrix.M11;
                Render();
            }
        }
        double Radio;

        private bool IsRMouseDown;
        private bool IsLMouseDown;

        private Point RMouseDownP;
        private Point LMouseDownP;
        private Vector PointLen;


        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RMouseDownP = Mouse.GetPosition(Image);
            IsRMouseDown = true;
            Render();
        }
        private void Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            LMouseDownP = Mouse.GetPosition(Image);
            IsLMouseDown = true;
            PointLen = LMouseDownP - RMouseDownP;
            Render();
        }

        private void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsRMouseDown = false;
            IsLMouseDown = false;
        }



        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (IsShow && (IsRMouseDown || IsLMouseDown))
            {
                if (IsRMouseDown)
                {
                    RMouseDownP = e.GetPosition(Image);
                }
                if (IsLMouseDown)
                {
                    LMouseDownP = e.GetPosition(Image);
                    PointLen = LMouseDownP - RMouseDownP;
                }
                Render();
            }
        }

        public static double CalculateAngle(Point point1, Point point2)
        {
            // 计算向量差
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;

            // 使用Atan2计算弧度
            double angleInRadians = Math.Atan2(deltaY, deltaX);

            // 将弧度转换为度
            double angleInDegrees = angleInRadians * (180.0 / Math.PI);

            // 标准化角度到[0, 360)范围，如果需要的话
            //if (angleInDegrees < 0) angleInDegrees += 360;

            return angleInDegrees;
        }

        private static double Det(double a, double b, double c, double d)
        {
            return a * d - b * c;
        }

        public static Point? GetIntersection(Point p, double angle, Point p1, Point p2)
        {
            // Convert angle to radians
            double angleRad = angle * Math.PI / 180.0;

            // Define the second lenc for the line from the given lenc and angle
            Point pAngle = new(p.X + Math.Cos(angleRad), p.Y + Math.Sin(angleRad));

            // Calculate the intersection of the two lines
            double detL1 = Det(p.X, p.Y, pAngle.X, pAngle.Y);
            double detL2 = Det(p1.X, p1.Y, p2.X, p2.Y);
            double x1mx2 = p.X - pAngle.X;
            double x3mx4 = p1.X - p2.X;
            double y1my2 = p.Y - pAngle.Y;
            double y3my4 = p1.Y - p2.Y;

            double xnom = Det(detL1, x1mx2, detL2, x3mx4);
            double ynom = Det(detL1, y1my2, detL2, y3my4);
            double denom = Det(x1mx2, y1my2, x3mx4, y3my4);

            if (denom == 0.0) // Lines are parallel
            {
                return null;
            }

            double x = xnom / denom;
            double y = ynom / denom;
            Point intersection = new(x, y);

            // Check if the intersection lenc lies on the line segment p1-p2
            if (!IsBetween(p1, p2, intersection))
            {
                return null; // Intersection is not within the line segment
            }

            return intersection;
        }

        private static bool IsBetween(Point A, Point B, Point C)
        {
            bool withinX = Math.Min(A.X, B.X) <= C.X && C.X <= Math.Max(A.X, B.X);
            bool withinY = Math.Min(A.Y, B.Y) <= C.Y && C.Y <= Math.Max(A.Y, B.Y);
            return withinX && withinY;
        }

        public static List<Point> CalculateIntersectionPoints(double width, double height, Point point, double angle)
        {
            List<Point> points = new();
            if (GetIntersection(point, angle, new Point(0, 0), new Point(0, width)) is Point point1)
                points.Add(point1);
            if (GetIntersection(point, angle, new Point(0, width), new Point(height, width)) is Point point2)
                points.Add(point2);
            if (GetIntersection(point, angle, new Point(height, width), new Point(height, 0)) is Point point3)
                points.Add(point3);
            if (GetIntersection(point, angle, new Point(height, 0), new Point(0, 0)) is Point point4)
                points.Add(point4);


            if (GetIntersection(point, angle + 90, new Point(0, 0), new Point(0, width)) is Point point5)
                points.Add(point5);
            if (GetIntersection(point, angle + 90, new Point(0, width), new Point(height, width)) is Point point6)
                points.Add(point6);
            if (GetIntersection(point, angle + 90, new Point(height, width), new Point(height, 0)) is Point point7)
                points.Add(point7);
            if (GetIntersection(point, angle + 90, new Point(height, 0), new Point(0, 0)) is Point point8)
                points.Add(point8);

            return points;
        }

        public void Render()
        {
            using DrawingContext dc = DrawVisualImage.RenderOpen();
            Brush brush = Brushes.Red;
            double ratio = 1 / ZoomboxSub.ContentMatrix.M11;
            Pen pen = new(brush, ratio);


            Point ActL = RMouseDownP + PointLen;


            double angle = CalculateAngle(RMouseDownP, ActL);
            Point CenterPoint = RMouseDownP;
            double ActualWidth = Image.ActualWidth;
            double ActualHeight = Image.ActualHeight;

            if (Mode == 0)
            {
                // 旋转变换
                List<Point> intersectionPoints = CalculateIntersectionPoints(ActualHeight, ActualWidth, CenterPoint, angle);

                if (intersectionPoints.Count == 4)
                {
                    dc.DrawLine(pen, intersectionPoints[0], intersectionPoints[1]); // 水平线
                    dc.DrawLine(pen, intersectionPoints[2], intersectionPoints[3]); // 垂直线
                }

                TextAttribute textAttribute = new();
                textAttribute.FontSize = 15 / ZoomboxSub.ContentMatrix.M11;

                FormattedText formattedText = new(angle.ToString("F1") + "°", CultureInfo.CurrentCulture, textAttribute.FlowDirection, new Typeface(textAttribute.FontFamily, textAttribute.FontStyle, textAttribute.FontWeight, textAttribute.FontStretch), textAttribute.FontSize, textAttribute.Brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                dc.DrawText(formattedText, RMouseDownP + new Vector(20, 20));



                int lenc = (int)Math.Sqrt(PointLen.X * PointLen.X + PointLen.Y * PointLen.Y);

                dc.DrawEllipse(Brushes.Transparent, pen, RMouseDownP, lenc, lenc);
                dc.DrawEllipse(Brushes.Transparent, pen, RMouseDownP, lenc + 10, lenc + 10);
                dc.DrawEllipse(Brushes.Transparent, pen, RMouseDownP, lenc + 30, lenc + 30);
                dc.DrawEllipse(Brushes.Transparent, pen, RMouseDownP, lenc + 50, lenc + 50);
                dc.DrawEllipse(Brushes.Transparent, pen, RMouseDownP, lenc + 100, lenc + 100);
                dc.DrawEllipse(Brushes.Transparent, pen, RMouseDownP, lenc + 200, lenc + 200);


                //dc.PushTransform(new RotateTransform(angle, RMouseDownP.X, RMouseDownP.Y));
                FormattedText formattedText1 = new(lenc.ToString("F1"), CultureInfo.CurrentCulture, textAttribute.FlowDirection, new Typeface(textAttribute.FontFamily, textAttribute.FontStyle, textAttribute.FontWeight, textAttribute.FontStretch), textAttribute.FontSize, textAttribute.Brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                dc.DrawText(formattedText1, RMouseDownP + PointLen);

                FormattedText formattedText2 = new((lenc + 10).ToString("F1"), CultureInfo.CurrentCulture, textAttribute.FlowDirection, new Typeface(textAttribute.FontFamily, textAttribute.FontStyle, textAttribute.FontWeight, textAttribute.FontStretch), textAttribute.FontSize, textAttribute.Brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                dc.DrawText(formattedText2, RMouseDownP + PointLen);

                FormattedText formattedText3 = new((lenc + 1).ToString("F1"), CultureInfo.CurrentCulture, textAttribute.FlowDirection, new Typeface(textAttribute.FontFamily, textAttribute.FontStyle, textAttribute.FontWeight, textAttribute.FontStretch), textAttribute.FontSize, textAttribute.Brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                dc.DrawText(formattedText3, RMouseDownP - PointLen);
            }
            else if (Mode == 1)
            {

                // 旋转变换
                List<Point> intersectionPoints = CalculateIntersectionPoints(ActualHeight, ActualWidth, CenterPoint, angle);

                if (intersectionPoints.Count == 4)
                {
                    dc.DrawLine(pen, intersectionPoints[0], intersectionPoints[1]); // 水平线
                    dc.DrawLine(pen, intersectionPoints[2], intersectionPoints[3]); // 垂直线
                }

                TextAttribute textAttribute = new();
                textAttribute.FontSize = 15 / ZoomboxSub.ContentMatrix.M11;

                FormattedText formattedText = new(angle.ToString("F1") + "°", CultureInfo.CurrentCulture, textAttribute.FlowDirection, new Typeface(textAttribute.FontFamily, textAttribute.FontStyle, textAttribute.FontWeight, textAttribute.FontStretch), textAttribute.FontSize, textAttribute.Brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                dc.DrawText(formattedText, RMouseDownP + new Vector(20, 20));
            }
            else if (Mode == 2)
            {
                double angle1 = (angle + 45) * Math.PI / 180.0;
                // 旋转变换
                List<Point> intersectionPoints = CalculateIntersectionPoints(ActualHeight, ActualWidth, CenterPoint + new Vector(5 * ratio * Math.Cos(angle1), 5 * ratio * Math.Sin(angle1)), angle);

                if (intersectionPoints.Count == 4)
                {
                    dc.DrawLine(pen, intersectionPoints[0], intersectionPoints[1]); // 水平线,
                    dc.DrawLine(pen, intersectionPoints[2], intersectionPoints[3]); // 垂直线
                }
                intersectionPoints = CalculateIntersectionPoints(ActualHeight, ActualWidth, CenterPoint - new Vector(5 * ratio * Math.Cos(angle1), 5 * ratio * Math.Sin(angle1)), angle);
                if (intersectionPoints.Count == 4)
                {
                    dc.DrawLine(pen, intersectionPoints[0], intersectionPoints[1]); // 水平线,
                    dc.DrawLine(pen, intersectionPoints[2], intersectionPoints[3]); // 垂直线
                }




                TextAttribute textAttribute = new();
                textAttribute.FontSize = 15 / ZoomboxSub.ContentMatrix.M11;

                FormattedText formattedText = new(angle.ToString("F1") + "°", CultureInfo.CurrentCulture, textAttribute.FlowDirection, new Typeface(textAttribute.FontFamily, textAttribute.FontStyle, textAttribute.FontWeight, textAttribute.FontStretch), textAttribute.FontSize, textAttribute.Brush, VisualTreeHelper.GetDpi(DrawVisualImage).PixelsPerDip);
                dc.DrawText(formattedText, RMouseDownP + new Vector(20, 20));
            }



        }

        private bool _IsShow;

        public void DrawVisualImageControl(bool Control)
        {
            if (Control)
            {
                if (!Image.ContainsVisual(DrawVisualImage))
                    Image.AddVisual(DrawVisualImage);
            }
            else
            {
                if (Image.ContainsVisual(DrawVisualImage))
                    Image.RemoveVisual(DrawVisualImage);
            }
        }

    }
}
