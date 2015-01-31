using System.ComponentModel;
using System.Windows;
using Utils;

namespace ClipboardViewer.MvvmBase
{
    public class ViewModelLocatorBase
    {
        public ViewModelLocatorBase():this(new ServiceLocator())
        {
            
        }

        public ViewModelLocatorBase(ServiceLocator container)
        {
            Container = container;
        }

        public ServiceLocator Container { get; protected set; }

        public void Register<TView,TViewModel>() where TView:FrameworkElement where TViewModel : ViewModelBase
        {
            var viewType = typeof (TView);
            Container.RegisterType<ViewModelBase, TViewModel>(viewType.Name);
        }

        public TViewModel Resolve<TView,TViewModel>() where TView:FrameworkElement where TViewModel : ViewModelBase
        {
            var viewType = typeof (TView);
            return Container.ResolveType<ViewModelBase>(viewType.Name) as TViewModel;
        }
    }
}