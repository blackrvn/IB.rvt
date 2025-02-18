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

    public class RSNamingMenuViewModel : ObservableObject
    {
        private readonly RSSettingsModel _model;
        public string Name { get; private set; }

        // The blueprint sequence that the user builds.
        public ObservableCollection<string> BlueprintElements { get; } = new ObservableCollection<string>();

        // Collection of available placeholders (for example, parameters from a room or project).
        public ObservableCollection<PlaceholderItem> AvailablePlaceholders { get; } = new ObservableCollection<PlaceholderItem>();

        // A grouped view of AvailablePlaceholders for display.
        public ICollectionView GroupedPlaceholders { get; }

        // Command that adds a dropped placeholder to the blueprint.
        public IRelayCommand<string> InsertPlaceholderCommand { get; }
        // Command that inserts a static text element into the blueprint.
        public IRelayCommand InsertStaticTextCommand { get; }

        public RSNamingMenuViewModel(RSSettingsModel model)
        {
            _model = model;
            Name = Localization.DialogElements.RoomStudy_NamingMenuHeader;

            ParameterSet parameterSetRooms = _model.GetParametersByBuiltInCategory(BuiltInCategory.OST_Rooms);
            ParameterSet paraneterSetProject = _model.GetParametersByBuiltInCategory(BuiltInCategory.OST_ProjectInformation);

            // Add placeholders from the room and project parameters.
            foreach (Parameter param in parameterSetRooms)
            {
                AvailablePlaceholders.Add(new PlaceholderItem { Category = "Room", ParameterName = param.Definition.Name, Id=param.Id.Value});
            }
            foreach (Parameter param in paraneterSetProject)
            {
                AvailablePlaceholders.Add(new PlaceholderItem { Category = "Project", ParameterName = param.Definition.Name, Id = param.Id.Value });
            }

            // Create a grouped view of the available placeholders by Category.
            GroupedPlaceholders = CollectionViewSource.GetDefaultView(AvailablePlaceholders);
            GroupedPlaceholders.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            // Initialize commands.
            InsertPlaceholderCommand = new RelayCommand<string>(InsertPlaceholder);
            InsertStaticTextCommand = new RelayCommand(InsertStaticText);
        }

        private void InsertPlaceholder(string placeholderText)
        {
            if (!string.IsNullOrWhiteSpace(placeholderText))
            {
                // For example, wrap the placeholder text in square brackets.
                BlueprintElements.Add($"[{placeholderText}]");
                Debug.WriteLine($"Inserted placeholder: {placeholderText}");
            }
        }

        private void InsertStaticText()
        {
            // In a real application, you might prompt the user for input.
            // Here, we simply insert a fixed static text element.
            BlueprintElements.Add("Static Text");
            Debug.WriteLine("Inserted static text element.");
        }
    }
}
