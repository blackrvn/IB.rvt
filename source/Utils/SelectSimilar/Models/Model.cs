using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Localization;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SelectSimilar.Models
{
    public class Model
    {
        public ParameterSet ParameterSet { get; private set; }
        public Element UserSelection { get; set; } // Element, wich was selected by the user
        public UIDocument UIDocument { get; set; }
        public Category CurrentCategory { get; private set; }
        public int RevitVersion
        {
            get { return RevitVersion; }
            set 
            {
                try { RevitVersion = value; } catch {FormatException e; RevitVersion = 2024; }
            }
        }

        public Model(UIDocument uiDocument, bool byCategory = false)
        {
            this.UIDocument = uiDocument;
            //this.RevitVersion = Int32.Parse(uiDocument.Document.Application.VersionNumber);
            if (!byCategory)
            {   
                Reference refrence = uiDocument.Selection.PickObject(ObjectType.Element);
                UserSelection = uiDocument.Document.GetElement(refrence);
                CurrentCategory = UserSelection.Category;
                ParameterSet = UserSelection.Parameters;
            }
        }

        public ObservableCollection<Category> CreateCategoryList()
        {
            ObservableCollection<Category> CategoryList = [];

            foreach (Category category in UIDocument.Document.Settings.Categories)
            {
                ElementFilter filter;
                filter = CreateCategoryFilter(category.BuiltInCategory);

                FilteredElementCollector collector = new FilteredElementCollector(UIDocument.Document).WherePasses(filter);
                if (collector.GetElementCount() > 0)
                {
                    CategoryList.Add(category);
                }
            }

            return CategoryList;
        }

        public void CreateParameterSetByCategory(Category category)
        {
            if (category == null)
            {
                Debug.WriteLine("Category is null in LoadParametersByCategory");
            }
            CurrentCategory = category;
            Dictionary<float, Parameter> AddedParams = [];
            ParameterSet = new();
            ElementFilter filter = CreateCategoryFilter(category.BuiltInCategory);
            FilteredElementCollector collectorInstances = new FilteredElementCollector(UIDocument.Document).WherePasses(filter).WhereElementIsNotElementType();
            FilteredElementCollector collectorTypes = new FilteredElementCollector(UIDocument.Document).WherePasses(filter).WhereElementIsElementType();
            try
            {
                foreach (Parameter parameter in collectorTypes.FirstElement().Parameters)
                {
                    ParameterSet.Insert(parameter);
                    AddedParams.Add(parameter.Id.Value, parameter);
                }

                foreach (Parameter parameter in collectorInstances.FirstElement().Parameters)
                {
                    if (!AddedParams.ContainsKey(parameter.Id.Value))
                    {
                        ParameterSet.Insert(parameter);
                        AddedParams.Add(parameter.Id.Value, parameter);
                    }
                }
            }
            
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void Filter(ParameterSet selectedOptions, bool categoryChecked, bool visibleInViewChecked)
        {
            List<FilterRule> parameterFilterRules = CreateFilterRules(selectedOptions);  // Extracted to a separate method

            var filters = new List<ElementFilter>();
            if (parameterFilterRules.Count > 0)
            {
                ElementParameterFilter parameterFilter = new(parameterFilterRules);
                filters.Add(parameterFilter);
            };

            // Optional filters for category and visibility
            if (categoryChecked)
            {
                filters.Add(CreateCategoryFilter(UserSelection.Category.BuiltInCategory));
            }

            if (visibleInViewChecked)
            {
                filters.Add(new VisibleInViewFilter(UIDocument.Document, UIDocument.Document.ActiveView.Id));
            }

            if (filters.Count > 0)
            {                
                ElementFilter combinedFilter = new LogicalAndFilter(filters);
                FilteredElementCollector collector = new(UIDocument.Document);
                ICollection<ElementId> similarElements = collector.WherePasses(combinedFilter).ToElementIds();

                // Set filtered elements as selected in the UI
                UIDocument.Selection.SetElementIds(similarElements);
            }
        }

        // Extracted method to create FilterRules based on ParameterSet
        private List<FilterRule> CreateFilterRules(ParameterSet selectedOptions)
        {
            var filterRules = new List<FilterRule>();

            foreach (Parameter parameter in selectedOptions)
            {
                ElementId parameterId = parameter.Id;
                ParameterValueProvider provider = new(parameterId);

                FilterRule filterRule = parameter.StorageType switch
                {
                    StorageType.String => new FilterStringRule(provider, new FilterStringEquals(), parameter.AsString()),
                    StorageType.Integer => new FilterIntegerRule(provider, new FilterNumericEquals(), parameter.AsInteger()),
                    StorageType.Double => new FilterDoubleRule(provider, new FilterNumericEquals(), parameter.AsDouble(), 0.001),
                    StorageType.ElementId => new FilterElementIdRule(provider, new FilterNumericEquals(), parameter.AsElementId()),
                    _ => throw new NotSupportedException("Unsupported parameter storage type.")
                };

                filterRules.Add(filterRule);
            }

            return filterRules;
        }

        // Extracted method to create a Category Filter
        private ElementFilter CreateCategoryFilter(BuiltInCategory builtInCategory)
        {
            return new ElementCategoryFilter(builtInCategory);
        }
    }
}
