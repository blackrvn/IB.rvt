using Library.ViewModels;
using System.Diagnostics;
using System.Xml;


namespace Library.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        // UI Strings
        public string CommandName { get; private set; }
        public string ButtonOKName { get; private set; }
        public string ButtonCancelName { get; private set; }

        // Commands
        public IRelayCommand OkCommand { get; private set; }
        public IRelayCommand CancelCommand { get; private set; }
        public IRelayCommand OnMouseHoverCommad { get; private set; }

        // Events
        public event Action RequestClose;

        public BaseViewModel()
        {
        }

        public virtual void InitializeCommands()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }


        public virtual void InitializeUIStrings(string commandName, string okName, string cancelName)
        {
            CommandName = commandName;
            ButtonOKName = okName;
            ButtonCancelName = cancelName;
        }

        public virtual void Cancel()
        {
            Debug.WriteLine("Cancel");
            RequestClose?.Invoke();
        }

        public virtual void Ok()
        {
            Debug.WriteLine("OK");
            Cancel();
        }

    }
}
