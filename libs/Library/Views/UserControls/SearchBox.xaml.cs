using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Library.Interfaces;
using Library.Views.Behaviors;

namespace Library.Views.UserControls
{
    // Now implementing IPlaceholderTextHost
    public partial class SearchBox : UserControl, IPlaceholderTextHost
    {
        public SearchBox()
        {
            InitializeComponent();
        }

        // Dependency Property for SearchText
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        // Dependency Property for ClearCommand
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

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(SearchBox), new PropertyMetadata(new CornerRadius(10)));

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(SearchBox), new PropertyMetadata(Brushes.Gray));

        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(SearchBox), new PropertyMetadata(new Thickness(1)));

        // IPlaceholderTextHost implementation
        // Map Text to SearchText.
        public string Text
        {
            get => SearchText;
            set => SearchText = value;
        }

        // Map Foreground to the inner TextBox's Foreground.
        public new Brush Foreground
        {
            get => PART_TextBox.Foreground;
            set => PART_TextBox.Foreground = value;
        }
    }
}
