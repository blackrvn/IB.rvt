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
            if (e.Data.GetDataPresent(DataFormats.Text) && sender is Border border)
            {
                string droppedText = (string)e.Data.GetData(DataFormats.Text);
                string targetTab = border.Tag?.ToString() ?? "Sheet";

                if (DataContext is RSNamingMenuViewModel vm)
                {
                    vm.InsertPlaceholder(vm.SelectedPlaceholder, targetTab);
                }
            }
        }

        // Handle double-click on placeholders list
        private void PlaceholdersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RSNamingMenuViewModel vm && vm.AddSelectedPlaceholderCommand.CanExecute(null))
            {
                string targetTab = vm.IsSheetTabSelected ? "Sheet" : "View";
                vm.AddSelectedPlaceholderCommand.Execute(targetTab);
            }
        }

        // Handle double-click on blueprint items
        private void BlueprintItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RSNamingMenuViewModel vm && vm.RemoveBlueprintElementCommand.CanExecute(null))
            {
                string targetTab = vm.IsSheetTabSelected ? "Sheet" : "View";
                vm.RemoveBlueprintElementCommand.Execute(targetTab);
            }
        }
    }
}