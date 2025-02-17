using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Library.Views.UserControls
{
    /// <summary>
    /// Interaction logic for HeaderedDropDown.xaml
    /// </summary>
    public partial class HeaderedDropDown : UserControl
    {
        public HeaderedDropDown()
        {
            InitializeComponent();
        }

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderedDropDown), new PropertyMetadata(""));

        // Alle Items (falls du sie brauchst)
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(HeaderedDropDown), new PropertyMetadata(null));

        // Bereits gefilterte Items, die vom VM kommen
        public IEnumerable FilteredItems
        {
            get => (IEnumerable)GetValue(FilteredItemsProperty);
            set => SetValue(FilteredItemsProperty, value);
        }
        public static readonly DependencyProperty FilteredItemsProperty =
            DependencyProperty.Register(nameof(FilteredItems), typeof(IEnumerable), typeof(HeaderedDropDown), new PropertyMetadata(null));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(HeaderedDropDown),
                new PropertyMetadata(null));

        // Text für die Suche, VM kümmert sich um Filterung
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(HeaderedDropDown), new PropertyMetadata(""));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(HeaderedDropDown), new PropertyMetadata(false));


        // CORNERRADIUS
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HeaderedDropDown),
                new PropertyMetadata(new CornerRadius(5)));



        public string SearchBoxPlaceHolder
        {
            get { return (string)GetValue(SearchBoxPlaceHolderProperty); }
            set { SetValue(SearchBoxPlaceHolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchBoxPlaceHolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchBoxPlaceHolderProperty =
            DependencyProperty.Register(nameof(SearchBoxPlaceHolder), typeof(string), typeof(HeaderedDropDown), new PropertyMetadata(string.Empty));


        /// <summary>
        /// Gets or sets the widths for each column in the LayoutGrid.
        /// </summary>
        public IList<GridLength> ColumnWidths
        {
            get => (IList<GridLength>)GetValue(ColumnWidthsProperty);
            set => SetValue(ColumnWidthsProperty, value);
        }

        public static readonly DependencyProperty ColumnWidthsProperty =
            DependencyProperty.Register(
                nameof(ColumnWidths),
                typeof(IList<GridLength>),
                typeof(HeaderedDropDown),
                new PropertyMetadata(null, OnColumnWidthsChanged));

        private static void OnColumnWidthsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HeaderedDropDown control)
            {
                control.ApplyColumnWidths();
            }
        }

        private void ApplyColumnWidths()
        {
            // Check that the property is set and our grid exists.
            if (ColumnWidths == null || LayoutGrid == null)
                return;

            // Loop through the grid's columns and assign widths from the collection.
            // This assumes that the number of items in ColumnWidths matches or exceeds
            // the number of ColumnDefinitions.
            for (int i = 0; i < LayoutGrid.ColumnDefinitions.Count && i < ColumnWidths.Count; i++)
            {
                LayoutGrid.ColumnDefinitions[i].Width = ColumnWidths[i];
            }
        }


    }

}
