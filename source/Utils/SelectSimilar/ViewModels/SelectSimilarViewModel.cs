using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Localization;
using SelectSimilar.Models;
using System.Diagnostics;
using System.Windows.Data;
using SelectSimilar.Views.UserControls;

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

    public partial class SelectSimilarViewModel : ObservableObject
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
        protected bool CategoryIsChecked { get; set; }
        protected bool VisibleInViewIsChecked { get; set; }

        // Parameters storage
        protected ParameterSet CheckedParameters { get; set; } = new();

        // UI strings

        public string CommandName { get; private set; }
        public string ButtonOKName { get; private set; }
        public string ButtonCancelName { get; private set; }
        public string ButtonApplyName { get; private set; }
        public string ButtonSaveApplyName { get; private set; }
        public string PlaceholderParameters { get; private set; }

        [ObservableProperty]
        public string searchTextParameters;


        // Path to Settings.Json
        public string SettingsFilePath { get; private set; }

        // Commands
        public IRelayCommand ApplyCommand { get; private set; }
        public IRelayCommand SaveAndApplyCommand { get; private set; }
        public IRelayCommand CancelCommand { get; private set; }
        public IRelayCommand OnMouseHoverCommad {  get; private set; }
        public IRelayCommand ClearCommand { get; private set; }

        public event Action RequestClose;

        // Constructor - can be overridden by derived classes
        public SelectSimilarViewModel()
        {
            //Empty for Child-Class
        }

        public SelectSimilarViewModel(Model model)
        {
            Model = model;

            InitializeUIStrings(PushButtons.SelectSimilar_MainName);
            InitializeCommands();
            InitializeSettings();
            InitializeCheckBoxes();
            InitializeFiltering();
        }

        // Protected or virtual methods can be overridden by derived classes
        public virtual void InitializeUIStrings(string commandName)
        {
            CommandName = commandName;
            ButtonOKName = DialogElements.Generl_ButtonOkName;
            ButtonCancelName = DialogElements.General_ButtonCancelName;
            ButtonApplyName = DialogElements.General_ButtonApplyName;
            ButtonSaveApplyName = DialogElements.General_ButtonSaveApplyName;
            PlaceholderParameters = DialogElements.SelectSimilar_PlaceholderParameters;
        }

        public virtual void InitializeCommands()
        {
            ApplyCommand = new RelayCommand(Apply);
            SaveAndApplyCommand = new RelayCommand(SaveAndApply);
            CancelCommand = new RelayCommand(Cancel);
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

        protected virtual void InitializeCheckBoxes()
        {
            var generalIDs = new List<BuiltInParameter>
            {
                BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                BuiltInParameter.ELEM_FAMILY_PARAM,
                BuiltInParameter.ELEM_CATEGORY_PARAM
            };

            foreach (Parameter parameter in Model.ParameterSet)
            {
                var checkBoxItem = GetOrCreateCheckBoxItem(parameter);
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

            AddVisibleInViewCheckBox();
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

        protected virtual CheckBoxItem GetOrCreateCheckBoxItem(Parameter parameter)
        {
            if (StoredSettings.TryGetValue(parameter.Element.Category.Name, out var categoryDict) &&
                categoryDict.TryGetValue(Math.Abs(parameter.Id.Value), out var checkBoxItem))
            {
                return checkBoxItem;
            }

            var newItem = new CheckBoxItem(Math.Abs(Math.Abs(parameter.Id.Value)), parameter.Definition.Name);

            if (!StoredSettings.ContainsKey(parameter.Element.Category.Name))
            {
                StoredSettings[parameter.Element.Category.Name] = [];
            }

            StoredSettings[parameter.Element.Category.Name].Add(Math.Abs(parameter.Id.Value), newItem);

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

        protected virtual void AddVisibleInViewCheckBox()
        {
            var checkBoxIsVisibleInView = new CheckBoxItem(0, DialogElements.SelectSimilar_VisibleInView);

            if (!StoredSettings.ContainsKey(Model.CurrentCategory.Name))
            {
                StoredSettings[Model.CurrentCategory.Name] = [];
            }

            StoredSettings[Model.CurrentCategory.Name][checkBoxIsVisibleInView.Id] = checkBoxIsVisibleInView;
            GeneralParameterCheckBoxes.Add(checkBoxIsVisibleInView);
        }

        // Methods for commands
        public virtual void Apply()
        {
            Model.Filter(CheckedParameters, CategoryIsChecked, VisibleInViewIsChecked);
            RequestClose?.Invoke();
        }

        public virtual void SaveAndApply()
        {
            string jsonString = JsonConvert.SerializeObject(StoredSettings, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, jsonString);
            Apply();
        }

        public virtual void Cancel()
        {
            Debug.WriteLine("Cancel");
            RequestClose?.Invoke();
        }

        // Event-Handler for CheckBox changes
        protected virtual void CheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
        protected virtual void HandleCheckBoxChange(CheckBoxItem checkBoxItem)
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
