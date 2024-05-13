﻿using ColorVision.Services.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ColorVision.Services.PhyCameras.Templates
{
    class TempCache
    {
        public string Cache { get; set; }
        public bool DarkNoiseIsSelected { get; set; }
        public bool DefectPointIsSelected { get; set; }
        public bool DSNUIsSelected { get; set; }
        public bool DistortionIsSelected { get; set; }
        public bool ColorShiftIsSelected { get; set; }
        public bool UniformityIsSelected { get; set; }
        public bool LumFourColorIsSelected { get; set; }
        public bool LumOneColorIsSelected { get; set; }
        public bool LumMultiColorIsSelected { get; set; }
        public bool LuminanceIsSelected { get; set; }



    }

    /// <summary>
    /// CalibrationControl.xaml 的交互逻辑
    /// </summary>
    public partial class CalibrationControl : UserControl
    {
        public CalibrationParam CalibrationParam { get => _CalibrationParam; set { _CalibrationParam = value;} }
        private CalibrationParam _CalibrationParam;

        public ICalibrationService<BaseResourceObject> CalibrationService { get; set; }

        public CalibrationControl(ICalibrationService<BaseResourceObject> calibrationService)
        {
            CalibrationService = calibrationService;

            InitializeComponent();
            CalibrationParam = new CalibrationParam();
            DataContext = CalibrationParam;
        }

        public Dictionary<string, List<ZipCalibrationItem>> CalibrationModeList { get; set; }

        public CalibrationControl(ICalibrationService<BaseResourceObject> calibrationService, CalibrationParam calibrationParam)
        {
            CalibrationService = calibrationService;
            InitializeComponent();
            CalibrationParam = calibrationParam;
            DataContext = CalibrationParam;
        }

        public CalibrationParam calibrationParam1 { get; set; }

        public ObservableCollection<GroupResource> groupResources { get; set; } = new ObservableCollection<GroupResource>();
        TempCache TempCache { get; set; } = new TempCache();

        public void Initializedsss(ICalibrationService<BaseResourceObject> calibrationService, CalibrationParam calibrationParam)
        {

            ComboBoxList.SelectionChanged -= ComboBox_SelectionChanged;

            CalibrationService = calibrationService;
            CalibrationParam = calibrationParam;
            DataContext = CalibrationParam;

            TempCache.Cache = calibrationParam.CalibrationMode;
            TempCache.DarkNoiseIsSelected = calibrationParam.Normal.DarkNoise.IsSelected;
            TempCache.DefectPointIsSelected = calibrationParam.Normal.DefectPoint.IsSelected;
            TempCache.DSNUIsSelected = calibrationParam.Normal.DSNU.IsSelected;
            TempCache.DistortionIsSelected = calibrationParam.Normal.Distortion.IsSelected;
            TempCache.ColorShiftIsSelected = calibrationParam.Normal.ColorShift.IsSelected;
            TempCache.UniformityIsSelected = calibrationParam.Normal.Uniformity.IsSelected;
            TempCache.LuminanceIsSelected = calibrationParam.Color.Luminance.IsSelected;
            TempCache.LumFourColorIsSelected = calibrationParam.Color.LumFourColor.IsSelected;
            TempCache.LumMultiColorIsSelected = calibrationParam.Color.LumMultiColor.IsSelected;
            TempCache.LumOneColorIsSelected = calibrationParam.Color.LumOneColor.IsSelected;


            string CalibrationMode = calibrationParam.CalibrationMode;

            ComboBoxList.Text = CalibrationMode;
            ComboBoxList.SelectionChanged += ComboBox_SelectionChanged;
        }


        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            ComboBoxList.ItemsSource = groupResources;
            foreach (var item in CalibrationService.VisualChildren)
            {
                if (item is GroupResource groupResource)
                {
                    groupResource.SetCalibrationResource();
                    groupResources.Add(groupResource);
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NativeMethods.Keyboard.PressKey(0x09);
            }
        }



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                CalibrationParam.Normal.DarkNoise.FilePath = string.Empty;
                CalibrationParam.Normal.DefectPoint.FilePath = string.Empty;
                CalibrationParam.Normal.DSNU.FilePath = string.Empty;
                CalibrationParam.Normal.Distortion.FilePath = string.Empty;
                CalibrationParam.Normal.ColorShift.FilePath = string.Empty;
                CalibrationParam.Normal.Uniformity.FilePath = string.Empty;

                CalibrationParam.Color.Luminance.FilePath = string.Empty;
                CalibrationParam.Color.LumFourColor.FilePath = string.Empty;
                CalibrationParam.Color.LumMultiColor.FilePath = string.Empty;
                CalibrationParam.Color.LumOneColor.FilePath = string.Empty;

                if (comboBox.SelectedValue is GroupResource groupResource)
                {
                    if (groupResource.Name == TempCache.Cache)
                    {
                        CalibrationParam.Normal.DarkNoise.IsSelected = TempCache.DarkNoiseIsSelected;
                        CalibrationParam.Normal.DefectPoint.IsSelected = TempCache.DefectPointIsSelected;
                        CalibrationParam.Normal.DSNU.IsSelected = TempCache.DSNUIsSelected;
                        CalibrationParam.Normal.Distortion.IsSelected = TempCache.DistortionIsSelected;
                        CalibrationParam.Normal.ColorShift.IsSelected = TempCache.ColorShiftIsSelected;
                        CalibrationParam.Normal.Uniformity.IsSelected = TempCache.UniformityIsSelected;

                        CalibrationParam.Color.Luminance.IsSelected = TempCache.LuminanceIsSelected;
                        CalibrationParam.Color.LumFourColor.IsSelected = TempCache.LumFourColorIsSelected;
                        CalibrationParam.Color.LumMultiColor.IsSelected = TempCache.LumMultiColorIsSelected;
                        CalibrationParam.Color.LumOneColor.IsSelected = TempCache.LumOneColorIsSelected;
                    }
                    else
                    {
                        CalibrationParam.Normal.DarkNoise.IsSelected = false;
                        CalibrationParam.Normal.DefectPoint.IsSelected = false;
                        CalibrationParam.Normal.DSNU.IsSelected = false;
                        CalibrationParam.Normal.Distortion.IsSelected = false;
                        CalibrationParam.Normal.ColorShift.IsSelected = false;
                        CalibrationParam.Normal.Uniformity.IsSelected = false;

                        CalibrationParam.Color.Luminance.IsSelected = false;
                        CalibrationParam.Color.LumFourColor.IsSelected = false;
                        CalibrationParam.Color.LumMultiColor.IsSelected = false;
                        CalibrationParam.Color.LumOneColor.IsSelected = false;
                    }



                    CalibrationParam.Normal.DarkNoise.FilePath = groupResource.DarkNoise?.Name ?? string.Empty;
                    CalibrationParam.Normal.DarkNoise.Id = groupResource.DarkNoise?.Id;
                    CalibrationParam.Normal.DefectPoint.FilePath = groupResource.DefectPoint?.Name ?? string.Empty;
                    CalibrationParam.Normal.DefectPoint.Id = groupResource.DefectPoint?.Id;
                    CalibrationParam.Normal.DSNU.FilePath = groupResource.DSNU?.Name ?? string.Empty;
                    CalibrationParam.Normal.DSNU.Id = groupResource.DSNU?.Id;
                    CalibrationParam.Normal.Distortion.FilePath = groupResource.Distortion?.Name ?? string.Empty;
                    CalibrationParam.Normal.Distortion.Id = groupResource.Distortion?.Id;
                    CalibrationParam.Normal.ColorShift.FilePath = groupResource.ColorShift?.Name ?? string.Empty;
                    CalibrationParam.Normal.ColorShift.Id = groupResource.ColorShift?.Id;
                    CalibrationParam.Normal.Uniformity.FilePath = groupResource.Uniformity?.Name ?? string.Empty;
                    CalibrationParam.Normal.Uniformity.Id = groupResource.Uniformity?.Id;
                    CalibrationParam.Color.Luminance.FilePath = groupResource.Luminance?.Name ?? string.Empty;
                    CalibrationParam.Color.Luminance.Id = groupResource.Luminance?.Id;
                    CalibrationParam.Color.LumFourColor.FilePath = groupResource.LumFourColor?.Name ?? string.Empty;
                    CalibrationParam.Color.LumFourColor.Id = groupResource.LumFourColor?.Id;
                    CalibrationParam.Color.LumMultiColor.FilePath = groupResource.LumMultiColor?.Name ?? string.Empty;
                    CalibrationParam.Color.LumMultiColor.Id = groupResource.LumMultiColor?.Id;
                    CalibrationParam.Color.LumOneColor.FilePath = groupResource.LumOneColor?.Name ?? string.Empty;
                    CalibrationParam.Color.LumOneColor.Id = groupResource.LumOneColor?.Id;
                }

            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CalibrationEdit CalibrationEdit = new(CalibrationService);
            CalibrationEdit.Closed += (s, e) =>
            {
                groupResources.Clear();
                foreach (var item in CalibrationService.VisualChildren)
                {
                    if (item is GroupResource groupResource)
                        groupResources.Add(groupResource);
                }
            };
            CalibrationEdit.Show();


        }


    }


}
