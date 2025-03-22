using RoomStudies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace RoomStudies.ViewModels
{
    public partial class RSTypeMenuViewModel : ObservableObject
    {
        private readonly RSSettingsModel _model;
        public string Name { get; private set; }

        [ObservableProperty]
        private string _searchTextTitleBlockType;
        [ObservableProperty]
        private string _searchTextFloorViewTemplate;
        [ObservableProperty]
        private string _searchTextCeilingViewTemplate;
        [ObservableProperty]
        private string _searchTextElevationViewTemplate;
        [ObservableProperty]
        private string _placeHolder;

        [ObservableProperty]
        public FamilySymbol _selectedTitleBlockType;
        [ObservableProperty]
        public FamilySymbol _selectedElevationType;
        [ObservableProperty]
        public View _selectedFloorViewTemplate;
        [ObservableProperty]
        public View _selectedCeilingViewTemplate;
        [ObservableProperty]
        public View _selectedElevationViewTemplate;

        [ObservableProperty]
        private ObservableCollection<FamilySymbol> _titleBlockTypes;
        [ObservableProperty]
        private ObservableCollection<FamilySymbol> _elevationTypes;
        [ObservableProperty]
        private ObservableCollection<View> _floorViewTemplates;
        [ObservableProperty]
        private ObservableCollection<View> _ceilingViewTemplates;
        [ObservableProperty]
        private ObservableCollection<View> _elevationViewTemplates;

        [ObservableProperty]
        private ICollectionView _filteredTitleBlockTypes;
        [ObservableProperty]
        private ICollectionView _filteredElevationTypes;
        [ObservableProperty]
        private ICollectionView _filteredFloorViewTemplates;
        [ObservableProperty]
        private ICollectionView _filteredCeilingViewTemplates;
        [ObservableProperty]
        private ICollectionView _filteredElevationViewTemplates;

        public string HeaderTitleBlock { get; private set; }
        public string HeaderElevationType { get; private set; }
        public string HeaderFloorViewTemplate { get; private set; }
        public string HeaderCeilingViewTemplate { get; private set; }
        public string HeaderElevationViewTemplate { get; private set; }

        public ICollection<GridLength> ColumnWidths { get; private set; }

        public RSTypeMenuViewModel(RSSettingsModel model)
        {
            _model = model;
            Name = Localization.DialogElements.RoomStudy_TypeMenuHeader;

            InitializeStrings();
            InitializeCollections();
            InitializeFiltering();

            ColumnWidths = new ObservableCollection<GridLength>
            {
                new(200),
                new(1, GridUnitType.Star)
            };

            // Track selection changes
            PropertyChanged += SelectionChanged;
        }

        private void InitializeCollections()
        {
            TitleBlockTypes = new(_model.GetTypes(BuiltInCategory.OST_TitleBlocks));
            ElevationTypes = new(_model.GetTypes(BuiltInCategory.OST_ElevationMarks));

            FloorViewTemplates = new(_model.GetViewTemplates(_model.GetElementIds(BuiltInCategory.OST_Views), ViewType.FloorPlan));
            CeilingViewTemplates = new(_model.GetViewTemplates(_model.GetElementIds(BuiltInCategory.OST_Views), ViewType.CeilingPlan));
            ElevationViewTemplates = new(_model.GetViewTemplates(_model.GetElementIds(BuiltInCategory.OST_Views), ViewType.Elevation));

            // Try to restore saved selections

            SelectedTitleBlockType = _model.FindElementById(TitleBlockTypes, _model.SelectedTitleBlockTypeId);
            SelectedElevationType = _model.FindElementById(ElevationTypes, _model.SelectedElevationTypeId);
            SelectedFloorViewTemplate = _model.FindElementById(FloorViewTemplates, _model.SelectedFloorViewTemplateId);
            SelectedCeilingViewTemplate = _model.FindElementById(CeilingViewTemplates, _model.SelectedCeilingViewTemplateId);
            SelectedElevationViewTemplate = _model.FindElementById(ElevationViewTemplates, _model.SelectedElevationViewTemplateId);
        }

        private void InitializeStrings()
        {
            PlaceHolder = Localization.DialogElements.General_Search;
            HeaderTitleBlock = Localization.DialogElements.RoomStudy_TitleBlockSelector;
            HeaderElevationType = Localization.DialogElements.RoomStudy_ElevationTypeSelector;
            HeaderFloorViewTemplate = Localization.DialogElements.RoomStudy_FloorTemplateSelector;
            HeaderCeilingViewTemplate = Localization.DialogElements.RoomStudy_CeilingTemplateSelector;
            HeaderElevationViewTemplate = Localization.DialogElements.RoomStudy_ElevationTemplateSelector;
        }

        private void InitializeFiltering()
        {
            FilteredTitleBlockTypes = CollectionViewSource.GetDefaultView(TitleBlockTypes) as CollectionView;
            FilteredElevationTypes = CollectionViewSource.GetDefaultView(ElevationTypes) as CollectionView;
            FilteredFloorViewTemplates = CollectionViewSource.GetDefaultView(FloorViewTemplates) as CollectionView;
            FilteredCeilingViewTemplates = CollectionViewSource.GetDefaultView(CeilingViewTemplates) as CollectionView;
            FilteredElevationViewTemplates = CollectionViewSource.GetDefaultView(ElevationViewTemplates) as CollectionView;

            SearchTextTitleBlockType = "";
            SearchTextFloorViewTemplate = "";
            SearchTextCeilingViewTemplate = "";
            SearchTextElevationViewTemplate = "";

            // Apply filters with appropriate search text for each collection
            FilteredTitleBlockTypes.Filter = obj => FilterBySearchText(obj, SearchTextTitleBlockType);
            FilteredElevationTypes.Filter = obj => FilterBySearchText(obj, ""); // No filtering for elevation types
            FilteredFloorViewTemplates.Filter = obj => FilterBySearchText(obj, SearchTextFloorViewTemplate);
            FilteredCeilingViewTemplates.Filter = obj => FilterBySearchText(obj, SearchTextCeilingViewTemplate);
            FilteredElevationViewTemplates.Filter = obj => FilterBySearchText(obj, SearchTextElevationViewTemplate);

            // Monitor property changes to refresh the appropriate filtered collection
            PropertyChanged += SearchBox_PropertyChanged;
        }

        private bool FilterBySearchText(object obj, string searchText)
        {
            if (obj is Element element)
            {
                return element.Name.ToLower().Contains(searchText?.ToLower() ?? "") ||
                       string.IsNullOrEmpty(searchText) ||
                       string.Equals(searchText, PlaceHolder);
            }
            return false;
        }

        private void SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            // When a selection changes, update the model
            if (e.PropertyName == nameof(SelectedTitleBlockType) && SelectedTitleBlockType != null)
            {
                _model.SelectedTitleBlockTypeId = SelectedTitleBlockType.Id;
            }
            else if (e.PropertyName == nameof(SelectedElevationType) && SelectedElevationType != null)
            {
                _model.SelectedElevationTypeId = SelectedElevationType.Id;
            }
            else if (e.PropertyName == nameof(SelectedFloorViewTemplate) && SelectedFloorViewTemplate != null)
            {
                _model.SelectedFloorViewTemplateId = SelectedFloorViewTemplate.Id;
            }
            else if (e.PropertyName == nameof(SelectedCeilingViewTemplate) && SelectedCeilingViewTemplate != null)
            {
                _model.SelectedCeilingViewTemplateId = SelectedCeilingViewTemplate.Id;
            }
            else if (e.PropertyName == nameof(SelectedElevationViewTemplate) && SelectedElevationViewTemplate != null)
            {
                _model.SelectedElevationViewTemplateId = SelectedElevationViewTemplate.Id;
            }
        }

        public void SearchBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Refresh the appropriate collection view when a search text property changes
            if (e.PropertyName == nameof(SearchTextTitleBlockType))
            {
                FilteredTitleBlockTypes.Refresh();
            }
            else if (e.PropertyName == nameof(SearchTextFloorViewTemplate))
            {
                FilteredFloorViewTemplates.Refresh();
            }
            else if (e.PropertyName == nameof(SearchTextCeilingViewTemplate))
            {
                FilteredCeilingViewTemplates.Refresh();
            }
            else if (e.PropertyName == nameof(SearchTextElevationViewTemplate))
            {
                FilteredElevationViewTemplates.Refresh();
            }
        }

        // Method to save the current selections
        public void SaveSettings()
        {
            if (SelectedTitleBlockType != null)
                _model.SelectedTitleBlockTypeId = SelectedTitleBlockType.Id;

            if (SelectedElevationType != null)
                _model.SelectedElevationTypeId = SelectedElevationType.Id;

            if (SelectedFloorViewTemplate != null)
                _model.SelectedFloorViewTemplateId = SelectedFloorViewTemplate.Id;

            if (SelectedCeilingViewTemplate != null)
                _model.SelectedCeilingViewTemplateId = SelectedCeilingViewTemplate.Id;

            if (SelectedElevationViewTemplate != null)
                _model.SelectedElevationViewTemplateId = SelectedElevationViewTemplate.Id;

            _model.SaveSettings();
        }
    }
}