using Library.ViewModels;
using RoomStudies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace RoomStudies.ViewModels
{
    public sealed partial class RSSettingsViewModel : BaseViewModel
    {
        private readonly RSSettingsModel _model;

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private List<object> _views = new List<object>();

        // Add properties to store naming formats
        [ObservableProperty]
        private string _sheetNamingFormat;

        [ObservableProperty]
        private string _viewNamingFormat;

        [ObservableProperty]
        private bool _useLettersForViewNumbering = true;

        [ObservableProperty]
        private bool _useNumbersForViewNumbering;

        // Command for saving settings
        public IRelayCommand SaveCommand { get; private set; }

        public RSSettingsViewModel(RSSettingsModel model)
        {
            _model = model;

            // Load saved settings
            SheetNamingFormat = _model.SheetNamingFormat;
            ViewNamingFormat = _model.ViewNamingFormat;
            UseLettersForViewNumbering = _model.UseLettersForViewNumbering;
            UseNumbersForViewNumbering = !UseLettersForViewNumbering;

            CurrentView = new RSTypeMenuViewModel(_model);
            InitializeUIStrings("Settings", "OK", "Cancel");
            InitializeViews();
            InitializeCommands();
        }

        private void InitializeViews()
        {
            Views.Add(new RSTypeMenuViewModel(_model));
            Views.Add(new RSNamingMenuViewModel(_model));
        }

        public override void InitializeCommands()
        {
            base.InitializeCommands();

            PropertyChanged += CurrentView_PropertyChanged;

            // Add additional commands for saving naming settings
            SaveCommand = new RelayCommand(SaveSettings);
        }

        private void CurrentView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentView))
            {
                Debug.WriteLine("Menu");

                // Check if we're leaving a naming menu view
                if (sender is RSSettingsViewModel vm && vm.CurrentView != null &&
                    !(vm.CurrentView is RSNamingMenuViewModel) &&
                    Views.Count >= 2 && Views[1] is RSNamingMenuViewModel oldNamingViewModel)
                {
                    // Unsubscribe from events to prevent memory leaks
                    oldNamingViewModel.PropertyChanged -= NamingViewModel_PropertyChanged;

                    // Save settings when leaving the view
                    SaveNamingSettings(oldNamingViewModel);
                }

                // If switching to naming menu, update its properties
                if (CurrentView is RSNamingMenuViewModel namingViewModel)
                {
                    // Initialize with saved settings if available
                    if (!string.IsNullOrEmpty(SheetNamingFormat))
                    {
                        // Load saved blueprint elements for Sheet (implementation needed)
                        LoadNamingFormat(namingViewModel, true);
                    }

                    if (!string.IsNullOrEmpty(ViewNamingFormat))
                    {
                        // Load saved blueprint elements for View (implementation needed)
                        LoadNamingFormat(namingViewModel, false);
                    }

                    namingViewModel.SheetDelimiter = string.IsNullOrEmpty(SheetNamingFormat) ? "_" : namingViewModel.SheetDelimiter;
                    namingViewModel.ViewDelimiter = string.IsNullOrEmpty(ViewNamingFormat) ? "_" : namingViewModel.ViewDelimiter;
                    namingViewModel.UseLettersForNumbering = UseLettersForViewNumbering;
                    namingViewModel.UseNumbersForNumbering = UseNumbersForViewNumbering;

                    // Subscribe to property changes to save settings
                    namingViewModel.PropertyChanged += NamingViewModel_PropertyChanged;
                }

                // Also track changes in type menu view
                if (e.PropertyName == nameof(CurrentView) && CurrentView is RSTypeMenuViewModel typeMenuViewModel)
                {
                    // Subscribe to save type selections
                    typeMenuViewModel.PropertyChanged += TypeMenuViewModel_PropertyChanged;
                }
            }
        }

        private void TypeMenuViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We don't need specific handling here as the TypeMenuViewModel
            // updates the model directly when selections change
        }

        private void NamingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is RSNamingMenuViewModel namingViewModel)
            {
                // Track changes to sheet and view settings
                if (e.PropertyName == nameof(RSNamingMenuViewModel.SheetDelimiter) ||
                    e.PropertyName == nameof(RSNamingMenuViewModel.CurrentSheetBlueprint))
                {
                    SheetNamingFormat = namingViewModel.GetFormattedSheetNaming();
                    Debug.WriteLine($"Sheet naming format updated: {SheetNamingFormat}");
                }
                else if (e.PropertyName == nameof(RSNamingMenuViewModel.ViewDelimiter) ||
                         e.PropertyName == nameof(RSNamingMenuViewModel.CurrentViewBlueprint))
                {
                    ViewNamingFormat = namingViewModel.GetFormattedViewNaming();
                    Debug.WriteLine($"View naming format updated: {ViewNamingFormat}");
                }
                else if (e.PropertyName == nameof(RSNamingMenuViewModel.UseLettersForNumbering))
                {
                    UseLettersForViewNumbering = namingViewModel.UseLettersForNumbering;
                    UseNumbersForViewNumbering = !UseLettersForViewNumbering;
                    Debug.WriteLine($"Numbering format updated: Letters={UseLettersForViewNumbering}");
                }
                else if (e.PropertyName == nameof(RSNamingMenuViewModel.UseNumbersForNumbering))
                {
                    UseNumbersForViewNumbering = namingViewModel.UseNumbersForNumbering;
                    UseLettersForViewNumbering = !UseNumbersForViewNumbering;
                    Debug.WriteLine($"Numbering format updated: Numbers={UseNumbersForViewNumbering}");
                }
            }
        }

        private void SaveNamingSettings(RSNamingMenuViewModel namingViewModel)
        {
            // Save current settings
            SheetNamingFormat = namingViewModel.GetFormattedSheetNaming();
            ViewNamingFormat = namingViewModel.GetFormattedViewNaming();
            UseLettersForViewNumbering = namingViewModel.UseLettersForNumbering;
            UseNumbersForViewNumbering = namingViewModel.UseNumbersForNumbering;

            Debug.WriteLine("Naming settings saved");
        }

        public void SaveSettings()
        {
            // If the current view is the naming menu, save its settings
            if (CurrentView is RSNamingMenuViewModel namingViewModel)
            {
                SaveNamingSettings(namingViewModel);
            }
            else if (CurrentView is RSTypeMenuViewModel typeMenuViewModel)
            {
                // Save the type selections
                typeMenuViewModel.SaveSettings();
            }

            // Update model with current settings
            _model.SheetNamingFormat = SheetNamingFormat;
            _model.ViewNamingFormat = ViewNamingFormat;

            // Fix for unassigned variable error - use null conditional operator
            var currentNamingViewModel = CurrentView as RSNamingMenuViewModel;
            _model.SheetDelimiter = currentNamingViewModel?.SheetDelimiter ?? _model.SheetDelimiter;
            _model.ViewDelimiter = currentNamingViewModel?.ViewDelimiter ?? _model.ViewDelimiter;
            _model.UseLettersForViewNumbering = UseLettersForViewNumbering;

            // Save to persistent storage
            _model.SaveSettings();

            Debug.WriteLine("All settings saved");
        }

        private void LoadNamingFormat(RSNamingMenuViewModel viewModel, bool isSheet)
        {
            string format = isSheet ? SheetNamingFormat : ViewNamingFormat;
            string delimiter = isSheet ? _model.SheetDelimiter : _model.ViewDelimiter;

            if (isSheet)
            {
                viewModel.SheetDelimiter = delimiter;
                LoadBlueprintElements(viewModel.CurrentSheetBlueprint, format);
            }
            else
            {
                viewModel.ViewDelimiter = delimiter;
                LoadBlueprintElements(viewModel.CurrentViewBlueprint, format);
            }
        }

        private string GetDelimiterFromFormat(string format)
        {
            // Default delimiter
            if (string.IsNullOrEmpty(format))
                return "_";

            // Try to detect delimiter from format
            // This is a simple implementation - a more robust one would handle escaped characters, etc.
            foreach (char potentialDelimiter in new[] { '_', '-', '.', ' ' })
            {
                if (format.Contains(potentialDelimiter))
                    return potentialDelimiter.ToString();
            }

            // If no common delimiter is found, default to underscore
            return "_";
        }

        private void LoadBlueprintElements(ObservableCollection<BluePrintItem> collection, string format)
        {
            // Clear existing elements
            collection.Clear();

            if (string.IsNullOrEmpty(format))
                return;

            // Split the format by delimiter
            string[] parts = format.Split(new[] { "," }, StringSplitOptions.None);

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                // Try to convert the part to a long ID
                if (long.TryParse(part, out long id))
                {
                    // Try to find a matching placeholder in available placeholders
                    BluePrintItem placeholder = FindPlaceholderById(id);

                    if (placeholder != null)
                    {
                        collection.Add(placeholder);
                    }
                    else
                    {
                        // If we couldn't find a matching placeholder, create a generic one
                        collection.Add(new BluePrintItem
                        {
                            Category = "Restored",
                            ParameterName = $"[ID: {id}]",
                            Id = id
                        });
                    }
                }
                else
                {
                    // If it's not a valid ID (might be an old format or static text), create a generic item
                    collection.Add(new BluePrintItem
                    {
                        Category = "Static",
                        ParameterName = part,
                        Id = -1  // Use -1 for static text
                    });
                }
            }
        }

        private BluePrintItem FindPlaceholderById(long id)
        {
            // If the current view is a RSNamingMenuViewModel, look in its available placeholders
            if (CurrentView is RSNamingMenuViewModel namingViewModel)
            {
                return namingViewModel.AvailableBluePrintsSheet.FirstOrDefault(p => p.Id == id);
            }

            // If we don't have a RSNamingMenuViewModel at hand, check all views
            if (Views.Count >= 2 && Views[1] is RSNamingMenuViewModel menuViewModel)
            {
                return menuViewModel.AvailableBluePrintsSheet.FirstOrDefault(p => p.Id == id);
            }

            // If we can't find it, return null
            return null;
        }

        public override void Ok()
        {
            // Save settings before closing
            SaveSettings();
            base.Ok();
        }
    }
}