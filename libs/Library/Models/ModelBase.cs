using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;


namespace Library.Models
{
    public class ModelBase
    {
        internal Document _doc;
        public Transaction ModelTransaction { get; set; }

        public Document Doc
        {
            get
            {
                return _doc; 
            }
            private set
            {
                _doc = Context.ActiveDocument;
            }

        }

        internal UIDocument _uiDocument;
        public UIDocument UIDocument
        {
            get
            {
                return _uiDocument;
            }
            private set
            {
                _uiDocument = Context.ActiveUiDocument;
            }
        }

        public ModelBase()
        {
            Doc = Context.ActiveDocument;
            UIDocument = Context.ActiveUiDocument;

        }
    }

}
