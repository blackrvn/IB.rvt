using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Localization;
using SelectSimilar.Models;

namespace SelectSimilar.ViewModels
{
    public partial class SettingsViewModel : SelectSimilarViewModel
    {
        // Collections
        public ObservableCollection<Category> CategoryList { get; private set; } = new ObservableCollection<Category>();
        public CheckBoxItem SuppressCheckBox { get; set; }

        // Commands
        public IRelayCommand CategoryChangedCommand { get; private set; }

        // UI Strings
        public string CategoriesHeader { get; private set; }
        public string PlaceholderCategories { get; private set; }
        public string SupressMainDialogText { get; set; }

        // Filtered Views
        public ICollectionView FilteredCategories { get; private set; }


        // Observable Properties
        [ObservableProperty]
        private string searchTextCategories;

        [ObservableProperty]
        private Category currentCategory;


        public SettingsViewModel(Model model) : base()
        {
            Model = model;

            this.PropertyChanged += Category_Changed;
            currentCategory = Model.UIDocument.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);

            InitializeUIStrings(
                PushButtons.SelectSimilar_SettingsName,
                DialogElements.General_ButtonOkName,
                DialogElements.General_ButtonCancelName);
            CategoriesHeader = DialogElements.SelectSimilar_CategoriesHeader;
            InitializeCommands();
            InitializeSettings();
            InitializeCategoryList();
            InitializeFiltering();
        }


        public void Category_Changed(object sender, EventArgs e)
        {
            if (sender is SettingsViewModel)
            {
                Clear();
                LoadParametersByCategory(CurrentCategory);
            }
        }

        public override void SearchBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.SearchBox_PropertyChanged(sender, e);

            if (e.PropertyName == nameof(SearchTextCategories))
            {
                Debug.WriteLine(SearchTextCategories);
                FilteredCategories.Refresh();
            }

        }

        public void AddSuppressCheckBox()
        {
            SuppressCheckBox = GetOrCreateCheckBoxItem("0", 1, DialogElements.SelectSimilar_Suppress);
        }

        public void InitializeCategoryList()
        {
            CategoryList = Model.CreateCategoryList();
        }

        public override void InitializeUIStrings(string commandName, string okName, string cancelName)
        {
            base.InitializeUIStrings(commandName, okName, cancelName);
            PlaceholderCategories = DialogElements.SelectSimilar_PlaceholderCategories;
            SupressMainDialogText = DialogElements.SelectSimilar_Suppress;
        }

        public override void InitializeFiltering()
        {
            base.InitializeFiltering();

            FilteredCategories = CollectionViewSource.GetDefaultView(CategoryList);
            SearchTextCategories = "";
            FilteredCategories.Filter = FilterCategories;

            PropertyChanged += SearchBox_PropertyChanged;

        }

        public void LoadParametersByCategory(Category category)
        {
            Model.CreateParameterSetByCategory(category);
            base.InitializeCheckBoxes();
        }

        public override void AddNonParametricCheckBoxes()
        {
            base.AddNonParametricCheckBoxes();
            AddSuppressCheckBox();
        }

        public void Clear()
        {
            base.GeneralParameterCheckBoxes.Clear();
            base.BuiltInParameterCheckBoxes.Clear();
            base.CustomParameterCheckBoxes.Clear();
        }

        private bool FilterCategories(object parameter)
        {
            if (parameter is Category category)
            {
                return string.IsNullOrEmpty(category.Name) || category.Name.IndexOf(SearchTextCategories, StringComparison.OrdinalIgnoreCase)>=0 || string.Equals(SearchTextCategories, PlaceholderCategories);
            }
            return false;
        }

        public override void Ok()
        {
            base.Save();
            base.Cancel();
        }
    }

}
