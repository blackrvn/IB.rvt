using RoomStudies.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RoomStudies.Views
{
    public partial class RSNamingMenu : UserControl
    {
        public RSNamingMenu()
        {
            InitializeComponent();
        }

        // Begin drag–drop when a placeholder is moved.
        private void Placeholder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is TextBlock tb)
            {
                DragDrop.DoDragDrop(tb, tb.Text, DragDropEffects.Copy);
            }
        }

        // Allow the blueprint builder area to accept drops.
        private void BlueprintBuilder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        // Forward drop event to the view model command.
        private void BlueprintBuilder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string droppedText = (string)e.Data.GetData(DataFormats.Text);
                if (DataContext is RSNamingMenuViewModel vm &&
                    vm.InsertPlaceholderCommand.CanExecute(droppedText))
                {
                    vm.InsertPlaceholderCommand.Execute(droppedText);
                }
            }
        }

        // Forward the "Insert Static Text" button click to the view model.
        private void InsertStaticText_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSNamingMenuViewModel vm &&
                vm.InsertStaticTextCommand.CanExecute(null))
            {
                vm.InsertStaticTextCommand.Execute(null);
            }
        }
    }
}
