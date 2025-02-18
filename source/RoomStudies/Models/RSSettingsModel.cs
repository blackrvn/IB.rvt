using Library.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomStudies.Models
{
    public class RSSettingsModel : ModelBase
    {


        public RSSettingsModel()
        {

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
                .Where(Element => Element is View view && view.IsTemplate && view.ViewType==viewType)
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

            if (types.GetElementCount() >0)
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
    }
}
