using SelectSimilar.Views.UserControls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelectSimilar.Views.Behaviors
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
            if (d is SearchBox searchBox)
            {
                searchBox.GotFocus -= RemovePlaceholder;
                searchBox.LostFocus -= ShowPlaceholder;
                searchBox.Loaded -= InitializePlaceholder;

                searchBox.GotFocus += RemovePlaceholder;
                searchBox.LostFocus += ShowPlaceholder;
                searchBox.Loaded += InitializePlaceholder;
            }
        }

        private static void InitializePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is SearchBox searchBox && (string.IsNullOrEmpty(searchBox.SearchText) || string.Equals(searchBox.SearchText, PlaceholderTextProperty)))
            {
                ShowPlaceholder(searchBox, null);
            }
        }

        private static void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is SearchBox searchBox && searchBox.Foreground == Brushes.Silver)
            {
                Debug.WriteLine("Hide placeholder");

                searchBox.SearchText = string.Empty;
                searchBox.Foreground = Brushes.Black;
            }
        }

        private static void ShowPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is SearchBox searchBox && string.IsNullOrEmpty(searchBox.SearchText))
            {
                Debug.WriteLine("Show placeholder");

                searchBox.SearchText = GetPlaceholderText(searchBox);
                searchBox.Foreground = Brushes.Silver;
            }
        }
    }
}
