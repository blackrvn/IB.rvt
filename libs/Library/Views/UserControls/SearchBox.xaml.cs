using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(SearchBox), new PropertyMetadata(new CornerRadius(10)));



        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderBrush.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(SearchBox), new PropertyMetadata(Brushes.Gray));



        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderThickness.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(SearchBox), new PropertyMetadata(new Thickness(1)));
    }
}
