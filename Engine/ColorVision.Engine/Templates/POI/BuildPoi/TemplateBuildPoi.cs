﻿using ColorVision.Engine.MySql;
using System.Collections.ObjectModel;

namespace ColorVision.Engine.Templates.POI.BuildPoi
{
    public class TemplateBuildPoi : ITemplate<ParamBuildPoi>, IITemplateLoad
    {
        public static ObservableCollection<TemplateModel<ParamBuildPoi>> Params { get; set; } = new ObservableCollection<TemplateModel<ParamBuildPoi>>();


        public TemplateBuildPoi()
        {
            Title = "Poi布点模板设置";
            Code = "BuildPOI";
            TemplateParams = Params;
        }

        public override IMysqlCommand? GetMysqlCommand() => new MysqlBuildPoi();
    }
}
