﻿using ColorVision.Common.MVVM;
using ColorVision.Common.Utilities;
using ColorVision.Engine.MySql;
using ColorVision.Engine.MySql.ORM;
using ColorVision.Engine.Services.Core;
using ColorVision.Engine.Services.Dao;
using ColorVision.Engine.Services.Devices.Camera;
using ColorVision.Engine.Services.PhyCameras.Configs;
using ColorVision.Engine.Services.PhyCameras.Dao;
using ColorVision.Engine.Services.PhyCameras.Group;
using ColorVision.Engine.Services.RC;
using ColorVision.Engine.Services.Types;
using cvColorVision;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColorVision.Engine.Services.PhyCameras
{
    public class PhyCameraManager:ViewModelBase
    {
        private static PhyCameraManager _instance;
        private static readonly object Locker = new();
        public static PhyCameraManager GetInstance() { lock (Locker) { return _instance ??= new PhyCameraManager(); } }
        public RelayCommand CreateCommand { get; set; }

        public RelayCommand ImportCommand { get; set; }

        public PhyCameraManager()
        {
            CreateCommand = new RelayCommand(a => Create());
            ImportCommand = new RelayCommand(a => Import());
            MySqlControl.GetInstance().MySqlConnectChanged += (s, e) => LoadPhyCamera();
            if (MySqlControl.GetInstance().IsConnect)
                LoadPhyCamera();
            RefreshEmptyCamera();
            PhyCameras.CollectionChanged += (s, e) => RefreshEmptyCamera();
        }



        public void RefreshEmptyCamera()
        {
            Count = SysResourceDao.Instance.GetAllEmptyCameraId().Count;
        }


        public int Count { get => _Count; set { _Count = value; NotifyPropertyChanged(); } }
        private int _Count;


        public PhyCamera? GetPhyCamera(string? Code) => PhyCameras.FirstOrDefault(a => a.Code == Code);

        public void Create()
        {
            if (SysResourceDao.Instance.GetAllEmptyCameraId().Count <= 0)
            {
                MessageBox.Show(Application.Current.GetActiveWindow(), "找不到未创建的相机,请插上相机后在尝试",nameof(PhyCameraManager));
                foreach (var item in ServiceManager.GetInstance().DeviceServices.OfType<DeviceCamera>())
                {
                    item.RefreshDeviceId();
                }
                return;
            }

            var createWindow = new CreateWindow(this)
            {
                Owner = Application.Current.GetActiveWindow(),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            createWindow.ShowDialog();
        }

        public void Import()
        {
            using var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                RestoreDirectory = true,
                Multiselect = true,
                Filter = "All files (*.*)|*.zip;*.lic",
                Title = "请选择许可证文件",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] selectedFiles = openFileDialog.FileNames;
                var licenses = CameraLicenseDao.Instance.GetAll();

                foreach (string file in selectedFiles)
                {
                    try
                    {
                        if (Path.GetExtension(file).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessZipFile(file, licenses);
                        }
                        else if (Path.GetExtension(file).Equals(".lic", StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessLicFile(file, licenses);
                        }
                        else
                        {
                            MessageBox.Show(WindowHelpers.GetActiveWindow(), "不支持的许可文件后缀", "ColorVision");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(WindowHelpers.GetActiveWindow(), $"处理文件失败 :{ex.Message}", "ColorVision");
                    }
                }
            }
            LoadPhyCamera();
        }

        private static void ProcessZipFile(string file, List<CameraLicenseModel> licenses)
        {
            using ZipArchive archive = ZipFile.OpenRead(file);
            var licFiles = archive.Entries.Where(entry => Path.GetExtension(entry.FullName).Equals(".lic", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var item in licFiles)
            {
                var licenseModel = GetOrCreateLicenseModel(Path.GetFileNameWithoutExtension(item.FullName), licenses);
                using var stream = item.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                licenseModel.LicenseValue = reader.ReadToEnd();

                UpdateLicenseModel(licenseModel);
            }
        }

        private static void ProcessLicFile(string file, List<CameraLicenseModel> licenses)
        {
            var licenseModel = GetOrCreateLicenseModel(Path.GetFileNameWithoutExtension(file), licenses);
            licenseModel.LicenseValue = File.ReadAllText(file);

            UpdateLicenseModel(licenseModel);
        }

        private static CameraLicenseModel GetOrCreateLicenseModel(string macAddress, List<CameraLicenseModel> licenses)
        {
            var licenseModel = licenses.Find(a => a.MacAddress == macAddress) ?? new CameraLicenseModel { MacAddress = macAddress };
            return licenseModel;
        }

        private static void UpdateLicenseModel(CameraLicenseModel licenseModel)
        {
            licenseModel.CusTomerName = licenseModel.ColorVisionLicense.Licensee;
            licenseModel.Model = licenseModel.ColorVisionLicense.DeviceMode;
            licenseModel.ExpiryDate = licenseModel.ColorVisionLicense.ExpiryDateTime;

            int ret = CameraLicenseDao.Instance.Save(licenseModel);

            UpdateSysResource(licenseModel);
        }

        private static void UpdateSysResource(CameraLicenseModel licenseModel)
        {
            var sysDictionaryModel = SysResourceDao.Instance.GetAll().Find(a => a.Code == licenseModel.MacAddress);
            if (sysDictionaryModel == null)
            {
                sysDictionaryModel = new SysResourceModel
                {
                    Code = licenseModel.MacAddress,
                    Type = (int)ServiceTypes.PhyCamera,
                    Value = JsonConvert.SerializeObject(CreateDefaultConfig())
                };

                int ret = SysResourceDao.Instance.Save(sysDictionaryModel);
                if(ret != -1 && sysDictionaryModel.Code !=null)
                {
                    RCFileUpload.GetInstance().CreatePhysicalCameraFloder(sysDictionaryModel.Code);
                }
                MessageBox.Show(WindowHelpers.GetActiveWindow(), $"{licenseModel.MacAddress} {(ret == -1 ? "添加物理相机失败" : "添加物理相机成功")}", "ColorVision");
            }
            else
            {
                sysDictionaryModel.Value = JsonConvert.SerializeObject(CreateDefaultConfig());
                int ret= SysResourceDao.Instance.Save(sysDictionaryModel);
                if (ret != -1 && sysDictionaryModel.Code != null)
                {
                    RCFileUpload.GetInstance().CreatePhysicalCameraFloder(sysDictionaryModel.Code);
                }
            }
        }

        private static ConfigPhyCamera CreateDefaultConfig()
        {
            return new ConfigPhyCamera
            {
                CameraType = CameraType.LV_Q,
                TakeImageMode = TakeImageMode.Measure_Normal,
                ImageBpp = ImageBpp.bpp8,
                Channel = ImageChannel.One,
            };
        }

        public EventHandler Loaded { get; set; }

        public ObservableCollection<PhyCamera> PhyCameras { get; set; } = new ObservableCollection<PhyCamera>();

        public void LoadPhyCamera()
        {
            var phyCameraBackup = PhyCameras.ToDictionary(pc => pc.Id, pc => pc);
                
            var list = SysResourceDao.Instance.GetAllType((int)ServiceTypes.PhyCamera);
            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.Value))
                {
                    // 创建新的 PhyCamera 对象

                    // 如果备份字典中存在该 PhyCamera 的 Id
                    if (phyCameraBackup.TryGetValue(item.Id, out var existingPhyCamera))
                    {
                        existingPhyCamera.Name = item.Name ?? string.Empty;
                        existingPhyCamera.SysResourceModel = item;
                        existingPhyCamera.Config.CameraID = item.Name ?? string.Empty;
                    }
                    else
                    {
                        var newPhyCamera = new PhyCamera(item);
                        if (!newPhyCamera.IsLicensed)
                            Task.Run(() => newPhyCamera.UploadLicenseNet());
                        LoadPhyCameraResources(newPhyCamera);
                        // 添加新的 PhyCamera 对象到集合中
                        PhyCameras.Add(newPhyCamera);
                    }
                }
            }

            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private static void LoadPhyCameraResources(PhyCamera phyCamera)
        {
            var sysResourceModels = SysResourceDao.Instance.GetResourceItems(phyCamera.SysResourceModel.Id);
            foreach (var sysResourceModel in sysResourceModels)
            {
                switch (sysResourceModel.Type)
                {
                    case (int)ServiceTypes.Group:
                        var groupResource = new GroupResource(sysResourceModel);
                        phyCamera.AddChild(groupResource);
                        LoadGroupResource(groupResource);
                        break;
                    case >= 30 and <= 40:
                        var calibrationResource = CalibrationResource.EnsureInstance(sysResourceModel);
                        phyCamera.AddChild(calibrationResource);
                        break;
                    default:
                        var baseFileResource = new ServiceFileBase(sysResourceModel);
                        phyCamera.AddChild(baseFileResource);
                        break;
                }
            }
        }

        public static void LoadGroupResource(GroupResource groupResource)
        {
            var sysResourceModels = SysResourceDao.Instance.GetGroupResourceItems(groupResource.SysResourceModel.Id);
            foreach (var sysResourceModel in sysResourceModels)
            {
                switch (sysResourceModel.Type)
                {
                    case (int)ServiceTypes.Group:
                        var nestedGroupResource = new GroupResource(sysResourceModel);
                        LoadGroupResource(nestedGroupResource);
                        groupResource.AddChild(nestedGroupResource);
                        break;
                    case >= 30 and <= 40:
                        var calibrationResource = CalibrationResource.EnsureInstance(sysResourceModel);
                        groupResource.AddChild(calibrationResource);
                        break;
                    default:
                        var baseResource = new ServiceBase(sysResourceModel);
                        groupResource.AddChild(baseResource);
                        break;
                }
            }
            groupResource.SetCalibrationResource();
        }
    }
}
