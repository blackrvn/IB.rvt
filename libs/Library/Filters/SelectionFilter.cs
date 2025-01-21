using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Filters
{
    public class BuiltInCategorySelectionFilter : ISelectionFilter
    {
        private BuiltInCategory BuiltInCategory {  get; set; }
        public BuiltInCategorySelectionFilter(BuiltInCategory builtInCategory )
        {
            if (builtInCategory != BuiltInCategory.INVALID)
            {
                BuiltInCategory = builtInCategory;
            }
        }
        public bool AllowElement(Element elem)
        {
            if (elem != null && elem.Category.BuiltInCategory == BuiltInCategory)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
