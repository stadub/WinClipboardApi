using ClipboardViewer.MvvmBase;
using ClipboardViewer.ViewModel;
using Utils;

namespace ClipboardViewer
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public ViewModelLocator(ServiceLocator container): base(container)
        {
            Register<MainWindow, MainWindowViewModel>();
        }


        public MainWindowViewModel MainPage
        {
            get { return base.Resolve<MainWindow, MainWindowViewModel>(); }
        }
    }
}
