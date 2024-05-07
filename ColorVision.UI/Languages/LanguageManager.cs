﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;

namespace ColorVision.UI.Languages
{
    public class LanguageManager
    {
        public static LanguageManager Current { get; set; } = new LanguageManager();
        private string DefalutProcessName = "ColorVision";

        public List<string> Languages { get; set; } = GetLanguages();

        public LanguageManager()
        {

        }

        public static Dictionary<string, string> keyValuePairs { get; set; }


        public static List<string> GetDefaultLanguages(string DefalutProcessDllName = "ColorVision.resources.dll")
        {
            List<string> list = new List<string>() { };
            string exeFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            foreach (var subDirectory in Directory.GetDirectories(exeFolderPath ?? string.Empty))
            {
                string[] files = Directory.GetFiles(subDirectory, DefalutProcessDllName, SearchOption.AllDirectories);

                if (files.Length > 0 && new DirectoryInfo(subDirectory).Name is string Name && !list.Contains(Name))
                {
                    list.Add(Name);
                }
            }
            if (!list.Contains("zh-Hans"))
                list.Add("zh-Hans");
            return list;
        }


        public static List<string> GetLanguages(string DefalutProcessDllName = "ColorVision.resources.dll")
        {
            keyValuePairs ??= new Dictionary<string, string>();
            keyValuePairs.Clear();

            List<string>  list =  new List<string>() { };
            list.Add(Thread.CurrentThread.CurrentUICulture.Name);

            if (Thread.CurrentThread.CurrentUICulture.Name == CultureInfo.InstalledUICulture.Name)
            {
                keyValuePairs.TryAdd(CultureInfo.InstalledUICulture.Name, Properties.Resources.UseSystem);
            }
            else
            {
                keyValuePairs.TryAdd(Thread.CurrentThread.CurrentUICulture.Name, Properties.Resources.ResourceManager.GetString(Thread.CurrentThread.CurrentUICulture.Name, CultureInfo.CurrentUICulture) ?? Thread.CurrentThread.CurrentUICulture.Name);
            }


            string exeFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            foreach (var subDirectory in Directory.GetDirectories(exeFolderPath??string.Empty))
            {
                string[] files = Directory.GetFiles(subDirectory, DefalutProcessDllName, SearchOption.AllDirectories);

                if (files.Length > 0  && new DirectoryInfo(subDirectory).Name is string Name && !list.Contains(Name))
                {
                    list.Add(Name);
                    keyValuePairs.TryAdd(Name, Properties.Resources.ResourceManager.GetString(Name, CultureInfo.CurrentUICulture) ?? "");
                }
            }
            if (!list.Contains("zh-Hans"))
            {
                list.Add("zh-Hans");
                keyValuePairs.TryAdd("zh-Hans", Properties.Resources.ResourceManager.GetString("zh-Hans", CultureInfo.CurrentUICulture) ?? "");
            }
            if (!list.Contains(CultureInfo.InstalledUICulture.Name))
            {
                list.Add(CultureInfo.InstalledUICulture.Name);
                keyValuePairs.TryAdd(CultureInfo.InstalledUICulture.Name, Properties.Resources.UseSystem);
            }

            return list;
        }



        public bool LanguageChange(string lang)
        {
            if (Thread.CurrentThread.CurrentUICulture.Name != lang)
            {
                if (MessageBox.Show(Properties.Resources.LanguageResartSign, DefalutProcessName, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
                    Process.Start(Application.ResourceAssembly.Location.Replace(".dll",".exe"),"-r");

                    Application.Current.Shutdown();
                    return true;
                }
            }
            return false;
        }


    }
}
