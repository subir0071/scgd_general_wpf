﻿using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ColorVision.Engine.Templates.Jsons.LedCheck2
{
    public class TemplateLedCheck2 : ITemplateJson<TemplateJsonParam>
    {
        public static ObservableCollection<TemplateModel<TemplateJsonParam>> Params { get; set; } = new ObservableCollection<TemplateModel<TemplateJsonParam>>();

        public TemplateLedCheck2()
        {
            Title = "灯珠检测2";
            Code = "LedCheck2";
            TemplateParams = Params;
            IsUserControl = true;
        }

        public override void SetUserControlDataContext(int index)
        {
            EditTemplateJson.SetParam(TemplateParams[index].Value);
        }
        public EditTemplateJson EditTemplateJson { get; set; } = new EditTemplateJson();

        public override UserControl GetUserControl()
        {
            return EditTemplateJson;
        }

    }




}
