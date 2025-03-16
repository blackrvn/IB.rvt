using RoomStudies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace RoomStudies.ViewModels
{
    public partial class  RSTypeMenuViewModel : ObservableObject
    {
        private readonly RSSettingsModel _model;
        public string Name { get; private set; }

        [ObservableProperty]
        private string _searchText;
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
            SelectedTitleBlockType = FindElementById(TitleBlockTypes, _model.SelectedTitleBlockTypeId) ?? TitleBlockTypes.FirstOrDefault();
            SelectedElevationType = FindElementById(ElevationTypes, _model.SelectedElevationTypeId) ?? ElevationTypes.FirstOrDefault();
            SelectedFloorViewTemplate = FindElementById(FloorViewTemplates, _model.SelectedFloorViewTemplateId) ?? FloorViewTemplates.FirstOrDefault();
            SelectedCeilingViewTemplate = FindElementById(CeilingViewTemplates, _model.SelectedCeilingViewTemplateId) ?? CeilingViewTemplates.FirstOrDefault();
            SelectedElevationViewTemplate = FindElementById(ElevationViewTemplates, _model.SelectedElevationViewTemplateId) ?? ElevationViewTemplates.FirstOrDefault();
        }

        private T FindElementById<T>(IEnumerable<T> collection, ElementId id) where T : Element
        {
            if (id == null || id == ElementId.InvalidElementId)
                return null;

            return collection.FirstOrDefault(e => e.Id.Value == id.Value);
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

            SearchText = "";

            FilteredTitleBlockTypes.Filter = FilterTypes;
            FilteredElevationTypes.Filter = FilterTypes;
            FilteredFloorViewTemplates.Filter = FilterTypes;
            FilteredCeilingViewTemplates.Filter = FilterTypes;
            FilteredElevationViewTemplates.Filter = FilterTypes;

            PropertyChanged += SearchBox_PropertyChanged;
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

        private bool FilterTypes(object obj)
        {
            if (obj is Element element)
            {
                return element.Name.ToLower().Contains(SearchText.ToLower()) || string.Equals(SearchText, PlaceHolder);
            }
            return false;
        }

        public void SearchBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText))
            {
                FilteredTitleBlockTypes.Refresh();
                FilteredElevationTypes.Refresh();
                FilteredFloorViewTemplates.Refresh();
                FilteredCeilingViewTemplates.Refresh();
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