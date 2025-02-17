using Library.Models;
using System;
using System.Collections.Generic;
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
    }
}
