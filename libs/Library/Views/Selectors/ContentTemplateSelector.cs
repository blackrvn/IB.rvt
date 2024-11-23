using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Library.Views.Selectors
{
    public class ContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CheckBoxTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item.GetType().Name == "CheckBoxItem")
            {
                return CheckBoxTemplate;
            }
            return TextTemplate;
        }
    }
}
