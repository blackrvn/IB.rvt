using Library.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RoomStudies.Models
{
    public class RSSettingsModel : ModelBase
    {
        private const string SETTINGS_FILENAME = "RoomStudiesSettings.xml";
        private string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Autodesk", "Revit", "Addins", Doc.Application.VersionNumber, "IB", SETTINGS_FILENAME);

        public RSSettingsModel()
        {
            // Load settings when model is created
            LoadSettings();
        }

        public ICollection<FamilySymbol> GetTypes(BuiltInCategory builtInCategory)
        {
            return new FilteredElementCollector(Doc)
                .OfCategory(builtInCategory)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .ToList();
        }

        public ICollection<Element> GetElements(BuiltInCategory builtInCategory)
        {
            return new FilteredElementCollector(Doc)
                .OfCategory(builtInCategory)
                .ToList();
        }

        public ICollection<ElementId> GetElementIds(BuiltInCategory builtInCategory)
        {
            return new FilteredElementCollector(Doc)
                .OfCategory(builtInCategory)
                .ToElementIds()
                .ToList();
        }

        public ICollection<View> GetViewTemplates(ICollection<ElementId> viewIds, ViewType viewType)
        {
            return new FilteredElementCollector(Doc, viewIds)
                .OfCategory(BuiltInCategory.OST_Views)
                .Where(Element => Element is View view && view.IsTemplate && view.ViewType == viewType)
                .Cast<View>()
                .ToList();
        }

        public ParameterSet GetParametersByBuiltInCategory(BuiltInCategory builtInCategory)
        {
            Dictionary<float, Parameter> AddedParams = [];
            ParameterSet parameterSet = new();

            FilteredElementCollector types = new FilteredElementCollector(Doc)
                .OfCategory(builtInCategory)
                .OfClass(typeof(FamilySymbol));

            FilteredElementCollector instances = new FilteredElementCollector(Doc)
                .OfCategory(builtInCategory)
                .WhereElementIsNotElementType();

            if (types.GetElementCount() > 0)
            {
                foreach (Parameter parameter in types.FirstElement().Parameters)
                {
                    parameterSet.Insert(parameter);
                    AddedParams.Add(parameter.Id.Value, parameter);
                }
            }

            foreach (Parameter parameter in instances.FirstElement().Parameters)
            {
                if (!AddedParams.ContainsKey(parameter.Id.Value))
                {
                    parameterSet.Insert(parameter);
                    AddedParams.Add(parameter.Id.Value, parameter);
                }
            }

            return parameterSet;
        }

        #region Settings

        // Properties to store naming settings
        public string SheetNamingFormat { get; set; }
        public string ViewNamingFormat { get; set; }
        public string SheetDelimiter { get; set; } = "_";  // Default delimiter
        public string ViewDelimiter { get; set; } = "_";   // Default delimiter
        public bool UseLettersForViewNumbering { get; set; } = true;

        // Selected types and templates
        public ElementId SelectedTitleBlockTypeId { get; set; }
        public ElementId SelectedElevationTypeId { get; set; }
        public ElementId SelectedFloorViewTemplateId { get; set; }
        public ElementId SelectedCeilingViewTemplateId { get; set; }
        public ElementId SelectedElevationViewTemplateId { get; set; }

        public void SaveSettings()
        {
            try
            {
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));

                // Create XML document with settings
                XDocument doc = new XDocument(
                    new XElement("RoomStudiesSettings",
                        new XElement("NamingSettings",
                            new XElement("SheetNamingFormat", SheetNamingFormat ?? string.Empty),
                            new XElement("ViewNamingFormat", ViewNamingFormat ?? string.Empty),
                            new XElement("SheetDelimiter", SheetDelimiter ?? "_"),
                            new XElement("ViewDelimiter", ViewDelimiter ?? "_"),
                            new XElement("UseLettersForNumbering", UseLettersForViewNumbering.ToString())
                        ),
                        new XElement("SelectedTypes",
                            new XElement("TitleBlockTypeId", SelectedTitleBlockTypeId?.Value.ToString() ?? string.Empty),
                            new XElement("ElevationTypeId", SelectedElevationTypeId?.Value.ToString() ?? string.Empty),
                            new XElement("FloorViewTemplateId", SelectedFloorViewTemplateId?.Value.ToString() ?? string.Empty),
                            new XElement("CeilingViewTemplateId", SelectedCeilingViewTemplateId?.Value.ToString() ?? string.Empty),
                            new XElement("ElevationViewTemplateId", SelectedElevationViewTemplateId?.Value.ToString() ?? string.Empty)
                        )
                    )
                );

                // Save to file
                doc.Save(SettingsFilePath);
                Debug.WriteLine($"Settings saved to {SettingsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void LoadSettings()
        {
            if (!File.Exists(SettingsFilePath))
            {
                Debug.WriteLine("No settings file found. Using defaults.");
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(SettingsFilePath);

                // Load naming settings
                var namingSettings = doc.Root.Element("NamingSettings");
                if (namingSettings != null)
                {
                    SheetNamingFormat = namingSettings.Element("SheetNamingFormat")?.Value;
                    ViewNamingFormat = namingSettings.Element("ViewNamingFormat")?.Value;

                    SheetDelimiter = namingSettings.Element("SheetDelimiter")?.Value ?? "_";
                    ViewDelimiter = namingSettings.Element("ViewDelimiter")?.Value ?? "_";

                    var useLettersElement = namingSettings.Element("UseLettersForNumbering");
                    if (useLettersElement != null && bool.TryParse(useLettersElement.Value, out bool useLetters))
                    {
                        UseLettersForViewNumbering = useLetters;
                    }
                }

                // Load selected types
                var selectedTypes = doc.Root.Element("SelectedTypes");
                if (selectedTypes != null)
                {
                    var titleBlockId = selectedTypes.Element("TitleBlockTypeId")?.Value;
                    if (!string.IsNullOrEmpty(titleBlockId) && int.TryParse(titleBlockId, out int titleBlockIntId))
                    {
                        SelectedTitleBlockTypeId = new ElementId((Int64)titleBlockIntId);
                    }

                    var elevationTypeId = selectedTypes.Element("ElevationTypeId")?.Value;
                    if (!string.IsNullOrEmpty(elevationTypeId) && int.TryParse(elevationTypeId, out int elevationIntId))
                    {
                        SelectedElevationTypeId = new ElementId((Int64)elevationIntId);
                    }

                    var floorViewTemplateId = selectedTypes.Element("FloorViewTemplateId")?.Value;
                    if (!string.IsNullOrEmpty(floorViewTemplateId) && int.TryParse(floorViewTemplateId, out int floorIntId))
                    {
                        SelectedFloorViewTemplateId = new ElementId((Int64)floorIntId);
                    }

                    var ceilingViewTemplateId = selectedTypes.Element("CeilingViewTemplateId")?.Value;
                    if (!string.IsNullOrEmpty(ceilingViewTemplateId) && int.TryParse(ceilingViewTemplateId, out int ceilingIntId))
                    {
                        SelectedCeilingViewTemplateId = new ElementId((Int64)ceilingIntId);
                    }

                    var elevationViewTemplateId = selectedTypes.Element("ElevationViewTemplateId")?.Value;
                    if (!string.IsNullOrEmpty(elevationViewTemplateId) && int.TryParse(elevationViewTemplateId, out int elevationTemplateIntId))
                    {
                        SelectedElevationViewTemplateId = new ElementId((Int64)elevationTemplateIntId);
                    }
                }

                Debug.WriteLine("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        #endregion
    }
}