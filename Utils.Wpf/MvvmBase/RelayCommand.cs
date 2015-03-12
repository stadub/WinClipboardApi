using System;
using System.Windows.Input;

namespace Utils.Wpf.MvvmBase
{
    public class RelayCommand : IRelayCommand
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

        public bool CanExecute()
        {
            var newStatus = canExecute();
            if (newStatus == status)
                return newStatus;

            status = newStatus;
            OnCanExecuteChanged();
            return newStatus;
        }

        public void RefreshCanExecute()
        {
            CanExecute();
        }

        public void Execute()
        {
            if (!CanExecute())
                return;
            execute();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }

    public interface IRelayCommand : ICommand
    {
        bool CanExecute();
        void RefreshCanExecute();
        void Execute();
    }
}