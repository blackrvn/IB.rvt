using Autodesk.Revit.DB;
using Library.Models;
using RoomStudies.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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

        public ICollection<ViewFamilyType> GetViewTypes(ViewFamily viewFamilyType)
        {
            return new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .Where(vft => vft.ViewFamily == viewFamilyType)
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

        public bool IsValidTypeId(ElementId elementId, BuiltInCategory builtInCategory)
        {
            Element element = Doc.GetElement(elementId);
            if (element == null)
            {
                return false;
            }
            return element.Category.BuiltInCategory == builtInCategory;
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

        public T FindElementById<T>(IEnumerable<T> collection, ElementId id) where T : Element
        {
            if (id == null || id == ElementId.InvalidElementId)
                return null;

            return collection.FirstOrDefault(e => e.Id.Value == id.Value);
        }
        private Parameter GetParametersOfElementById(Element element, ElementId id)
        {
            return element.Parameters.Cast<Parameter>().Where(p => p.Id == id).FirstOrDefault();
        }

        public List<Parameter> DecodeFormat(Element element, string format)
        {
            List<Parameter> parameters = [];
            string[] parts = format.Split(new[] {","}, StringSplitOptions.None);

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                // Try to convert the part to a long ID
                if (long.TryParse(part, out long id))
                {
                    Parameter parameter = GetParametersOfElementById(element, new ElementId(id));
                    if (parameter != null)
                    {
                        parameters.Add(parameter);
                    }
                    else
                    {
                        ProjectInfo projectInfo = Doc.ProjectInformation;
                        parameter = projectInfo.Parameters.Cast<Parameter>().Where(p => p.Id == new ElementId(id)).FirstOrDefault();
                        parameters.Add(parameter);
                    }
                }
            }
            return parameters;
        }


        #region Settings

        // Properties to store naming settings
        public string SheetNamingFormat { get; set; }
        public string ViewNamingFormat { get; set; }
        public string SheetDelimiter { get; set; }
        public string ViewDelimiter { get; set; }
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
                    if (string.IsNullOrEmpty(SheetNamingFormat))
                    {
                        SheetNamingFormat = "-1006901,-1006900,-1006916";
                    }
                    ViewNamingFormat = namingSettings.Element("ViewNamingFormat")?.Value;
                    if (string.IsNullOrEmpty(ViewNamingFormat))
                    {
                        ViewNamingFormat = "-1006901,-1006900,-1006916";
                    }

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
                    if (int.TryParse(titleBlockId, out int titleBlockIntId))
                    {
                        if (!string.IsNullOrEmpty(titleBlockId) && (IsValidTypeId(SelectedTitleBlockTypeId, BuiltInCategory.OST_TitleBlocks)))
                        {
                            SelectedTitleBlockTypeId = new ElementId((Int64)titleBlockIntId);
                        }
                        else if (string.IsNullOrEmpty(titleBlockId) || (!IsValidTypeId(SelectedTitleBlockTypeId, BuiltInCategory.OST_TitleBlocks)))
                        {
                            SelectedTitleBlockTypeId = GetTypes(BuiltInCategory.OST_TitleBlocks).First().Id;
                        }
                    }

                    var elevationTypeId = selectedTypes.Element("ElevationTypeId")?.Value;
                    if (int.TryParse(elevationTypeId, out int elevationIntId))
                    {
                        if (!string.IsNullOrEmpty(elevationTypeId))
                        {
                            SelectedElevationTypeId = new ElementId((Int64)elevationIntId);
                        }
                        else if (string.IsNullOrEmpty(elevationTypeId))
                        {
                            SelectedElevationTypeId = new FilteredElementCollector(Doc)
                                                        .OfClass(typeof(ViewFamilyType))
                                                        .Cast<ViewFamilyType>()
                                                        .FirstOrDefault(v => v.FamilyName == Localization.LogicElements.RoomStudies_ElevationFamilyName &&
                                                                                v.FindParameter(BuiltInParameter.ALL_MODEL_TYPE_NAME)
                                                                                .AsValueString() == Localization.LogicElements.RoomStudies_ElevationTypeName)
                                                        ?.Id ?? ElementId.InvalidElementId;
                        }
                    }

                    var floorViewTemplateId = selectedTypes.Element("FloorViewTemplateId")?.Value;
                    if (int.TryParse(floorViewTemplateId, out int floorIntId))
                    {
                        if (!string.IsNullOrEmpty(floorViewTemplateId))
                        {
                            SelectedFloorViewTemplateId = new ElementId((Int64)floorIntId);
                        }
                        else if (string.IsNullOrEmpty(floorViewTemplateId))
                        {
                            SelectedFloorViewTemplateId = GetViewTemplates(GetElementIds(BuiltInCategory.OST_Views), ViewType.FloorPlan).First().Id;
                        }
                    }

                    var ceilingViewTemplateId = selectedTypes.Element("CeilingViewTemplateId")?.Value;
                    if (int.TryParse(ceilingViewTemplateId, out int ceilingIntId))
                    {
                        if (!string.IsNullOrEmpty(ceilingViewTemplateId))
                        {
                            SelectedCeilingViewTemplateId = new ElementId((Int64)ceilingIntId);
                        }
                        else if (string.IsNullOrEmpty(ceilingViewTemplateId))
                        {
                            SelectedCeilingViewTemplateId = GetViewTemplates(GetElementIds(BuiltInCategory.OST_Views), ViewType.CeilingPlan).First().Id;
                        }
                    }

                    var elevationViewTemplateId = selectedTypes.Element("ElevationViewTemplateId")?.Value;
                    if (int.TryParse(elevationViewTemplateId, out int elevationTemplateIntId))
                    {
                        if (!string.IsNullOrEmpty(elevationViewTemplateId))
                        {
                            SelectedElevationViewTemplateId = new ElementId((Int64)elevationTemplateIntId);
                        }
                        else if (string.IsNullOrEmpty(elevationViewTemplateId))
                        {
                            SelectedElevationViewTemplateId = GetViewTemplates(GetElementIds(BuiltInCategory.OST_Views), ViewType.Elevation).First().Id;
                        }
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