using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using Localization;
using SelectSimilar.Models;
using System.Diagnostics;
using System.Windows.Data;
using Library.Views.UserControls;
using Library.ViewModels;

namespace SelectSimilar.ViewModels
{
    [method: JsonConstructor]
    public partial class CheckBoxItem(long id, string name) : ObservableObject
    {
        [ObservableProperty]
        private bool isChecked;

        public string Name { get; set; } = name;
        public long Id { get; set; } = id;
    }

    public partial class SelectSimilarViewModel : BaseViewModel
    {
        protected Model Model { get; set; }

        // ObservableCollections for CheckBoxItems
        public ObservableCollection<CheckBoxItem> BuiltInParameterCheckBoxes { get; set; } = [];
        public ObservableCollection<CheckBoxItem> CustomParameterCheckBoxes { get; set; } = [];
        public ObservableCollection<CheckBoxItem> GeneralParameterCheckBoxes { get; set; } = [];

        // CollectionViews for filtering
        public ICollectionView FilteredBuiltInParameters { get; private set; }
        public ICollectionView FilteredCustomParameters { get; private set; }
        public ICollectionView FilteredGeneralParameters { get; private set; }


        // Lookup table for ParameterId to Parameter
        public Dictionary<long, Parameter> ParameterIdMapping { get; private set; } = [];

        // Stored settings from JSON
        public Dictionary<string, Dictionary<long, CheckBoxItem>> StoredSettings { get; set; } = [];

        // Flags for visibility
        public bool CategoryIsChecked { get; private set; }
        public bool VisibleInViewIsChecked { get; private set; }
        public bool SuppressMainDialog { get; private set; }

        // Parameters storage
        public ParameterSet CheckedParameters { get; private set; } = new();

        // UI strings
        public string ButtonSaveApplyName { get; private set; }
        public string PlaceholderParameters { get; private set; }

        [ObservableProperty]
        public string searchTextParameters;

        public string BuiltInParametersHeader { get; private set; }
        public string CustomParametersHeader { get; private set; }
        public string GeneralParametersHeader { get; private set; }


        // Path to Settings.Json
        public string SettingsFilePath { get; private set; }

        // Commands
        public IRelayCommand SaveAndApplyCommand { get; private set; }
        public IRelayCommand ClearCommand { get; private set; }

        // Constructor - can be overridden by derived classes
        public SelectSimilarViewModel()
        {
            //Empty for Child-Class
        }

        public SelectSimilarViewModel(Model model)
        {
            Model = model;

            InitializeUIStrings(
                PushButtons.SelectSimilar_MainName, 
                DialogElements.General_ButtonApplyName, 
                DialogElements.General_ButtonCancelName);
            InitializeCommands();
            InitializeSettings();
            CheckSupressionMode();
            InitializeCheckBoxes();
            InitializeFiltering();
        }

        // Protected or virtual methods can be overridden by derived classes
        public override void InitializeUIStrings(string commandName, string okName, string cancelName)
        {
            base.InitializeUIStrings(commandName, okName, cancelName);

            ButtonSaveApplyName = DialogElements.General_ButtonSaveApplyName;
            PlaceholderParameters = DialogElements.SelectSimilar_PlaceholderParameters;
            BuiltInParametersHeader = DialogElements.SelectSimilar_BIPHeader;
            GeneralParametersHeader = DialogElements.SelectSimilar_GeneralParametersHeader;
            CustomParametersHeader = DialogElements.SelectSimilar_CustomParametersHeader;

        }

        private void CheckSupressionMode()
        {
            CheckBoxItem supressionCheckBox = GetOrCreateCheckBoxItem("0", 1, DialogElements.SelectSimilar_Suppress);
            SuppressMainDialog = supressionCheckBox.IsChecked;
        }

        public override void InitializeCommands()
        {
            base.InitializeCommands();

            SaveAndApplyCommand = new RelayCommand(SaveAndApply);
            ClearCommand = new RelayCommand<object>(ClearSearchBox);
        }

        public virtual void InitializeSettings()
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string revitVersion = Model.UIDocument.Document.Application.VersionNumber;
            SettingsFilePath = Path.Combine(userPath, "Autodesk", "Revit", "Addins", revitVersion, "Core", "Settings.JSON");

            if (File.Exists(SettingsFilePath))
            {
                string jsonString = File.ReadAllText(SettingsFilePath);
                StoredSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<long, CheckBoxItem>>>(jsonString);
            }
        }

        public virtual void InitializeCheckBoxes()
        {
            var generalIDs = new List<BuiltInParameter>
            {
                BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                BuiltInParameter.ELEM_FAMILY_PARAM,
                BuiltInParameter.ELEM_CATEGORY_PARAM
            };

            foreach (Parameter parameter in Model.ParameterSet)
            {
                var checkBoxItem = GetOrCreateCheckBoxItem(parameter.Element.Category.Name, parameter.Id.Value, parameter.Definition.Name);
                AddCheckBoxToCollections(checkBoxItem, parameter, generalIDs);

                if (!ParameterIdMapping.ContainsKey(Math.Abs(parameter.Id.Value)))
                {
                    ParameterIdMapping.Add(Math.Abs(parameter.Id.Value), parameter);

                }

                if (checkBoxItem.IsChecked)
                {
                    CheckedParameters.Insert(parameter);
                }
            }

            AddNonParametricCheckBoxes();
        }

