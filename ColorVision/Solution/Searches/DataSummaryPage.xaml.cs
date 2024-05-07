﻿using ColorVision.Common.Sorts;
using ColorVision.Common.MVVM;
using ColorVision.Services.DAO;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using ColorVision.Services.RC;

namespace ColorVision.Solution.Searches
{
    public enum ArchiveStatus
    {
        NotArchived = -1,
        Pending = 0,
        Archived = 1,
        Failed = -2
    }

    public class ViewBatchResult : ViewModelBase,ISortID,ISortCreateTime, ISortBatch
    {
        public BatchResultMasterModel BatchResultMasterModel { get; set; }
        public ViewBatchResult(BatchResultMasterModel batchResultMasterModel)
        {
            BatchResultMasterModel = batchResultMasterModel;
            Id = batchResultMasterModel.Id;
            Batch = batchResultMasterModel.Name;
            BatchCode = batchResultMasterModel.Code;
            CreateTime = batchResultMasterModel.CreateDate;
            TotalTime = TimeSpan.FromMilliseconds((double)(batchResultMasterModel.TotalTime??0));
        }

        public int Id { get { return _Id; } set { _Id = value; NotifyPropertyChanged(); } }
        private int _Id;

        public ArchiveStatus ArchiveStatus { get => (ArchiveStatus)BatchResultMasterModel.ArchivedFlag; }

        public bool IshowArch => ArchiveStatus == ArchiveStatus.Pending;


        public string? Batch { get { return _Batch; } set { _Batch = value; NotifyPropertyChanged(); } }
        private string? _Batch;
        public string? BatchCode { get { return _BatchCode; } set { _BatchCode = value; NotifyPropertyChanged(); } }
        private string? _BatchCode;

        public DateTime? CreateTime { get=> _CreateTime;  set { _CreateTime = value; NotifyPropertyChanged(); } }
        private DateTime? _CreateTime;

        public TimeSpan? TotalTime { get => _TotalTime; set { _TotalTime = value; NotifyPropertyChanged(); } }
        private TimeSpan? _TotalTime;
    }

    /// <summary>
    /// DataSummaryPage.xaml 的交互逻辑
    /// </summary>
    public partial class DataSummaryPage : Page
    {
        public Frame Frame { get; set; }

        public DataSummaryPage(Frame MainFrame)
        {
            Frame = MainFrame;
            InitializeComponent();
        }
        BatchResultMasterDao batchResultMasterDao = new BatchResultMasterDao();

        public ObservableCollection<ViewBatchResult> ViewBatchResults { get; set; } = new ObservableCollection<ViewBatchResult>();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewBatchResults.Clear();
            var BatchResultMasterModels = batchResultMasterDao.GetAll();
            foreach (var item in BatchResultMasterModels)
            {
                ViewBatchResults.Add(new ViewBatchResult(item));
            }
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            listView1.ItemsSource = ViewBatchResults;
            if (listView1.View is GridView gridView)
                GridViewColumnVisibility.AddGridViewColumn(gridView.Columns, GridViewColumnVisibilities);
        }
        private void KeyEnter(object sender, KeyEventArgs e)
        {

        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var BatchResultMasterModels = batchResultMasterDao.ConditionalQuery(SearchBox.Text);
            ViewBatchResults.Clear();
            foreach (var item in BatchResultMasterModels)
            {
                ViewBatchResults.AddUnique(new ViewBatchResult(item));
            }
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            var BatchResultMasterModels = batchResultMasterDao.ConditionalQuery(SearchBox.Text);
            ViewBatchResults.Clear();
            foreach (var item in BatchResultMasterModels)
            {
                ViewBatchResults.AddUnique(new ViewBatchResult(item));
            }
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        public ObservableCollection<GridViewColumnVisibility> GridViewColumnVisibilities { get; set; } = new ObservableCollection<GridViewColumnVisibility>();
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu && contextMenu.Items.Count == 0 && listView1.View is GridView gridView)
                 GridViewColumnVisibility.GenContentMenuGridViewColumn(contextMenu, gridView.Columns, GridViewColumnVisibilities);
        }


        private void GridViewColumnSort(object sender, RoutedEventArgs e)
        {
            if (sender is GridViewColumnHeader gridViewColumnHeader && gridViewColumnHeader.Content != null)
            {
                foreach (var item in GridViewColumnVisibilities)
                {
                    if (item.ColumnName.ToString() == gridViewColumnHeader.Content.ToString())
                    {
                        switch (item.ColumnName)
                        {
                            case "序号":
                                item.IsSortD = !item.IsSortD;
                                ViewBatchResults.SortByID(item.IsSortD);
                                break;
                            case "测量时间":
                                item.IsSortD = !item.IsSortD;
                                ViewBatchResults.SortByCreateTime(item.IsSortD);
                                break;
                            case "批次号":
                                item.IsSortD = !item.IsSortD;
                                ViewBatchResults.SortByBatch(item.IsSortD);
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                }
            }

        }
        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedIndex > -1)
            {
                Frame.Navigate(new BatchShowPage(Frame, ViewBatchResults[listView.SelectedIndex]));
            }
        }
        private void Arch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ViewBatchResult viewBatchResult && viewBatchResult.BatchCode !=null)
            {
                MQTTRCService.GetInstance().Archived(viewBatchResult.BatchCode);
                MessageBox.Show("归档指令已经发送");
                Frame.Refresh();
            }
        }
    }
}
