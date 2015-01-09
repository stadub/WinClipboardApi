using System;
using System.Windows.Input;

namespace SkypeHelper.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        private bool status;
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            if (canExecute != null)
                this.canExecute = canExecute;
            else
                this.canExecute = () => true;
            status = true;
        }

        public bool CanExecute(object parameter)
        {
            var newStatus = canExecute();
            if (newStatus == status)
                return newStatus;

            status = newStatus;
            OnCanExecuteChanged();
            return newStatus;
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;
            execute();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}