        public virtual void InitializeFiltering()
        {
            
            FilteredBuiltInParameters = CollectionViewSource.GetDefaultView(BuiltInParameterCheckBoxes);
            FilteredCustomParameters = CollectionViewSource.GetDefaultView(CustomParameterCheckBoxes);
            FilteredGeneralParameters = CollectionViewSource.GetDefaultView(GeneralParameterCheckBoxes);

            SearchTextParameters = "";

            FilteredBuiltInParameters.Filter = FilterParameters;
            FilteredCustomParameters.Filter = FilterParameters;
            FilteredGeneralParameters.Filter = FilterParameters;

            // Reaktion auf Änderungen von SearchTextParameters
            PropertyChanged += SearchBox_PropertyChanged;

        }

        protected virtual CheckBoxItem GetOrCreateCheckBoxItem(string categoryName, long id, string name)
        {
            if (StoredSettings.TryGetValue(categoryName, out var categoryDict) &&
                categoryDict.TryGetValue(Math.Abs(id), out var checkBoxItem))
            {
                return checkBoxItem;
            }

            var newItem = new CheckBoxItem(Math.Abs(Math.Abs(id)), name);

            if (!StoredSettings.ContainsKey(categoryName))
            {
                StoredSettings[categoryName] = [];
            }

            StoredSettings[categoryName].Add(Math.Abs(id), newItem);

            return newItem;
        }

        protected virtual void AddCheckBoxToCollections(CheckBoxItem checkBoxItem, Parameter parameter, List<BuiltInParameter> generalIDs)
        {
            checkBoxItem.PropertyChanged += CheckBox_PropertyChanged;

            if (parameter.Definition is InternalDefinition internalDefinition && internalDefinition.BuiltInParameter != BuiltInParameter.ELEM_CATEGORY_PARAM_MT)
            {
                if (internalDefinition.BuiltInParameter == BuiltInParameter.INVALID)
                {
                    CustomParameterCheckBoxes.Add(checkBoxItem);
                }
                else if (!generalIDs.Contains(internalDefinition.BuiltInParameter))
                {
                    BuiltInParameterCheckBoxes.Add(checkBoxItem);
                }
                else
                {
                    GeneralParameterCheckBoxes.Add(checkBoxItem);
                }
            }
        }


        public virtual void AddNonParametricCheckBoxes()
        {
            CheckBoxItem checkBoxIsVisibleInView = GetOrCreateCheckBoxItem("0", 0, DialogElements.SelectSimilar_VisibleInView);
            GeneralParameterCheckBoxes.Add(checkBoxIsVisibleInView);
        }


        // Methods for commands
        public override void Ok()
        {
            base.Ok(); 
            Model.Filter(CheckedParameters, CategoryIsChecked, VisibleInViewIsChecked);
        }

        public virtual void Save()
        {
            string jsonString = JsonConvert.SerializeObject(StoredSettings, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, jsonString);
        }

        public virtual void SaveAndApply()
        {
            Save();
            Ok();
        }

        // Event-Handler for CheckBox changes
        public virtual void CheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckBoxItem.IsChecked) && sender is CheckBoxItem checkBoxItem)
            {
                HandleCheckBoxChange(checkBoxItem);
            }
        }

        public virtual void SearchBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchTextParameters))
            {
                FilteredBuiltInParameters.Refresh();
                FilteredCustomParameters.Refresh();
                FilteredGeneralParameters.Refresh();
            }
        }

        // Handle CheckBox changes
        public virtual void HandleCheckBoxChange(CheckBoxItem checkBoxItem)
        {
            if (checkBoxItem.Name == DialogElements.SelectSimilar_VisibleInView)
            {
                Debug.WriteLine(ParameterIdMapping[checkBoxItem.Id]);
                VisibleInViewIsChecked = checkBoxItem.IsChecked;
            }
            else if (checkBoxItem.Name == BuiltInParameter.ELEM_CATEGORY_PARAM.ToLabel())
            {
                Debug.WriteLine(ParameterIdMapping[checkBoxItem.Id]);
                CategoryIsChecked = checkBoxItem.IsChecked;
            }
            else
            {
                if (checkBoxItem.IsChecked)
                {
                    Debug.WriteLine(ParameterIdMapping[checkBoxItem.Id].Definition.Name);
                    CheckedParameters.Insert(ParameterIdMapping[checkBoxItem.Id]);
                }
                else
                {
                    Debug.WriteLine(ParameterIdMapping[checkBoxItem.Id].Definition.Name);
                    CheckedParameters.Erase(ParameterIdMapping[checkBoxItem.Id]);
                }
            }
        }

        protected virtual void ClearSearchBox(object parameter)
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

        protected virtual bool FilterParameters(object obj)
        {
            if (obj is CheckBoxItem checkBoxItem)
            {
                return string.IsNullOrEmpty(SearchTextParameters) || checkBoxItem.Name.IndexOf(SearchTextParameters, StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(SearchTextParameters, PlaceholderParameters);
            }
            return false;
        }

    }
}
