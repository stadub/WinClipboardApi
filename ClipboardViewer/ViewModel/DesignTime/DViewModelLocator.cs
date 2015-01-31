using Utils;
using Utils.Wpf.MvvmBase;

namespace ClipboardViewer.ViewModel.DesignTime
{
    public class DViewModelLocator : ViewModelLocatorBase
    {
        public DViewModelLocator(ServiceLocator container): base(container)
        {
            Register<MainWindow, DMainWindowViewModel>();
        }

        public DMainWindowViewModel MainPage
        {
            get { return base.Resolve<MainWindow, DMainWindowViewModel>(); }
        }
    }
}