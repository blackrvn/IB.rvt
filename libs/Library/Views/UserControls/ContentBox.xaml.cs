using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Library.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ContentBox.xaml
    /// </summary>
    public partial class ContentBox : UserControl
    {
        public ContentBox()
        {
            InitializeComponent();
        }

        public IEnumerable ContentCollection
        {
            get { return (IEnumerable)GetValue(ContentCollectionProperty); }
            set { SetValue(ContentCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentCollectionProperty =
            DependencyProperty.Register("ContentCollection", typeof(IEnumerable), typeof(ContentBox));



        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ContentBox), new PropertyMetadata("test"));



        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ContentBox), new PropertyMetadata(new CornerRadius(10)));



        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderBrush.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ContentBox), new PropertyMetadata(Brushes.Gray));



        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderThickness.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(ContentBox), new PropertyMetadata(new Thickness(1)));


        public Object SelectedItem
        {
            get { return (Object)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItem", typeof(Object), typeof(ContentBox));


    }
}
