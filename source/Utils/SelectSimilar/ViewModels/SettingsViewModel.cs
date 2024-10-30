using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using Localization;
using Newtonsoft.Json;
using SelectSimilar.Models;
using SelectSimilar.Views.UserControls;

namespace SelectSimilar.ViewModels
{
    public partial class SettingsViewModel : SelectSimilarViewModel
    {
        public ObservableCollection<Category> CategoryList { get; private set; }
        public IRelayCommand OkCommand { get; private set; }
        public IRelayCommand CategoryChangedCommand { get; private set; }
        public ICollectionView FilteredCategories { get; private set; }
        public string PlaceholderCategories {  get; private set; }
        [ObservableProperty]
        private string searchTextCategories;

        [ObservableProperty]
        private Category currentCategory;

        public SettingsViewModel(Model model) : base()
        {
            Model = model;

            this.PropertyChanged += Category_Changed;
            currentCategory = Model.UIDocument.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);

            InitializeUIStrings(PushButtons.SelectSimilar_SettingsName);
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

        public void InitializeCategoryList()
        {
            CategoryList = [];

            CategoryList = Model.CreateCategoryList();
        }

        public override void InitializeCommands()
        {
            base.InitializeCommands();
            OkCommand = new RelayCommand(Ok);

        }

        public override void InitializeUIStrings(string commandName)
        {
            base.InitializeUIStrings(commandName);
            PlaceholderCategories = DialogElements.SelectSimilar_PlaceholderCategories;

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

        public void Ok()
        {
            Debug.WriteLine("OK");
            string jsonString = JsonConvert.SerializeObject(StoredSettings, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, jsonString);
            base.Cancel();
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
    }

}
