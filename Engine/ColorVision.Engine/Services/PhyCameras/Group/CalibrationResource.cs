﻿using ColorVision.Common.MVVM;
using ColorVision.Common.Utilities;
using ColorVision.Engine.Services.Core;
using ColorVision.Engine.Services.Dao;
using ColorVision.UI.Authorizations;
using ColorVision.UI.Sorts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ColorVision.Engine.Services.PhyCameras.Group
{
    public class CalibrationFileConfig : ViewModelBase
    {
        public double Aperturein { get; set; }
        public double ExpTime { get; set; }
        public double ND { get; set; }
        public double ShotType { get; set; }
        public double Title { get; set; }
        public double Focallength { get; set; }
        public double GetImgMode { get; set; }
    }



    public class CalibrationResource : ServiceFileBase, ISortID, ISortFilePath
    {
        public static List<CalibrationResource> CalibrationResources { get; set; } = new List<CalibrationResource>();

        public static CalibrationResource EnsureInstance(SysResourceModel sysResourceModel)
        {
            var list = CalibrationResources.Find(a => a.SysResourceModel.Id == sysResourceModel.Id);
            if (list != null)
                return list;
            return new CalibrationResource(sysResourceModel);
        }
        public RelayCommand OpenCommand { get; set; }



        public CalibrationFileConfig Config { get; set; }
        public CalibrationResource(SysResourceModel sysResourceModel) : base(sysResourceModel)
        {
            CalibrationResources.Add(this);
            OpenCommand = new RelayCommand(a=> Open(),a => AccessControl.Check(PermissionMode.Administrator));
        }


        public void Open()
        {
            if (this.GetAncestor<PhyCamera>() is PhyCamera phyCamera)
            {
                if (Directory.Exists(phyCamera.Config.FileServerCfg.FileBasePath))
                {
                    string path = SysResourceModel.Value;

                    string filepath = Path.Combine(phyCamera.Config.FileServerCfg.FileBasePath, phyCamera.Code,"cfg", path);

                    PlatformHelper.OpenFolderAndSelectFile(filepath);
                }
            }
        }


        public override void Save()
        {
            SysResourceModel.Remark = JsonConvert.SerializeObject(Config);
            VSysResourceDao.Instance.Save(SysResourceModel);
        }
    }
}
