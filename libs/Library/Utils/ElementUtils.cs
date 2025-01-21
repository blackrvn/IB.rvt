using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Utils
{
    public static class ElementUtils
    {
        public static ElementId GetCropBoxOfView(View view)
        {
            ParameterValueProvider provider = new(new ElementId((Int64)BuiltInParameter.ID_PARAM));
            FilterElementIdRule rule = new(provider, new FilterNumericEquals(), view.Id);
            ElementParameterFilter filter = new(rule);
            return new FilteredElementCollector(view.Document)
                .WherePasses(filter)
                .ToElementIds()
                .Where<ElementId>(a => a.Value != view.Id.Value)
                .FirstOrDefault<ElementId>();
        }
    }
}
