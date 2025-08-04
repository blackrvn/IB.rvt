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

        // Tab selection properties
        [ObservableProperty]
        private bool _isSheetTabSelected = true;

        [ObservableProperty]
        private bool _isViewTabSelected;

        // The blueprint sequences that the user builds for Sheet and View
        public ObservableCollection<PlaceholderItem> SheetBlueprintElements { get; } = new ObservableCollection<PlaceholderItem>();
        public ObservableCollection<PlaceholderItem> ViewBlueprintElements { get; } = new ObservableCollection<PlaceholderItem>();

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
        private PlaceholderItem _selectedSheetBlueprintElement;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveBlueprintElementCommand))]
        private PlaceholderItem _selectedViewBlueprintElement;

        // Commands
        public IRelayCommand<string> InsertPlaceholderCommand { get; }
        public IRelayCommand<string> InsertStaticTextCommand { get; }
        public IRelayCommand<string> RemoveBlueprintElementCommand { get; }
        public IRelayCommand<string> AddSelectedPlaceholderCommand { get; }

        // Text properties
        [ObservableProperty]
        private string _sheetDelimiter;

        [ObservableProperty]
        private string _viewDelimiter;

        // View numbering options
        [ObservableProperty]
        private bool _useLettersForNumbering = true;

        [ObservableProperty]
        private bool _useNumbersForNumbering;

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

            // Initialize commands with lambda expressions that capture the parameters
            InsertPlaceholderCommand = new RelayCommand<string>((targetTab) => InsertPlaceholder(SelectedPlaceholder, targetTab));
            InsertStaticTextCommand = new RelayCommand<string>((targetTab) => InsertStaticText(targetTab));
            RemoveBlueprintElementCommand = new RelayCommand<string>(RemoveSelectedBlueprintElement, CanRemoveBlueprintElement);
            AddSelectedPlaceholderCommand = new RelayCommand<string>(AddSelectedPlaceholder, CanAddSelectedPlaceholder);

            // Initialize tab selection change handling
            PropertyChanged += TabSelectionChanged;

            // Monitor numbering option changes
            PropertyChanged += NumberingOptionChanged;
        }



        partial void OnUseLettersForNumberingChanged(bool value)
        {
            if (value)
            {
                UseNumbersForNumbering = !value;
            }
        }

        partial void OnUseNumbersForNumberingChanged(bool value)
        {
            if (value)
            {
                UseLettersForNumbering = !value;
            }
        }

        private void NumberingOptionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UseLettersForNumbering) || e.PropertyName == nameof(UseNumbersForNumbering))
            {
                Debug.WriteLine($"Numbering option changed: Letters={UseLettersForNumbering}, Numbers={UseNumbersForNumbering}");
            }
        }

        private void TabSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSheetTabSelected))
            {
                if (IsSheetTabSelected)
                {
                    IsViewTabSelected = false;
                    Debug.WriteLine("Sheet tab selected");
                }
            }
            else if (e.PropertyName == nameof(IsViewTabSelected))
            {
                if (IsViewTabSelected)
                {
                    IsSheetTabSelected = false;
                    Debug.WriteLine("View tab selected");
                }
            }
        }

        public void InsertPlaceholder(PlaceholderItem placeholderItem, string targetTab = null)
        {
            if (placeholderItem != null)
            {
                // Default to the currently selected tab if not specified
                if (string.IsNullOrEmpty(targetTab))
                {
                    targetTab = IsSheetTabSelected ? "Sheet" : "View";
                }

                if (targetTab == "Sheet")
                {
                    SheetBlueprintElements.Add(placeholderItem);
                }
                else
                {
                    ViewBlueprintElements.Add(placeholderItem);
                }
                Debug.WriteLine($"Inserted placeholder: {placeholderItem.ParameterName} into {targetTab} tab");
            }
        }

        public void InsertStaticText(string targetTab, string staticText = "_")
        {
            // In a real application, you might prompt the user for input.
            // Here, we simply insert a fixed static text element.
            if (string.IsNullOrEmpty(targetTab))
            {
                targetTab = IsSheetTabSelected ? "Sheet" : "View";
            }

            if (targetTab == "Sheet")
            {
                SheetBlueprintElements.Add(new PlaceholderItem { ParameterName = staticText, Id = -1 });
            }
            else
            {
                ViewBlueprintElements.Add(new PlaceholderItem { ParameterName = staticText, Id = -1 });
            }
            Debug.WriteLine($"Inserted static text element: {staticText} into {targetTab} tab");
        }

        private bool CanRemoveBlueprintElement(string targetTab)
        {
            if (string.IsNullOrEmpty(targetTab))
            {
                targetTab = IsSheetTabSelected ? "Sheet" : "View";
            }

            return targetTab == "Sheet"
                ? SelectedSheetBlueprintElement != null
                : SelectedViewBlueprintElement != null;
        }

        public void RemoveSelectedBlueprintElement(string targetTab)
        {
            if (string.IsNullOrEmpty(targetTab))
            {
                targetTab = IsSheetTabSelected ? "Sheet" : "View";
            }

            if (targetTab == "Sheet" && SelectedSheetBlueprintElement != null)
            {
                Debug.WriteLine($"Removed element from Sheet: {SelectedSheetBlueprintElement.ParameterName}");
                SheetBlueprintElements.Remove(SelectedSheetBlueprintElement);
            }
            else if (targetTab == "View" && SelectedViewBlueprintElement != null)
            {
                Debug.WriteLine($"Removed element from View: {SelectedViewBlueprintElement.ParameterName}");
                ViewBlueprintElements.Remove(SelectedViewBlueprintElement);
            }
        }

        private bool CanAddSelectedPlaceholder(string targetTab)
        {
            return SelectedPlaceholder != null;
        }

        public void AddSelectedPlaceholder(string targetTab)
        {
            if (SelectedPlaceholder != null)
            {
                InsertPlaceholder(SelectedPlaceholder, targetTab);
            }
        }

        private void CurrentPlaceHolder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPlaceholder))
            {
                Debug.WriteLine(SelectedPlaceholder?.ParameterName ?? "No selection");
                AddSelectedPlaceholderCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(SelectedSheetBlueprintElement))
            {
                Debug.WriteLine(SelectedSheetBlueprintElement?.ParameterName ?? "No Sheet blueprint element selected");
                RemoveBlueprintElementCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(SelectedViewBlueprintElement))
            {
                Debug.WriteLine(SelectedViewBlueprintElement?.ParameterName ?? "No View blueprint element selected");
                RemoveBlueprintElementCommand.NotifyCanExecuteChanged();
            }
        }

        // Public methods to get the formatted naming blueprint for Sheet and View
        public string GetFormattedSheetNaming()
        {
            return FormatBlueprint(SheetBlueprintElements);
        }

        public string GetFormattedViewNaming()
        {
            return FormatBlueprint(ViewBlueprintElements);
        }

        private string FormatBlueprint(IEnumerable<PlaceholderItem> elements)
        {
            if (elements == null || !elements.Any())
                return string.Empty;

            // Use IDs instead of parameter names to avoid issues with delimiters in parameter names
            string result = string.Join(",", elements.Select(e => e.Id.ToString()));
            return result;
        }
    }
}