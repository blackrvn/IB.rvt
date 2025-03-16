using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Library.Interfaces;
using Library.Views.Behaviors;

namespace Library.Views.UserControls
{
    // Now implementing IPlaceholderTextHost
    public partial class RoundedTextBox : UserControl, IPlaceholderTextHost
    {
        public RoundedTextBox()
        {
            InitializeComponent();
        }

        // Text Dependency Property
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(RoundedTextBox),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // CornerRadius Dependency Property
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(RoundedTextBox),
                new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // BorderBrush Dependency Property
        public new static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(
                "BorderBrush",
                typeof(Brush),
                typeof(RoundedTextBox),
                new PropertyMetadata(Brushes.Black));

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // IPlaceholderTextHost implementation
        // For Text, we already have the dependency property.
        // Now implement Foreground by mapping to the inner TextBox.
        public new Brush Foreground
        {
            get => MainTextBox.Foreground;
            set => MainTextBox.Foreground = value;
        }
    }
}
