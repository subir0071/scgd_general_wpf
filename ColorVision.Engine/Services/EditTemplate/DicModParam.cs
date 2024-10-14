﻿
using ColorVision.Common.MVVM;
using ColorVision.Common.Utilities;
using ColorVision.Engine.MySql;
using ColorVision.Engine.MySql.ORM;
using ColorVision.Engine.Rbac;
using ColorVision.Engine.Templates;
using ColorVision.UI.Authorizations;
using ColorVision.UI.Menus;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ColorVision.Engine.Services.SysDictionary
{


    public class ExportDicModParam : MenuItemBase
    {
        public override string OwnerGuid => "TemplateAlgorithm";

        public override string GuidId => "EditDeaflutAlgrothmParam";
        public override int Order => 99;
        public override string Header => "编辑默认算法模板";

        [RequiresPermission(PermissionMode.Administrator)]
        public override void Execute()
        {
            if (MySqlSetting.Instance.IsUseMySql && !MySqlSetting.IsConnect)
            {
                MessageBox.Show(Application.Current.GetActiveWindow(), "数据库连接失败，请先连接数据库在操作", "ColorVision");
                return;
            }
            new TemplateEditorWindow(new TemplateModParam()) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog(); ;
        }
    }

    public class TemplateModParam : ITemplate<DicModParam>, IITemplateLoad
    {
        public static ObservableCollection<TemplateModel<DicModParam>> Params { get; set; } = new ObservableCollection<TemplateModel<DicModParam>>();

        public TemplateModParam()
        {
            Title = "DicModParam";
            TemplateParams = Params;
            IsUserControl = true;  
        }

        public EditDictionaryMode EditDictionaryMode { get; set; } = new EditDictionaryMode();

        public override UserControl GetUserControl()
        {
            return EditDictionaryMode;
        }
        public override void SetUserControlDataContext(int index)
        {
            EditDictionaryMode.SetParam(TemplateParams[index].Value);
        }

        public override void Delete(int index)
        {
            if (index > -1 && index < TemplateParams.Count)
            {
                var item = TemplateParams[index];
                SysDictionaryModDao.Instance.DeleteById(item.Value.Id,false);
                TemplateParams.RemoveAt(index);
                MenuManager.GetInstance().LoadMenuItemFromAssembly();
            }
        }

        public override void Load()
        {
            var backup = TemplateParams.ToDictionary(tp => tp.Id, tp => tp);

            if (MySqlSetting.Instance.IsUseMySql && MySqlSetting.IsConnect)
            {
                var models = SysDictionaryModDao.Instance.GetAllByParam( new Dictionary<string, object>() { {"tenant_id", UserConfig.Instance.TenantId },{"mod_type",7 } });
                foreach (var model in models)
                {
                    var list = SysDictionaryModDetailDao.Instance.GetAllByPid(model.Id);
                    var t = new DicModParam(model, list);

                    if (backup.TryGetValue(t.Id, out var template))
                    {
                        template.Value = t;
                        template.Key = t.Name;
                    }
                    else
                    {
                        var templateModel = new TemplateModel<DicModParam>(t.Name ?? "default", t);
                        TemplateParams.Add(templateModel);
                    }
                }
            }
        }


        public override void Save()
        {
            if (SaveIndex.Count == 0) return;

            foreach (var index in SaveIndex)
            {
                if (index > -1 && index < TemplateParams.Count)
                {
                    var item = TemplateParams[index];
                    var modMasterModel = SysDictionaryModDao.Instance.GetById(item.Value.Id);

                    foreach (var modDetaiModel in TemplateParams[index].Value.ModDetaiModels)
                    {
                        SysDictionaryModDetailDao.Instance.Save(modDetaiModel);
                    }
                }
            }
        }

        public override void OpenCreate()
        {
            CreateDicTemplate createDicTemplate = new CreateDicTemplate(this) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner };
            createDicTemplate.ShowDialog();
        }

        public override void Create(string templateCode, string templateName)
        {
            SysDictionaryModModel sysDictionaryModModel = new SysDictionaryModModel() { Name = templateName, Code = templateCode, ModType = 7 };
            SysDictionaryModDao.Instance.Save(sysDictionaryModModel);
            var list = SysDictionaryModDetailDao.Instance.GetAllByPid(sysDictionaryModModel.Id);
            var t = new DicModParam(sysDictionaryModModel, list);
            var templateModel = new TemplateModel<DicModParam>(t.Name ?? "default", t);
            TemplateParams.Add(templateModel);
        }
    }



    public class DicModParam : ParamModBase
    {

        public SysDictionaryModModel modMasterModel { get; set; }

        public DicModParam()
        {
            CreateCommand = new RelayCommand(a => new CreateDicModeDetail(this) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog(), a => true);
        }

        public DicModParam(SysDictionaryModModel modMasterModel,List<SysDictionaryModDetaiModel> dicModParams) 
        {
            Id = modMasterModel.Id;
            Name = modMasterModel.Name ??"default";
            ModDetaiModels = new ObservableCollection<SysDictionaryModDetaiModel>(dicModParams);
            CreateCommand = new RelayCommand(a => new CreateDicModeDetail(this) { Owner = Application.Current.GetActiveWindow(), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog(), a => true);
        }

        public ObservableCollection<SysDictionaryModDetaiModel> ModDetaiModels { get; set; }
    };
}
