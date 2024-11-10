using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Library.Views.UserControls
{
    public partial class SearchBox : UserControl
    {
        public SearchBox()
        {
            InitializeComponent();
        }

        // Dependency Property für SearchText
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set
            {
                SetValue(SearchTextProperty, value);
            }
        }


        // Dependency Property für ClearCommand
        public static readonly DependencyProperty ClearCommandProperty =
            DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(SearchBox), new PropertyMetadata(null));

        public ICommand ClearCommand
        {
            get => (ICommand)GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }
    }
}
