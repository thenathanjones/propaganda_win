using System;
using System.Windows.Input;

namespace Propaganda.Core.Util
{
    public class ActionCommand : ICommand
    {
        private readonly Action _toInvoke;
        private readonly Func<bool> _canExecute;

        public ActionCommand(Action toInvoke)
        {
            _toInvoke = toInvoke;
        }

        public ActionCommand(Action toInvoke, Func<bool> canExecute)
        {
            _toInvoke = toInvoke;
            _canExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            else
                return _canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_toInvoke != null)
                _toInvoke.Invoke();
        }

        #endregion
    }
}