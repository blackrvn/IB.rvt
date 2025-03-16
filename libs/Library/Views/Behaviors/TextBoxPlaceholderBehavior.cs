using Library.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Library.Views.Behaviors
{
    public static class TextBoxPlaceholderBehavior
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(TextBoxPlaceholderBehavior),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static string GetPlaceholderText(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderTextProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Instead of checking for SearchBox, we check for IPlaceholderTextHost.
            if (d is IPlaceholderTextHost && d is FrameworkElement fe)
            {
                // Detach events in case they were previously attached.
                fe.GotFocus -= RemovePlaceholder;
                fe.LostFocus -= ShowPlaceholder;
                fe.Loaded -= InitializePlaceholder;

                // Attach events.
                fe.GotFocus += RemovePlaceholder;
                fe.LostFocus += ShowPlaceholder;
                fe.Loaded += InitializePlaceholder;
            }
        }

        private static void InitializePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is IPlaceholderTextHost host)
            {
                string placeholder = GetPlaceholderText((DependencyObject)sender);
                if (string.IsNullOrEmpty(host.Text) || string.Equals(host.Text, placeholder))
                {
                    ShowPlaceholder(sender, null);
                }
            }
        }

        private static void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is IPlaceholderTextHost host && host.Foreground == Brushes.Silver)
            {
                Debug.WriteLine("Hide placeholder");
                host.Text = string.Empty;
                host.Foreground = Brushes.Black;
            }
        }

        private static void ShowPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is IPlaceholderTextHost host && string.IsNullOrEmpty(host.Text))
            {
                Debug.WriteLine("Show placeholder");
                host.Text = GetPlaceholderText((DependencyObject)sender);
                host.Foreground = Brushes.Silver;
            }
        }
    }
}
