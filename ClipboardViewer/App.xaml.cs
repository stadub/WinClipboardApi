using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using Utils;
using Clipboard = ClipboardHelper.Clipboard;

namespace ClipboardViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ServiceLocator container;
        protected override void OnStartup(StartupEventArgs e)
        {

            //ClipbordWatcher watcher = new ClipbordWatcher();
            //watcher.StartListen();
            //var acessibleFromats = Providers.GetCurrentProviders().ToList();

            //container.RegisterInstance<IClipbordWatcher, ClipbordWatcher>(watcher);
            //container.RegisterInitializer<IClipboard>(() => Clipboard.CreateReadWrite(watcher));
            //container.RegisterType<IClipboard, Clipboard>();


            var accessibleFromats = Providers.GetCurrentProviders().ToList();


            container = (ServiceLocator)this.FindResource("ServiceLocator");

            Debug.Assert(container != null, "ServiceLocator doesn't exist in application resources");
            container.RegisterInitializer<IClipboard>(Clipboard.CreateReadOnly);

            container.RegisterInstance<IEnumerable<Func<IClipbordFormatProvider>>, List<Func<IClipbordFormatProvider>>>(accessibleFromats);

            ObjectDataProvider vmLocatorObject = (ObjectDataProvider)this.FindResource("ViewModelLocator");
            Debug.Assert(vmLocatorObject != null, "ViewModelLocator doesn't exist in application resources");
            ViewModelLocator vmLocator = (ViewModelLocator) vmLocatorObject.ObjectInstance;


            Debug.Assert(vmLocator != null, "vmLocatorObject has incorrect type");

            base.OnStartup(e);

        }



        protected override void OnExit(ExitEventArgs e)
        {
            container.Dispose();
            base.OnExit(e);
        }

    }


}
