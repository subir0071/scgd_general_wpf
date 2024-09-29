﻿using ColorVision.Engine.Templates;
using System.Collections.ObjectModel;

namespace ColorVision.Engine.Services.Devices.Algorithm.Templates.FocusPoints
{
    public class TemplateFocusPoints : ITemplate<FocusPointsParam>, IITemplateLoad
    {
        public static ObservableCollection<TemplateModel<FocusPointsParam>> Params { get; set; } = new ObservableCollection<TemplateModel<FocusPointsParam>>();

        public TemplateFocusPoints()
        {
            Title = "FocusPoints算法设置";
            Code = "focusPoints";
            TemplateParams = Params;
        }
    }
}
