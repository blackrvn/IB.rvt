using Localization;
using RoomStudies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace RoomStudies.ViewModels
{
    public class PlaceholderItem
    {
        public string Category { get; set; }
        public string ParameterName { get; set; }
        public long Id { get; set; }
    }

    public partial class RSNamingMenuViewModel : ObservableObject
    {
        private readonly RSSettingsModel _model;
        public string Name { get; private set; }

        // The blueprint sequence that the user builds.
        public ObservableCollection<PlaceholderItem> BlueprintElements { get; } = new ObservableCollection<PlaceholderItem>();

        // Collection of available placeholders (for example, parameters from a room or project).
        public ObservableCollection<PlaceholderItem> AvailablePlaceholders { get; } = new ObservableCollection<PlaceholderItem>();

        // A grouped view of AvailablePlaceholders for display.
        public ICollectionView GroupedPlaceholders { get; }

        // Selected items properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddSelectedPlaceholderCommand))]
        private PlaceholderItem _selectedPlaceholder;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveBlueprintElementCommand))]
        private PlaceholderItem _selectedBlueprintElement;

        // Commands
        public IRelayCommand<PlaceholderItem> InsertPlaceholderCommand { get; }
        public IRelayCommand InsertStaticTextCommand { get; }
        public IRelayCommand RemoveBlueprintElementCommand { get; }
        public IRelayCommand AddSelectedPlaceholderCommand { get; }

        // Text
        [ObservableProperty]
        private string _delimiter;

        public RSNamingMenuViewModel(RSSettingsModel model)
        {
            _model = model;
            Name = Localization.DialogElements.RoomStudy_NamingMenuHeader;

            ParameterSet parameterSetRooms = _model.GetParametersByBuiltInCategory(BuiltInCategory.OST_Rooms);
            ParameterSet paraneterSetProject = _model.GetParametersByBuiltInCategory(BuiltInCategory.OST_ProjectInformation);

            // Add placeholders from the room and project parameters.
            foreach (Parameter param in parameterSetRooms)
            {
                AvailablePlaceholders.Add(new PlaceholderItem { Category = "Room", ParameterName = param.Definition.Name, Id = param.Id.Value });
            }
            foreach (Parameter param in paraneterSetProject)
            {
                AvailablePlaceholders.Add(new PlaceholderItem { Category = "Project", ParameterName = param.Definition.Name, Id = param.Id.Value });
            }

            PropertyChanged += CurrentPlaceHolder_PropertyChanged;

            // Create a grouped view of the available placeholders by Category.
            GroupedPlaceholders = CollectionViewSource.GetDefaultView(AvailablePlaceholders);
            GroupedPlaceholders.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            // Initialize commands.
            InsertPlaceholderCommand = new RelayCommand<PlaceholderItem>(InsertPlaceholder);
            InsertStaticTextCommand = new RelayCommand<string>(InsertStaticText);
            RemoveBlueprintElementCommand = new RelayCommand(RemoveSelectedBlueprintElement, CanRemoveBlueprintElement);
            AddSelectedPlaceholderCommand = new RelayCommand(AddSelectedPlaceholder, CanAddSelectedPlaceholder);
        }

        public void InsertPlaceholder(PlaceholderItem placeholderItem)
        {
            if (placeholderItem != null)
            {
                BlueprintElements.Add(placeholderItem);
                Debug.WriteLine($"Inserted placeholder: {placeholderItem.ParameterName}");
            }
        }

        public void InsertStaticText(string staticText = "_")
        {
            // In a real application, you might prompt the user for input.
            // Here, we simply insert a fixed static text element.
            BlueprintElements.Add(new PlaceholderItem { ParameterName = staticText, Id = -1 });
            Debug.WriteLine($"Inserted static text element: {staticText}");
        }

        private bool CanRemoveBlueprintElement()
        {
            return SelectedBlueprintElement != null;
        }

        public void RemoveSelectedBlueprintElement()
        {
            if (SelectedBlueprintElement != null)
            {
                BlueprintElements.Remove(SelectedBlueprintElement);
                Debug.WriteLine($"Removed element: {SelectedBlueprintElement}");
            }
        }

        private bool CanAddSelectedPlaceholder()
        {
            return SelectedPlaceholder != null;
        }

        public void AddSelectedPlaceholder()
        {
            if (SelectedPlaceholder != null)
            {
                InsertPlaceholder(SelectedPlaceholder);
            }
        }

        private void CurrentPlaceHolder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPlaceholder))
            {
                Debug.WriteLine(SelectedPlaceholder?.ParameterName ?? "No selection");
                AddSelectedPlaceholderCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(SelectedBlueprintElement))
            {
                Debug.WriteLine(SelectedBlueprintElement?.ParameterName ?? "No blueprint element selected");
                RemoveBlueprintElementCommand.NotifyCanExecuteChanged();
            }
        }
    }
}