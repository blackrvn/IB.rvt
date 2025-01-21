using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Interfaces
{
    public interface IDocumentService
    {
        Document GetDocument();
        UIDocument GetUIDocument();
    }
}
