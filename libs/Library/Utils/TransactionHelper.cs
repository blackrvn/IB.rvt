using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;


namespace Library.Utils
{
    public class TransactionHelper : IDisposable
    {
        private readonly Document _doc;
        private TransactionGroup _group;
        private bool _disposed;

        public TransactionHelper(Document doc, string groupName)
        {
            _doc = doc ?? throw new ArgumentException($"Argument was not valid: doc");
            _group = new(_doc, groupName);
            _group.Start();
        }

        public TransactionGroup GetTransactionGroup()
        {
            return _group;
        }

        public void Execute(Action action, string transactionName = "Internal transaction")
        {
            using var t = new Transaction(_doc, transactionName);
            t.Start();
            try
            {
                action.Invoke();
                t.Commit();
            }
            catch
            {
                t.RollBack();
                throw;
            }
        }

        public void Execute(ICollection<Action> actions, string transactionName = "Internal transaction")
        {
            using var t = new Transaction(_doc, transactionName);
            t.Start();
            try
            {
                foreach (Action action in actions)
                {
                    action.Invoke();
                }

                t.Commit();
            }
            catch
            {
                t.RollBack();
                throw;
            }

        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _group.Assimilate();
                _group = null;
                _disposed = true;
            }
        }
    }
}
