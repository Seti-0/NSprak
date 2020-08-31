using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;

namespace NSprakIDE.Themes
{
    public class Alias : MarkupExtension
    {
        public object Resource { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IRootObjectProvider provider = (IRootObjectProvider) serviceProvider.GetService(typeof(IRootObjectProvider));

            switch (provider?.RootObject)
            {
                case FrameworkElement element: return element.TryFindResource(Resource);
                case IDictionary dictionary: return dictionary[Resource];
                default: return null;
            }
        }
    }
}
