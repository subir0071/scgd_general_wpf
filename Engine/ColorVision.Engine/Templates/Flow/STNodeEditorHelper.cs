﻿using ColorVision.Common.MVVM;
using ColorVision.Engine.Services.Devices.Calibration;
using ColorVision.Engine.Services.Devices.Camera.Templates.CameraExposure;
using ColorVision.Engine.Services.Devices.Camera;
using ColorVision.Engine.Services.Devices.Sensor.Templates;
using ColorVision.Engine.Services;
using ColorVision.Engine.Templates.DataLoad;
using ColorVision.Engine.Templates.Distortion;
using ColorVision.Engine.Templates.FocusPoints;
using ColorVision.Engine.Templates.FOV;
using ColorVision.Engine.Templates.Ghost;
using ColorVision.Engine.Templates.ImageCropping;
using ColorVision.Engine.Templates.JND;
using ColorVision.Engine.Templates.Jsons.KB;
using ColorVision.Engine.Templates.LedCheck;
using ColorVision.Engine.Templates.LEDStripDetection;
using ColorVision.Engine.Templates.MTF;
using ColorVision.Engine.Templates.POI.BuildPoi;
using ColorVision.Engine.Templates.POI.POIFilters;
using ColorVision.Engine.Templates.POI.POIOutput;
using ColorVision.Engine.Templates.POI.POIRevise;
using ColorVision.Engine.Templates.POI;
using ColorVision.Engine.Templates.ROI;
using ColorVision.Engine.Templates.SFR;
using ColorVision.Engine.Templates.Validate;
using FlowEngineLib.Start;
using ST.Library.UI.NodeEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using ColorVision.Engine.Templates.Jsons;
using System.Windows;
using System.Windows.Media;
using System.Reflection;
using FlowEngineLib.End;
using FlowEngineLib.Base;
using FlowEngineLib.Node.Algorithm;
using FlowEngineLib;
using iText.Commons.Utils;

namespace ColorVision.Engine.Templates.Flow
{

    [STNode("/03_4 Algorithm")]
    public class NodeTest : CVBaseServerNode
    {
        private int _OrderIndex;

        private string _TempName;

        private string _CaliTemplate;

        private string _ImgFileName;

        private STNodeEditText<string> m_ctrl_temp;

        [STNodeProperty("o-index", "Input Order Index", true, false, false)]
        public int OrderIndex
        {
            get
            {
                return _OrderIndex;
            }
            set
            {
                _OrderIndex = value;
            }
        }

        [STNodeProperty("参数模板", "参数模板", true)]
        public string TempName
        {
            get
            {
                return _TempName;
            }
            set
            {
                _TempName = value;
                m_ctrl_temp.Value = value;
            }
        }

        [STNodeProperty("图像文件", "图像文件", true)]
        public string ImgFileName
        {
            get
            {
                return _ImgFileName;
            }
            set
            {
                _ImgFileName = value;
            }
        }

        public NodeTest()
            : base("KB算法", "Algorithm", "SVR.Algorithm.Default", "DEV.Algorithm.Default")
        {
            operatorCode = "KB";
            _CaliTemplate = "";
            _TempName = "";
            _OrderIndex = -1;
        }
        protected override void m_in_start_DataTransfer(object sender, STNodeOptionEventArgs e)
        {
            if (e.Status != ConnectionStatus.Connected || e.TargetOption.Data == null)
            {
                FlowConfig.Instance.IsShowNickName = true;
                m_op_end.TransferData();
            }
            return;
        }
        protected override void DoTransCompleted(CVStartCFC action)
        {
            return;
        }
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ctrl_temp = CreateControl(typeof(STNodeEditText<string>), m_custom_item, "模板:", _TempName);
        }

