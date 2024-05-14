﻿using ColorVision.Common.Extension;
using ColorVision.Common.MVVM;
using ColorVision.UI.HotKey;
using ColorVision.Themes;
using ColorVision.UI;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ColorVision.Themes
{
    public class ThemesExport : IHotKey,IMenuItemMeta
    {
        public string? OwnerGuid => "Tool";

        public string? GuidId => "MenuTheme";

        public int Order => 1000;

        public string? Header => Util.Properties.Resource.MenuTheme;

        public string? InputGestureText => "Ctrl + Shift + T";

        public object? Icon => null;
        public RelayCommand Command => new(a => { });

        public MenuItem MenuItem { get
            { 
                MenuItem MenuTheme = new() { Header = Header,InputGestureText =InputGestureText };

                foreach (var item in Enum.GetValues(typeof(Theme)).Cast<Theme>())
                {
                    MenuItem ThemeItem = new();
                    ThemeItem.Header = Util.Properties.Resource.ResourceManager.GetString(item.ToDescription(), CultureInfo.CurrentUICulture) ?? "";
                    ThemeItem.Click += (s, e) =>
                    {
                        ThemeConfig.Instance.Theme = item;
                        Application.Current.ApplyTheme(item);
                    };
                    ThemeItem.Tag = item;
                    ThemeItem.IsChecked = ThemeManager.Current.CurrentTheme == item;
                    MenuTheme.Items.Add(ThemeItem);
                }

                MenuTheme.Loaded += (s, e) =>
                {
                    foreach (var item in MenuTheme.Items)
                    {
                        if (item is MenuItem ThemeItem && ThemeItem.Tag is Theme Theme)
                            ThemeItem.IsChecked = ThemeManager.Current.CurrentTheme == Theme;
                    }
                };
                return MenuTheme;
            } }

        public HotKeys HotKeys => new(Util.Properties.Resource.Theme, new Hotkey(Key.T, ModifierKeys.Control | ModifierKeys.Shift), Execute);

        private void Execute()
        {
            // 获取当前主题的索引
            int currentThemeIndex = (int)(ThemeManager.Current.CurrentTheme ?? Theme.UseSystem);

            // 获取主题总数，缓存以避免重复计算
            int themeCount = Enum.GetValues(typeof(Theme)).Length;

            // 计算下一个主题的索引
            int nextThemeIndex = (currentThemeIndex + 1) % themeCount;

            // 更新当前主题
            Theme newTheme = (Theme)nextThemeIndex;
            ThemeConfig.Instance.Theme = newTheme;

            // 应用新主题
            Application.Current.ApplyTheme(newTheme);
        }
    }
}
