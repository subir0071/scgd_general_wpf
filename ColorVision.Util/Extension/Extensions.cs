﻿#pragma warning disable CS8602 // 取消引用可能出现的空引用。

using System;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Reflection;
using ColorVision.MVVM;
using System.Diagnostics;
using System.Text.Unicode;
using Newtonsoft.Json;
using System.IO;


namespace ColorVision.Extension
{
    /// <summary>
    // 扩展加载，没有特殊标记的丢在这里，反正会自动识别加载
    /// </summary>
    public static class Extensions
    {
        public static JsonSerializerSettings settings { get; set; } = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };


        public static string ToJsonN(this object obj, JsonSerializerSettings? options = null)
        {
            return JsonConvert.SerializeObject(obj, options ?? settings);
        }

        public static bool ToJsonNFile(this object obj, string filePath, JsonSerializerSettings? options = null)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(obj, options ?? settings);
                File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("### [" + ex.Source + "] Exception: " + ex.Message);
                Trace.WriteLine("### " + ex.StackTrace);
                return false;
            }
        }


        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);


        public static SolidColorBrush ToBrush(this Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public static SolidColorBrush ToBrush(this Color? color)
        {
            if (color == null)
                return new SolidColorBrush(Colors.Transparent);
            return new SolidColorBrush((Color)color);
        }

        public static Color ToColor(this string color)
        {
            return (Color)ColorConverter.ConvertFromString(color);
        }

        public static Color ToColor(this SolidColorBrush brush)
        {
            if (brush == null)
                return Colors.Transparent;
            return brush.Color;
        }

        public static Color ToColor(this Brush brush)
        {
            if (brush == null)
                return Colors.Transparent;
            if (brush is SolidColorBrush)
                return (brush as SolidColorBrush).Color;
            else if (brush is LinearGradientBrush)
                return (brush as LinearGradientBrush).GradientStops[0].Color;
            else if (brush is RadialGradientBrush)
                return (brush as RadialGradientBrush).GradientStops[0].Color;
            else
                return Colors.Transparent;
        }

        /// <summary>
        /// 对图标的扩展
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource ToImageSource(this System.Drawing.Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }


        public static string Description(object obj)
        {
            Type type = obj.GetType();
            MemberInfo[] infos = type.GetMember(obj.ToString() ?? "");
            if (infos != null && infos.Length != 0)
            {
                object[] attrs = infos[0].GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
                if (attrs != null && attrs.Length != 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return type.ToString();
        }


    }
}
