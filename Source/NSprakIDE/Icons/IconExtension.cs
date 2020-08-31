using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace NSprakIDE.Icons
{
    public class IconExtension : MarkupExtension
    {
        public string IconName { get; set; }

        public bool Large { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string size = Large ? "30" : "16";

            Uri uri = new Uri($"pack://application:,,,/NSprakIDE;component/Icons/Images/icons8-{IconName}-{size}.png");

            return new BitmapImage(uri);
        }
    }
}
