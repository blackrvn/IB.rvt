using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class MenuControl : UserControl
    {
        public MenuControl()
        {
            InitializeComponent();
        }

        #region MenuItems DependencyProperty

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(
                "MenuItems",
                typeof(ICollection<object>),
                typeof(MenuControl),
                new PropertyMetadata(null));

        public ICollection<object> MenuItems
        {
            get => (ICollection<object>)GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }

        #endregion

        #region CurrentSelection DependencyProperty

        public static readonly DependencyProperty CurrentSelectionProperty =
            DependencyProperty.Register(
                "CurrentSelection",
                typeof(object),
                typeof(MenuControl),
                new PropertyMetadata(null));

        public object CurrentSelection
        {
            get => GetValue(CurrentSelectionProperty);
            set => SetValue(CurrentSelectionProperty, value);
        }

        #endregion

        #region CornerRadius DependencyProperty

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(MenuControl),
                new PropertyMetadata(new CornerRadius(5)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region Spacing DependencyProperty

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                "Spacing",
                typeof(Thickness),
                typeof(MenuControl),
                new PropertyMetadata(new Thickness(5)));

        public Thickness Spacing
        {
            get => (Thickness)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        #endregion
    }
}
