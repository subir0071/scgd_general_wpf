﻿using ColorVision.Common.Utilities;
using ColorVision.FloatingBall;
using ColorVision.Scheduler;
using ColorVision.Solution;
using ColorVision.Solution.Searches;
using ColorVision.Themes;
using ColorVision.UI;
using ColorVision.UI.Configs;
using ColorVision.UI.HotKey;
using ColorVision.UI.Menus;
using ColorVision.UI.Views;
using log4net;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ColorVision
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        public ViewGridManager ViewGridManager { get; set; }
        public static MainWindowConfig Config => MainWindowConfig.Instance;

        public MainWindow()
        {
            InitializeComponent();
            Config.SetWindow(this);
            SizeChanged += (s, e) => Config.SetConfig(this);
            var IsAdministrator = Tool.IsAdministrator();
            //Title += $"- {(IsAdministrator ? Properties.Resources.RunAsAdmin : Properties.Resources.NotRunAsAdmin)}";
            Title = "ColorVision";
            this.ApplyCaption();
            this.SetWindowFull(Config);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            MenuManager.GetInstance().Menu = Menu1;

            this.DataContext = Config;

            if (WindowConfig.IsExist)
            {
                if (WindowConfig.Icon != null)
                    Icon = WindowConfig.Icon;
                Title = WindowConfig.Title ?? Title;
            }
            ViewGridManager SolutionViewGridManager = new();
            SolutionViewGridManager.MainView = SolutionGrid;
            SolutionView solutionView = new();
            SolutionViewGridManager.AddView(0, solutionView);
            solutionView.View.ViewIndex = 0;

            SolutionViewGridManager.SetViewNum(-1);

            ViewGridManager = ViewGridManager.GetInstance();
            ViewGridManager.MainView = ViewGrid;

            ViewGridManager.SetViewGrid(ViewConfig.Instance.LastViewCount);
            ViewGridManager.GetInstance().ViewMaxChangedEvent += (e) => ViewConfig.Instance.LastViewCount = e;

            DisPlayManager.GetInstance().Init(this, StackPanelSPD);

            Debug.WriteLine(Properties.Resources.LaunchSuccess);

            //Task.Run(CheckCertificate);
            SolutionTab1.Content = new TreeViewControl();
            PluginLoader.LoadPlugins("Plugins");
            PluginLoader.LoadAssembly<IPlugin>(Assembly.GetExecutingAssembly());
            MenuManager.GetInstance().LoadMenuItemFromAssembly();
            this.LoadHotKeyFromAssembly();

            QuartzSchedulerManager.GetInstance();
            Application.Current.MainWindow = this;
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadIMainWindowInitialized();

                    FluidMoveBehavior fluidMoveBehavior = new()
                    {
                        AppliesTo = FluidMoveScope.Children,
                        Duration = TimeSpan.FromSeconds(0.1)
                    };
                    Interaction.GetBehaviors(StackPanelSPD).Add(fluidMoveBehavior);
                });
            });
            if (Config.OpenFloatingBall)
                new FloatingBallWindow().Show();
            ProgramTimer.StopAndReport();
        }

        public static async void LoadIMainWindowInitialized() 
        {
            foreach (var assembly in AssemblyHandler.GetInstance().GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes().Where(t => typeof(IMainWindowInitialized).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    if (Activator.CreateInstance(type) is IMainWindowInitialized componentInitialize)
                    {
                        try
                        {
                            await componentInitialize.Initialize();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                    }
                }
            }
        }


        private void ViewGrid_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int nums))
            {
                switch (nums)
                {
                    case 20:
                        ViewGridManager.SetViewGridTwo();
                        break;
                    case 21:
                        ViewGridManager.SetViewGrid(2);
                        break;
                    case 30:
                        ViewGridManager.SetViewGridThree();
                        break;
                    case 31:
                        ViewGridManager.SetViewGridThree(false);
                        break;
                    default:
                        ViewGridManager.SetViewGrid(nums);
                        break;
                }
            }
        }


        private void StatusBarGrid_Initialized(object sender, EventArgs e)
        {
            ContextMenu contextMenu= new ContextMenu();
            StatusBarGrid.ContextMenu = contextMenu;


            void AddStatusBarIconMetadata(StatusBarIconMetadata statusBarIconMetadata)
            {
                if (statusBarIconMetadata.Type == StatusBarType.Icon)
                {
                    // 创建 StatusBarItem
                    StatusBarItem statusBarItem = new StatusBarItem { ToolTip = statusBarIconMetadata.Description };
                    statusBarItem.DataContext = statusBarIconMetadata.Source;

                    if (statusBarIconMetadata.VisibilityBindingName != null)
                    {
                        var visibilityBinding = new Binding(statusBarIconMetadata.VisibilityBindingName)
                        {
                            Converter = (IValueConverter)Application.Current.FindResource("bool2VisibilityConverter")
                        };
                        statusBarItem.SetBinding(VisibilityProperty, visibilityBinding);
                    }
                    // 设置 MouseLeftButtonDown 事件处理程序
                    if (statusBarIconMetadata.Action != null)
                    {
                        statusBarItem.MouseLeftButtonDown += (s, e) => statusBarIconMetadata.Action.Invoke();
                    }
                    // 创建 ToggleButton
                    ToggleButton toggleButton = new ToggleButton { IsEnabled = false };
                    // 设置 Style 资源
                    if (Application.Current.TryFindResource(statusBarIconMetadata.ButtonStyleName) is Style styleResource)
                        toggleButton.Style = styleResource;

                    // 设置 IsChecked 绑定
                    var isCheckedBinding = new Binding(statusBarIconMetadata.BindingName) { Mode = BindingMode.OneWay };
                    toggleButton.SetBinding(ToggleButton.IsCheckedProperty, isCheckedBinding);
                    statusBarItem.Content = toggleButton;
                    toggleButton.DataContext = statusBarIconMetadata.Source;

                    StatusBarIconDocker.Children.Add(statusBarItem);

                    MenuItem menuItem = new MenuItem() { Header = statusBarIconMetadata.Name };
                    menuItem.Click += (s, e) => menuItem.IsChecked = !menuItem.IsChecked;
                    menuItem.DataContext = statusBarIconMetadata.Source;
                    // 绑定 MenuItem 的 IsChecked 属性到 VisibilityBindingName
                    if (statusBarIconMetadata.VisibilityBindingName != null)
                    {
                        var isCheckedBinding1 = new Binding(statusBarIconMetadata.VisibilityBindingName)
                        {
                            Mode = BindingMode.TwoWay,
                        };
                        menuItem.SetBinding(MenuItem.IsCheckedProperty, isCheckedBinding1);
                    }
                    contextMenu.Items.Add(menuItem);
                }
                else if (statusBarIconMetadata.Type == StatusBarType.Text)
                {
                    StatusBarItem statusBarItem = new StatusBarItem();
                    statusBarItem.DataContext = statusBarIconMetadata.Source;

                    var Binding = new Binding(statusBarIconMetadata.BindingName) { Mode = BindingMode.OneWay };
                    statusBarItem.SetBinding(ContentProperty, Binding);


                    if (statusBarIconMetadata.VisibilityBindingName != null)
                    {
                        var visibilityBinding = new Binding(statusBarIconMetadata.VisibilityBindingName)
                        {
                            Converter = (IValueConverter)Application.Current.FindResource("bool2VisibilityConverter")
                        };
                        statusBarItem.SetBinding(VisibilityProperty, visibilityBinding);
                    }

                    StatusBarTextDocker.Children.Add(statusBarItem);

                    MenuItem menuItem = new MenuItem() { Header = statusBarIconMetadata.Name };
                    menuItem.Click += (s, e) => menuItem.IsChecked = !menuItem.IsChecked;
                    menuItem.DataContext = statusBarIconMetadata.Source;
                    // 绑定 MenuItem 的 IsChecked 属性到 VisibilityBindingName
                    if (statusBarIconMetadata.VisibilityBindingName != null)
                    {
                        var isCheckedBinding = new Binding(statusBarIconMetadata.VisibilityBindingName)
                        {
                            Mode = BindingMode.TwoWay,
                        };
                        menuItem.SetBinding(MenuItem.IsCheckedProperty, isCheckedBinding);
                    }
                    contextMenu.Items.Add(menuItem);
                }

            }

            var allSettings = new List<StatusBarIconMetadata>();

            foreach (var assembly in AssemblyHandler.GetInstance().GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t => typeof(IStatusBarIconProvider).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    if (Activator.CreateInstance(type) is IStatusBarIconProvider configSetting)
                    {
                        allSettings.AddRange(configSetting.GetStatusBarIconMetadata());
                    }
                }
            }
            // 先按 ConfigSettingType 分组，再在每个组内按 Order 排序
            var sortedSettings = allSettings
                .GroupBy(setting => setting.Type)
                .SelectMany(group => group.OrderBy(setting => setting.Order));

            // 将排序后的配置设置添加到集合中
            foreach (var item in sortedSettings)
            {
                AddStatusBarIconMetadata(item);
            }

        }

    }
}
