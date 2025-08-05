using Library.Views.UserControls;
using Localization;
using RoomStudies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace RoomStudies.ViewModels
{
    public class BluePrintItem
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
        public ObservableCollection<BluePrintItem> CurrentSheetBlueprint { get; } = new ObservableCollection<BluePrintItem>(); // Blueprint elements for sheet selected by user
        public ObservableCollection<BluePrintItem> CurrentViewBlueprint { get; } = new ObservableCollection<BluePrintItem>(); // Blueprint elements for view selected by user

        // Collection of available placeholders (for example, parameters from a room or project).
        public ObservableCollection<BluePrintItem> AvailableBluePrintsSheet { get; } = new ObservableCollection<BluePrintItem>();
        public ObservableCollection<BluePrintItem> AvailableBluePrintsView { get; } = new ObservableCollection<BluePrintItem>();


        // A grouped view of AvailableBluePrintsSheet for display.
        public ICollectionView GroupedBlueprintSheet { get; set; }
        public ICollectionView GroupedBlueprintView { get; set; }


        // Selected items properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddSelectedBlueprintCommand))]
        private BluePrintItem _selectedAvailableBlueprint; // The currently selected blueprint item from the available placeholders

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedBlueprintCommand))]
        private BluePrintItem _selectedSheetBlueprintElement; // The currently selected blueprint item from the current sheet blueprint

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedBlueprintCommand))]
        private BluePrintItem _selectedViewBlueprintElement; // The currently selected blueprint item from the current view blueprint

        // Commands
        public IRelayCommand<string> InsertPlaceholderCommand { get; set; }
        public IRelayCommand<string> RemoveSelectedBlueprintCommand { get; set; }
        public IRelayCommand<string> AddSelectedBlueprintCommand { get; set; }
        public IRelayCommand<object> ClearSearchBoxCommand { get; set; }

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

        // SearchTexts
        [ObservableProperty]
        private string _searchTextViews;
        [ObservableProperty]
        private string _searchTextSheets;
        [ObservableProperty]
        private string _placeHolder;


        public RSNamingMenuViewModel(RSSettingsModel model)
        {
            _model = model;
            Name = Localization.DialogElements.RoomStudy_NamingMenuHeader;
            PlaceHolder = Localization.DialogElements.General_Search;

            PropertyChanged += CurrentBluePrint_PropertyChanged;
            PropertyChanged += SearchBox_PropertyChanged;
            PropertyChanged += TabSelectionChanged;
            PropertyChanged += NumberingOptionChanged;

            InitializeCollections();
            InitializeFiltering();
            InitializeCommands();

        }

        public void GatherParameters()
        {
            ParameterSet parameterSetRooms = _model.GetParametersByBuiltInCategory(BuiltInCategory.OST_Rooms);
            ParameterSet paraneterSetProject = _model.GetParametersByBuiltInCategory(BuiltInCategory.OST_ProjectInformation);
            // Add placeholders from the room and project parameters.
            foreach (Parameter param in parameterSetRooms)
            {
                AvailableBluePrintsSheet.Add(new BluePrintItem { Category = "Room", ParameterName = param.Definition.Name, Id = param.Id.Value });
                AvailableBluePrintsView.Add(new BluePrintItem { Category = "Room", ParameterName = param.Definition.Name, Id = param.Id.Value });

            }
            foreach (Parameter param in paraneterSetProject)
            {
                AvailableBluePrintsSheet.Add(new BluePrintItem { Category = "Project", ParameterName = param.Definition.Name, Id = param.Id.Value });
                AvailableBluePrintsView.Add(new BluePrintItem { Category = "Project", ParameterName = param.Definition.Name, Id = param.Id.Value });
            }
        }

        private void InitializeCollections()
        {
            GatherParameters();

            GroupedBlueprintSheet = CollectionViewSource.GetDefaultView(AvailableBluePrintsSheet);
            GroupedBlueprintView = CollectionViewSource.GetDefaultView(AvailableBluePrintsView);

            GroupedBlueprintSheet.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            GroupedBlueprintView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

        }

        private void InitializeFiltering()
        {
            SearchTextSheets = string.Empty;
            SearchTextViews = string.Empty;

            if (IsSheetTabSelected)
            {
                GroupedBlueprintSheet.Filter = obj => FilterBySearchText(obj, SearchTextSheets);
            }
            else if (IsViewTabSelected)
            {
                GroupedBlueprintSheet.Filter = obj => FilterBySearchText(obj, SearchTextViews);
            }

        }

        private void InitializeCommands()
        {
            // Initialize commands with lambda expressions that capture the parameters
            InsertPlaceholderCommand = new RelayCommand<string>((targetTab) => InsertBluePrintItem(SelectedAvailableBlueprint, targetTab));
            RemoveSelectedBlueprintCommand = new RelayCommand<string>(RemoveSelectedBlueprintElement, CanRemoveBlueprintElement);
            AddSelectedBlueprintCommand = new RelayCommand<string>(AddSelectedBluePrint, CanAddSelectedBluePrint);
            ClearSearchBoxCommand = new RelayCommand<object>(ClearSearchBox);
        }

        private bool FilterBySearchText(object obj, string searchText)
        {
            if (obj is BluePrintItem bluePrint)
            {
                return bluePrint.ParameterName.ToLower().Contains(searchText?.ToLower() ?? "") ||
                       string.IsNullOrEmpty(searchText) ||
                       string.Equals(searchText, PlaceHolder);
            } 
            return false;
        }

        private void ClearSearchBox(object parameter)
        {
            if (parameter is SearchBox searchBox)
            {
                searchBox.SearchText = string.Empty;
            }
            else if (parameter == null)
            {
                Debug.WriteLine("Sender is null");
            }
        }

        public void SearchBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Refresh the appropriate collection view when a search text property changes
            if (e.PropertyName == nameof(SearchTextSheets))
            {
                GroupedBlueprintSheet.Refresh();
                Debug.WriteLine($"{nameof(GroupedBlueprintSheet)} was refreshed");

            }
            else if (e.PropertyName == nameof(SearchTextViews))
            {
                GroupedBlueprintView.Refresh();
                Debug.WriteLine($"{nameof(GroupedBlueprintView)} was refreshed");
            }
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

        public void InsertBluePrintItem(BluePrintItem bluePrintItem, string targetTab = null)
        {
            if (bluePrintItem != null)
            {
                // Default to the currently selected tab if not specified
                if (string.IsNullOrEmpty(targetTab))
                {
                    targetTab = IsSheetTabSelected ? "Sheet" : "View";
                }

                if (targetTab == "Sheet")
                {
                    CurrentSheetBlueprint.Add(bluePrintItem);
                }
                else
                {
                    CurrentViewBlueprint.Add(bluePrintItem);
                }
                Debug.WriteLine($"Inserted blueprint: {bluePrintItem.ParameterName} into {targetTab} tab");
            }
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
                CurrentSheetBlueprint.Remove(SelectedSheetBlueprintElement);
            }
            else if (targetTab == "View" && SelectedViewBlueprintElement != null)
            {
                Debug.WriteLine($"Removed element from View: {SelectedViewBlueprintElement.ParameterName}");
                CurrentViewBlueprint.Remove(SelectedViewBlueprintElement);
            }
        }

        private bool CanAddSelectedBluePrint(string targetTab)
        {
            return SelectedAvailableBlueprint != null;
        }

        public void AddSelectedBluePrint(string targetTab)
        {
            if (SelectedAvailableBlueprint != null)
            {
                InsertBluePrintItem(SelectedAvailableBlueprint, targetTab);
            }
        }

        private void CurrentBluePrint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedAvailableBlueprint))
            {
                Debug.WriteLine(SelectedAvailableBlueprint?.ParameterName ?? "No selection");
                AddSelectedBlueprintCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(SelectedSheetBlueprintElement))
            {
                Debug.WriteLine(SelectedSheetBlueprintElement?.ParameterName ?? "No Sheet blueprint element selected");
                RemoveSelectedBlueprintCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(SelectedViewBlueprintElement))
            {
                Debug.WriteLine(SelectedViewBlueprintElement?.ParameterName ?? "No View blueprint element selected");
                RemoveSelectedBlueprintCommand.NotifyCanExecuteChanged();
            }
        }

        // Public methods to get the formatted naming blueprint for Sheet and View
        public string GetFormattedSheetNaming()
        {
            return FormatBlueprint(CurrentSheetBlueprint);
        }

        public string GetFormattedViewNaming()
        {
            return FormatBlueprint(CurrentViewBlueprint);
        }

        private string FormatBlueprint(IEnumerable<BluePrintItem> elements)
        {
            if (elements == null || !elements.Any())
                return string.Empty;

            // Use IDs instead of parameter names to avoid issues with delimiters in parameter names
            string result = string.Join(",", elements.Select(e => e.Id.ToString()));
            return result;
        }
    }
}