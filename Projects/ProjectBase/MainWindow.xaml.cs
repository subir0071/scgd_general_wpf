﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectBase
{
    /// <summary>
    /// Interaction logic for MarkdownViewWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.DataContext = ProjectBaseConfig.Instance;
        }



        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listView1_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GridViewColumnSort(object sender, RoutedEventArgs e)
        {

        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void SNtextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GridSplitter_DragCompleted1(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {

        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void TestClick(object sender, RoutedEventArgs e)
        {

        }
    }
}