        protected override object getBaseEventData(CVStartCFC start)
        {
            KBParam kBParam = new KBParam(_TempName, _ImgFileName, GetImageFileType(_ImgFileName), _CaliTemplate);
            getPreStepParam(start, kBParam);
            return kBParam;
        }
    }

    public class STNodeEditorHelper:ViewModelBase
    {
        public STNodeEditor STNodeEditorMain { get; set; }

        public STNodePropertyGrid STNodePropertyGrid1 { get; set; }
        public StackPanel SignStackPannel { get; set; }

        public STNodeTreeView STNodeTreeView1 { get; set; }

        public STNodeEditorHelper(STNodeEditor sTNodeEditor, STNodeTreeView sTNodeTreeView1, STNodePropertyGrid sTNodePropertyGrid, StackPanel stackPanel)
        {
            STNodeEditorMain = sTNodeEditor;
            STNodeTreeView1 = sTNodeTreeView1;
            STNodePropertyGrid1 = sTNodePropertyGrid;
            SignStackPannel = stackPanel;
            STNodeEditorMain.NodeAdded += StNodeEditor1_NodeAdded;
            STNodeEditorMain.ActiveChanged += STNodeEditorMain_ActiveChanged;
            AddContentMenu();
        }

        #region Activate
        private void STNodeEditorMain_ActiveChanged(object? sender, EventArgs e)
        {

            STNodePropertyGrid1.SetNode(STNodeEditorMain.ActiveNode);
            SignStackPannel.Children.Clear();

            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Node.Camera.CommCameraNode commCaeraNode)
            {
                AddStackPanel(name => commCaeraNode.DeviceCode = name, commCaeraNode.DeviceCode, "", ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>().ToList());
                var reuslt = ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>().ToList().Find(a => a.Code == commCaeraNode.DeviceCode);
                AddStackPanel(name => commCaeraNode.CalibTempName = name, commCaeraNode.CalibTempName, "校正", reuslt?.PhyCamera?.CalibrationParams ?? new ObservableCollection<TemplateModel<Services.PhyCameras.Group.CalibrationParam>>());


                // Usage
                AddStackPanel(name => commCaeraNode.TempName = name, commCaeraNode.TempName, "曝光模板", new TemplateCameraExposureParam());
                AddStackPanel(name => commCaeraNode.POITempName = name, commCaeraNode.POITempName, "POI模板", new TemplatePoi());
                AddStackPanel(name => commCaeraNode.POIFilterTempName = name, commCaeraNode.POIFilterTempName, "POI过滤", new TemplatePoiFilterParam());
                AddStackPanel(name => commCaeraNode.POIReviseTempName = name, commCaeraNode.POIReviseTempName, "POI修正", new TemplatePoiReviseParam());
            }

            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Node.Algorithm.AlgorithmKBNode kbnode)
            {
                AddStackPanel(name => kbnode.TempName = name, kbnode.TempName, "KB", new TemplateKB());

            }


            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Algorithm.CalibrationNode calibrationNode)
            {
                AddStackPanel(name => calibrationNode.DeviceCode = name, calibrationNode.DeviceCode, "", ServiceManager.GetInstance().DeviceServices.OfType<DeviceCalibration>().ToList());

                var reuslt = ServiceManager.GetInstance().DeviceServices.OfType<DeviceCalibration>().ToList().Find(a => a.Code == calibrationNode.DeviceCode);
                AddStackPanel(name => calibrationNode.TempName = name, calibrationNode.TempName, "校正", reuslt?.PhyCamera?.CalibrationParams ?? new ObservableCollection<TemplateModel<Services.PhyCameras.Group.CalibrationParam>>());
            }

            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Algorithm.AlgorithmNode algorithmNode)
            {
                void Refesh()
                {
                    SignStackPannel.Children.Clear();

                    switch (algorithmNode.Algorithm)
                    {
                        case FlowEngineLib.Algorithm.AlgorithmType.MTF:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "MTF", new TemplateMTF());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.SFR:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "SFR", new TemplateSFR());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.FOV:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "FOV", new TemplateFOV());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.鬼影:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "鬼影", new TemplateGhost());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.畸变:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "畸变", new TemplateDistortionParam());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.灯珠检测1:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "灯珠检测1", new TemplateLedCheck());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.灯珠检测OLED:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "灯珠检测OLED", new TemplateMTF());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.灯带检测:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "灯带检测", new TemplateLEDStripDetection()); ;
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.发光区检测1:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "发光区检测1", new TemplateFocusPoints());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.发光区检测OLED:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "发光区检测OLED", new TemplateRoi());
                            break;
                        case FlowEngineLib.Algorithm.AlgorithmType.JND:
                            AddStackPanel(name => algorithmNode.TempName = name, algorithmNode.TempName, "JND", new TemplateJND());
                            break;
                        default:
                            break;
                    }
                }
                algorithmNode.nodeEvent -= (s, e) => Refesh();
                algorithmNode.nodeEvent += (s, e) => Refesh();
                Refesh();
            }


            if (STNodeEditorMain.ActiveNode is FlowEngineLib.CVCameraNode cvCameraNode)
            {
                AddStackPanel(name => cvCameraNode.DeviceCode = name, cvCameraNode.DeviceCode, "", ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>().ToList());

                var reuslt = ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>().ToList().Find(a => a.Code == cvCameraNode.DeviceCode);
                AddStackPanel(name => cvCameraNode.CalibTempName = name, cvCameraNode.CalibTempName, "校正", reuslt?.PhyCamera?.CalibrationParams ?? new ObservableCollection<TemplateModel<Services.PhyCameras.Group.CalibrationParam>>());

                AddStackPanel(name => cvCameraNode.POITempName = name, cvCameraNode.POITempName, "POI模板", new TemplatePoi());
                AddStackPanel(name => cvCameraNode.POIFilterTempName = name, cvCameraNode.POIFilterTempName, "POI过滤", new TemplatePoiFilterParam());
                AddStackPanel(name => cvCameraNode.POIReviseTempName = name, cvCameraNode.POIReviseTempName, "POI修正", new TemplatePoiReviseParam());

            }


            if (STNodeEditorMain.ActiveNode is FlowEngineLib.LVCameraNode lcCameranode)
            {
                AddStackPanel(name => lcCameranode.DeviceCode = name, lcCameranode.DeviceCode, "", ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>().ToList());
                var reuslt = ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>().ToList().Find(a => a.Code == lcCameranode.DeviceCode);
                AddStackPanel(name => lcCameranode.CaliTempName = name, lcCameranode.CaliTempName, "校正", reuslt?.PhyCamera?.CalibrationParams ?? new ObservableCollection<TemplateModel<Services.PhyCameras.Group.CalibrationParam>>());

                AddStackPanel(name => lcCameranode.POITempName = name, lcCameranode.POITempName, "POI模板", new TemplatePoi());
                AddStackPanel(name => lcCameranode.POIFilterTempName = name, lcCameranode.POIFilterTempName, "POI过滤", new TemplatePoiFilterParam());
                AddStackPanel(name => lcCameranode.POIReviseTempName = name, lcCameranode.POIReviseTempName, "POI修正", new TemplatePoiReviseParam());

            }

            if (STNodeEditorMain.ActiveNode is FlowEngineLib.BuildPOINode buidpoi)
            {
                AddStackPanel(name => buidpoi.TemplateName = name, buidpoi.TemplateName, "POI模板", new TemplateBuildPoi());
            }

            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Node.Algorithm.AlgDataLoadNode algDataLoadNode)
            {
                AddStackPanel(name => algDataLoadNode.TempName = name, algDataLoadNode.TempName, "模板", new TemplateDataLoad());
            }
            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Node.OLED.OLEDImageCroppingNode OLEDImageCroppingNode)
            {
                AddStackPanel(name => OLEDImageCroppingNode.TempName = name, OLEDImageCroppingNode.TempName, "参数模板", new TemplateImageCropping());
            }
            if (STNodeEditorMain.ActiveNode is FlowEngineLib.POINode poinode)
            {
                AddStackPanel(name => poinode.TemplateName = name, poinode.TemplateName, "POI模板", new TemplatePoi());
                AddStackPanel(name => poinode.FilterTemplateName = name, poinode.FilterTemplateName, "POI过滤", new TemplatePoiFilterParam());
                AddStackPanel(name => poinode.ReviseTemplateName = name, poinode.ReviseTemplateName, "POI修正", new TemplatePoiReviseParam());
                AddStackPanel(name => poinode.OutputTemplateName = name, poinode.OutputTemplateName, "文件输出模板", new TemplatePoiOutputParam());
            }

            if (STNodeEditorMain.ActiveNode is FlowEngineLib.CommonSensorNode commonsendorNode)
            {
                AddStackPanel(name => commonsendorNode.TempName = name, commonsendorNode.TempName, "模板名称", TemplateSensor.AllParams);
            }
            if (STNodeEditorMain.ActiveNode is FlowEngineLib.Node.Algorithm.AlgComplianceMathNode algComplianceMathNode)
            {
                void Refesh()
                {
                    SignStackPannel.Children.Clear();
                    switch (algComplianceMathNode.ComplianceMath)
                    {
                        case FlowEngineLib.Node.Algorithm.ComplianceMathType.CIE:
                            AddStackPanel(name => algComplianceMathNode.TempName = name, algComplianceMathNode.TempName, "CIE", new ObservableCollection<TemplateModel<ValidateParam>>(TemplateComplyParam.CIEParams.SelectMany(p => p.Value)));
                            break;
                        case FlowEngineLib.Node.Algorithm.ComplianceMathType.JND:
                            AddStackPanel(name => algComplianceMathNode.TempName = name, algComplianceMathNode.TempName, "JND", new TemplateComplyParam("Comply.JND"));
                            break;
                        default:
                            break;
                    }
                }
                algComplianceMathNode.nodeEvent -= (s, e) => Refesh();
                algComplianceMathNode.nodeEvent += (s, e) => Refesh();
                Refesh();
            }
        }

        void AddStackPanel<T>(Action<string> updateStorageAction, string tempName, string signName, List<T> itemSource) where T : DeviceService
        {
            DockPanel dockPanel = new DockPanel() { Margin = new Thickness(0, 0, 0, 2) };
            dockPanel.Children.Add(new TextBlock() { Text = signName });

            HandyControl.Controls.ComboBox comboBox = new HandyControl.Controls.ComboBox()
            {
                DisplayMemberPath = "Code",
                Style = (Style)Application.Current.FindResource("ComboBoxPlus.Small")
            };

            HandyControl.Controls.InfoElement.SetShowClearButton(comboBox, true);
            comboBox.ItemsSource = itemSource;
            var selectedItem = itemSource.FirstOrDefault(x => x.Code == tempName);
            if (selectedItem != null)
                comboBox.SelectedIndex = itemSource.IndexOf(selectedItem);

            comboBox.SelectionChanged += (s, e) =>
            {
                string selectedName = string.Empty;

                if (comboBox.SelectedValue is T templateModel)
                {
                    selectedName = templateModel.Code;
                }
                updateStorageAction(selectedName);
                STNodePropertyGrid1.Refresh();
            };

            dockPanel.Children.Add(comboBox);
            SignStackPannel.Children.Add(dockPanel);
        }


        void AddStackPanel<T>(Action<string> updateStorageAction, string tempName, string signName, ObservableCollection<TemplateModel<T>> itemSource) where T : ParamModBase
        {
            DockPanel dockPanel = new DockPanel() { Margin = new Thickness(0, 0, 0, 2) };
            dockPanel.Children.Add(new TextBlock() { Text = signName });

            HandyControl.Controls.ComboBox comboBox = new HandyControl.Controls.ComboBox()
            {
                SelectedValuePath = "Value",
                DisplayMemberPath = "Key",
                Style = (Style)Application.Current.FindResource("ComboBoxPlus.Small")
            };

            HandyControl.Controls.InfoElement.SetShowClearButton(comboBox, true);
            comboBox.ItemsSource = itemSource;
            var selectedItem = itemSource.FirstOrDefault(x => x.Key == tempName);
            if (selectedItem != null)
                comboBox.SelectedIndex = itemSource.IndexOf(selectedItem);

            comboBox.SelectionChanged += (s, e) =>
            {
                string selectedName = string.Empty;

                if (comboBox.SelectedValue is T templateModel)
                {
                    selectedName = templateModel.Name;
                }
                updateStorageAction(selectedName);
                STNodePropertyGrid1.Refresh();
            };

            dockPanel.Children.Add(comboBox);
            SignStackPannel.Children.Add(dockPanel);
        }


        void AddStackPanel<T>(Action<string> updateStorageAction, string tempName, string signName, ITemplateJson<T> template) where T : TemplateJsonParam, new()
        {
            DockPanel dockPanel = new DockPanel() { Margin = new Thickness(0, 0, 0, 2) };
            dockPanel.Children.Add(new TextBlock() { Text = signName, Width = 50 });
            HandyControl.Controls.ComboBox comboBox = new HandyControl.Controls.ComboBox()
            {
                SelectedValuePath = "Value",
                DisplayMemberPath = "Key",
                Style = (Style)Application.Current.FindResource("ComboBoxPlus.Small"),
                Width = 120
            };
            HandyControl.Controls.InfoElement.SetShowClearButton(comboBox, true);
            comboBox.ItemsSource = template.TemplateParams;
            var selectedItem = template.TemplateParams.FirstOrDefault(x => x.Key == tempName);
            if (selectedItem != null)
                comboBox.SelectedIndex = template.TemplateParams.IndexOf(selectedItem);

            comboBox.SelectionChanged += (s, e) =>
            {
                string selectedName = string.Empty;

                if (comboBox.SelectedValue is T templateModel)
                {
                    selectedName = templateModel.Name;
                }
                updateStorageAction(selectedName);
                STNodePropertyGrid1.Refresh();
            };


            Grid grid = new Grid
            {
                Width = 20,
                Margin = new Thickness(5, 0, 0, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };

            // 创建 TextBlock
            TextBlock textBlock = new TextBlock
            {
                Text = "\uE713",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 15,
                Foreground = (Brush)Application.Current.Resources["GlobalTextBrush"]
            };

            // 创建 Button
            Button button = new Button
            {
                Width = 20,
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
            };

            button.Click += (s, e) =>
            {
                new TemplateEditorWindow(template, comboBox.SelectedIndex) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
            };

            // 将控件添加到 Grid
            grid.Children.Add(textBlock);
            grid.Children.Add(button);


            dockPanel.Children.Add(comboBox);
            dockPanel.Children.Add(grid);
            SignStackPannel.Children.Add(dockPanel);
        }

        void AddStackPanel<T>(Action<string> updateStorageAction, string tempName, string signName, ITemplate<T> template) where T : ParamModBase, new()
        {
            DockPanel dockPanel = new DockPanel() { Margin = new Thickness(0, 0, 0, 2) };
            dockPanel.Children.Add(new TextBlock() { Text = signName, Width = 50 });
            HandyControl.Controls.ComboBox comboBox = new HandyControl.Controls.ComboBox()
            {
                SelectedValuePath = "Value",
                DisplayMemberPath = "Key",
                Style = (Style)Application.Current.FindResource("ComboBoxPlus.Small"),
                Width = 120
            };
            HandyControl.Controls.InfoElement.SetShowClearButton(comboBox, true);
            comboBox.ItemsSource = template.TemplateParams;
            var selectedItem = template.TemplateParams.FirstOrDefault(x => x.Key == tempName);
            if (selectedItem != null)
                comboBox.SelectedIndex = template.TemplateParams.IndexOf(selectedItem);

            comboBox.SelectionChanged += (s, e) =>
            {
                string selectedName = string.Empty;

                if (comboBox.SelectedValue is T templateModel)
                {
                    selectedName = templateModel.Name;
                }
                updateStorageAction(selectedName);
                STNodePropertyGrid1.Refresh();
            };


            Grid grid = new Grid
            {
                Width = 20,
                Margin = new Thickness(5, 0, 0, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };

            // 创建 TextBlock
            TextBlock textBlock = new TextBlock
            {
                Text = "\uE713",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 15,
                Foreground = (Brush)Application.Current.Resources["GlobalTextBrush"]
            };

            // 创建 Button
            Button button = new Button
            {
                Width = 20,
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
            };

            button.Click += (s, e) =>
            {
                new TemplateEditorWindow(template, comboBox.SelectedIndex) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
            };

            // 将控件添加到 Grid
            grid.Children.Add(textBlock);
            grid.Children.Add(button);


            dockPanel.Children.Add(comboBox);
            dockPanel.Children.Add(grid);
            SignStackPannel.Children.Add(dockPanel);
        }

        #endregion

        #region ContextMenu
        private void StNodeEditor1_NodeAdded(object sender, STNodeEditorEventArgs e)
        {
            STNode node = e.Node;
            node.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            node.ContextMenuStrip.Items.Add("删除", null, (s, e1) => STNodeEditorMain.Nodes.Remove(node));
        }

        public void AddContentMenu()
        {
            STNodeEditorMain.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            Type STNodeTreeViewtype = STNodeTreeView1.GetType();

            // 获取私有字段信息
            FieldInfo fieldInfo = STNodeTreeViewtype.GetField("m_dic_all_type", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                // 获取字段的值
                var value = fieldInfo.GetValue(STNodeTreeView1);
                Dictionary<string, List<Type>> values = new Dictionary<string, List<Type>>();
                if (value is Dictionary<Type, string> m_dic_all_type)
                {
                    foreach (var item in m_dic_all_type)
                    {
                        if (values.ContainsKey(item.Value))
                        {
                            values[item.Value].Add(item.Key);
                        }
                        else
                        {
                            values.Add(item.Value, new List<Type>() { item.Key });
                        }
                    }

                    foreach (var nodetype in values.OrderBy(x => x.Key, Comparer<string>.Create((x, y) => Common.NativeMethods.Shlwapi.CompareLogical(x, y))))
                    {
                        string header = nodetype.Key.Replace("FlowEngineLib/", "");
                        var toolStripItem = new System.Windows.Forms.ToolStripMenuItem(header);


                        foreach (var type in nodetype.Value)
                        {
                            if (type.IsSubclassOf(typeof(STNode)))
                            {
                                if (Activator.CreateInstance(type) is STNode sTNode)
                                {
                                    toolStripItem.DropDownItems.Add(sTNode.Title, null, (s, e) =>
                                    {
                                        STNode sTNode1 = (STNode)Activator.CreateInstance(type);
                                        if (sTNode1 != null)
                                        {
                                            sTNode1.Create();
                                            var p = STNodeEditorMain.PointToClient(lastMousePosition);
                                            p = STNodeEditorMain.ControlToCanvas(p);
                                            sTNode1.Left = p.X;
                                            sTNode1.Top = p.Y;
                                            STNodeEditorMain.Nodes.Add(sTNode1);
                                        }
                                    });
                                }
                            }

                        }
                        STNodeEditorMain.ContextMenuStrip.Items.Add(toolStripItem);

                    }

                }
            }


            STNodeEditorMain.ContextMenuStrip.Opening += (s, e) =>
            {
                if (IsOptionDisConnected) e.Cancel = true;
                if (IsHover())
                    e.Cancel = true;
                IsOptionDisConnected = false;
            };
            STNodeEditorMain.OptionDisConnected += (s, e) =>
            {
                IsOptionDisConnected = true;
            };
        }
        bool IsOptionDisConnected;


        private System.Drawing.Point lastMousePosition;

        public bool IsHover()
        {
            lastMousePosition = System.Windows.Forms.Cursor.Position;
            var p = STNodeEditorMain.PointToClient(System.Windows.Forms.Cursor.Position);
            p = STNodeEditorMain.ControlToCanvas(p);

            foreach (var item in STNodeEditorMain.Nodes)
            {
                if (item is STNode sTNode)
                {
                    bool result = sTNode.Rectangle.Contains(p);
                    if (result)
                        return true;

                    if (sTNode.GetInputOptions() is STNodeOption[] inputOptions)
                    {
                        foreach (STNodeOption inputOption in inputOptions)
                        {
                            if (inputOption != STNodeOption.Empty && inputOption.DotRectangle.Contains(p))
                            {
                                return true;
                            }
                        }
                    }

                    if (sTNode.GetOutputOptions() is STNodeOption[] outputOptions)
                    {
                        foreach (STNodeOption outputOption in outputOptions)
                        {
                            if (outputOption != STNodeOption.Empty && outputOption.DotRectangle.Contains(p))
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }

        #endregion

        #region AutoLayout
        public ConnectionInfo[] ConnectionInfo { get; set; }
        public float CanvasScale { get => STNodeEditorMain.CanvasScale; set { STNodeEditorMain.ScaleCanvas(value, STNodeEditorMain.CanvasValidBounds.X + STNodeEditorMain.CanvasValidBounds.Width / 2, STNodeEditorMain.CanvasValidBounds.Y + STNodeEditorMain.CanvasValidBounds.Height / 2); NotifyPropertyChanged(); } }
        public void AutoSize()
        {
            // Calculate the centers
            var boundsCenterX = STNodeEditorMain.Bounds.Width / 2;
            var boundsCenterY = STNodeEditorMain.Bounds.Height / 2;

            // Calculate the scale factor to fit CanvasValidBounds within Bounds
            var scaleX = (float)STNodeEditorMain.Bounds.Width / (float)STNodeEditorMain.CanvasValidBounds.Width;
            var scaleY = (float)STNodeEditorMain.Bounds.Height / (float)STNodeEditorMain.CanvasValidBounds.Height;
            CanvasScale = Math.Min(scaleX, scaleY);
            CanvasScale = CanvasScale > 1 ? 1 : CanvasScale;
            // Apply the scale
            STNodeEditorMain.ScaleCanvas(CanvasScale, STNodeEditorMain.CanvasValidBounds.X + STNodeEditorMain.CanvasValidBounds.Width / 2, STNodeEditorMain.CanvasValidBounds.Y + STNodeEditorMain.CanvasValidBounds.Height / 2);

            var validBoundsCenterX = STNodeEditorMain.CanvasValidBounds.Width / 2;
            var validBoundsCenterY = STNodeEditorMain.CanvasValidBounds.Height / 2;

            // Calculate the offsets to move CanvasValidBounds to the center of Bounds
            var offsetX = boundsCenterX - validBoundsCenterX * CanvasScale - 50 * CanvasScale;
            var offsetY = boundsCenterY - validBoundsCenterY * CanvasScale - 50 * CanvasScale;


            // Move the canvas
            STNodeEditorMain.MoveCanvas(offsetX, STNodeEditorMain.CanvasOffset.Y, bAnimation: true, CanvasMoveArgs.Left);
            STNodeEditorMain.MoveCanvas(offsetX, offsetY, bAnimation: true, CanvasMoveArgs.Top);
        }

        public void ApplyTreeLayout(int startX, int startY, int horizontalSpacing, int verticalSpacing)
        {
            ConnectionInfo = STNodeEditorMain.GetConnectionInfo();
            STNode rootNode = GetRootNode();
            int currentY = startY;
            HashSet<STNode> MoreParens = new HashSet<STNode>();

            void LayoutNode(STNode node, int current)
            {
                int depeth = GetMaxDepth(node);
                // 设置当前节点的位置
                node.Left = startX + depeth * horizontalSpacing;
                node.Top = current;

                var parent = GetParent(node);
                // 递归布局子节点
                var children = GetChildren(node);

                foreach (var child in children)
                {
                    if (GetParent(child).Count > 1)
                    {
                        MoreParens.Add(child);
                    }
                    else
                    {
                        LayoutNode(child, currentY);
                        var childrenWithout1 = GetChildrenWithout(node);
                        if (childrenWithout1.Count > 1)
                        {
                            currentY += verticalSpacing;
                        }
                    }
                }
                var childrenWithout = GetChildrenWithout(node);
                if (childrenWithout.Count > 1)
                {
                    currentY = childrenWithout.Last().Top;
                }

                // 调整父节点位置到子节点的中心
                if (childrenWithout.Count != 0)
                {
                    int firstChildY = childrenWithout.First().Top;
                    int lastChildY = childrenWithout.Last().Top;
                    node.Top = (firstChildY + lastChildY) / 2;
                }

                if (parent.Count > 1)
                {
                    int firstChildY = parent.First().Top;
                    int lastChildY = parent.Last().Top;
                    node.Top = (firstChildY + lastChildY) / 2;
                }
            }

            void MoreParentsLayoutNode(STNode node)
            {
                node.Left = startX + GetMaxDepth(node) * horizontalSpacing;
                var parent = GetParent(node);
                // 递归布局子节点
                var children = GetChildren(node);

                int minParentY = parent.Min(c => c.Top);
                int maxParentY = parent.Max(c => c.Top);

                node.Top = (minParentY + maxParentY) / 2;

                SetCof(node, verticalSpacing);
                int currenty = node.Top;
                foreach (var child in children)
                {
                    LayoutNode(child, currenty);
                    currenty += verticalSpacing;
                }
                MoreParens.Remove(node);
            }
            LayoutNode(rootNode, currentY);
            while (MoreParens.Count > 0)
            {
                foreach (var item in MoreParens.Cast<STNode>().ToList())
                {
                    MoreParentsLayoutNode(item);
                }
            }

        }

        public void SetCof(STNode node, int verticalSpacing)
        {
            foreach (var item in STNodeEditorMain.Nodes)
            {
                if (item is STNode onode)
                {
                    if (onode != node && onode.Left == node.Left && onode.Top == node.Top)
                    {
                        onode.Top += verticalSpacing;
                        SetCof(node, verticalSpacing);
                    }
                }
            }
        }


        public int GetMaxDepth(STNode node)
        {
            var parent = GetParent(node);
            if (parent.Count == 0)
            {
                return 0;
            }
            return parent.Max(c => GetMaxDepth(c)) + 1;
        }

        List<STNode> GetParent(STNode node)
        {
            var list = ConnectionInfo.Where(c => c.Input.Owner == node);
            List<STNode> children = new();
            foreach (var item in list)
            {
                children.Add(item.Output.Owner);

            }
            return children;
        }
        List<STNode> GetChildrenWithout(STNode node)
        {
            var list = ConnectionInfo.Where(c => c.Output.Owner == node);
            List<STNode> children = new();
            foreach (var item in list)
            {
                if (GetParent(item.Input.Owner).Count == 1)
                {
                    children.Add(item.Input.Owner);
                }
            }
            return children;
        }

        List<STNode> GetChildren(STNode node)
        {
            var list = ConnectionInfo.Where(c => c.Output.Owner == node);
            List<STNode> children = new();
            foreach (var item in list)
            {
                children.Add(item.Input.Owner);

            }
            return children;
        }

        public STNode GetRootNode()
        {
            foreach (var item in STNodeEditorMain.Nodes)
            {
                if (item is STNode sTNode && sTNode is MQTTStartNode startNode)
                    return startNode;
            }
            return null;
        }

        public bool CheckFlow()
        {
            ConnectionInfo = STNodeEditorMain.GetConnectionInfo();

            bool isContainsMQTTStartNode = false;
            bool isContainsCVEndNode = false;
            STNode startNode = null;
            STNode endNode = null;

            foreach (var item in STNodeEditorMain.Nodes)
            {
                if (item is MQTTStartNode mqttStartNode)
                {
                    isContainsMQTTStartNode = true;
                    startNode = mqttStartNode;
                }
                else if (item is CVEndNode cvEndNode)
                {
                    isContainsCVEndNode = true;
                    endNode = cvEndNode;
                }
            }

            if (!isContainsMQTTStartNode)
            {
                MessageBox.Show(Application.Current.GetActiveWindow(), "找不到流程起始结点");
                return false;
            }

            if (!isContainsCVEndNode)
            {
                MessageBox.Show(Application.Current.GetActiveWindow(), "找不到流程结束结点");
                return false;
            }

            // 检查从起点到终点的路径
            if (!IsPathExists(startNode, endNode))
            {
                MessageBox.Show(Application.Current.GetActiveWindow(), "无法找到从起始结点到结束结点的有效路径");
                return false;
            }
            return true;
        }

        private bool IsPathExists(STNode startNode, STNode endNode)
        {
            var visited = new HashSet<STNode>();
            var queue = new Queue<STNode>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                if (currentNode == endNode)
                {
                    return true;
                }

                visited.Add(currentNode);

                var children = GetChildren(currentNode);
                foreach (var child in children)
                {
                    if (!visited.Contains(child))
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
