using RoomStudies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace RoomStudies.ViewModels
{
    partial class  RSTypeMenuViewModel : ObservableObject
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
            Name = "TypeMenu";

            InitializeStrings();
            InitializeCollections();
            InitializeFiltering();

            ColumnWidths = new ObservableCollection<GridLength>
            {
                new(200),
                new(1, GridUnitType.Star)
            };

        }

        private void InitializeCollections()
        {
            TitleBlockTypes = new(_model.GetTypes(BuiltInCategory.OST_TitleBlocks));
            ElevationTypes = new(_model.GetTypes(BuiltInCategory.OST_ElevationMarks));

            FloorViewTemplates = new(_model.GetViewTemplates(_model.GetElementIds(BuiltInCategory.OST_Views), ViewType.FloorPlan));
            CeilingViewTemplates = new(_model.GetViewTemplates(_model.GetElementIds(BuiltInCategory.OST_Views), ViewType.CeilingPlan));
            ElevationViewTemplates = new(_model.GetViewTemplates(_model.GetElementIds(BuiltInCategory.OST_Views), ViewType.Elevation));

            SelectedTitleBlockType = TitleBlockTypes.FirstOrDefault();
            SelectedElevationType = ElevationTypes.FirstOrDefault();
            SelectedFloorViewTemplate = FloorViewTemplates.FirstOrDefault();
            SelectedCeilingViewTemplate = CeilingViewTemplates.FirstOrDefault();
            SelectedElevationViewTemplate = ElevationViewTemplates.FirstOrDefault();

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

    }
}
