using Autodesk.Revit.UI;
using Library.Interfaces;


namespace Core.Services
{
    public class DocumentService(UIApplication uIApplication) : IDocumentService
    {
        private readonly UIApplication _uiApplication = uIApplication;

        public Document GetDocument()
        {
            return _uiApplication.ActiveUIDocument.Document;
        }

        public UIDocument GetUIDocument()
        {
            return _uiApplication.ActiveUIDocument;
        }
    }
}
