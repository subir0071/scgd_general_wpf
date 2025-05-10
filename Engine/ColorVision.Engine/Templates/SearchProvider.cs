﻿using ColorVision.Common.MVVM;
using ColorVision.Engine.Templates.Flow;
using ColorVision.Engine.Templates.Jsons.KB;
using ColorVision.Engine.Templates.POI;
using ColorVision.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ColorVision.Engine.Templates
{
    public class SearchProvider: ISearchProvider
    {
        public SearchProvider() { }

        public IEnumerable<ISearch> GetSearchItems()
        {
            var templateNames = TemplateControl.ITemplateNames.Values.SelectMany(item => item.GetTemplateNames()).Distinct().ToList();
            foreach (var item in templateNames)
            {
                SearchMeta search = new SearchMeta
                {
                    Type = SearchType.File,
                    Header = item,
                    Command = new RelayCommand(a =>
                    {
                        if (TemplateControl.FindDuplicateTemplate(item) is ITemplate template)
                        {
                            if (template.IsSideHide)
                            {
                                template.PreviewMouseDoubleClick(template.GetTemplateIndex(item));
                            }
                            else
                            {
                                new TemplateEditorWindow(template, template.GetTemplateIndex(item)) { Owner = Application.Current.GetActiveWindow() }.Show();
                            }
                        }
                    })
                };
                yield return search;
            }
        }
    }
}
