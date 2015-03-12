using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardViewer.ViewModel;
using ClipboardViewer.ViewModel.DesignTime;
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
            
            //container.RegisterInitializer<IClipboard>(() => Clipboard.CreateReadWrite(watcher));
            //container.RegisterType<IClipboard, Clipboard>();
            var bootstraper = this.FindResource("Bootstraper");
#if CheckInit
            container = (ServiceLocator)this.FindResource("ServiceLocator");

            Debug.Assert(container != null, "ServiceLocator doesn't exist in application resources");

            CheckInitDataProvider("Bootstraper");
            CheckInitDataProvider("Bootstraper.Container");
            CheckInitDataProvider("ViewModelLocator");

#endif
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                RewriteOriginalByDesignMode("Bootstraper");
                RewriteOriginalByDesignMode("ViewModelLocator");
               
            }

            base.OnStartup(e);

        }

        private object CheckInitDataProvider(string name)
        {
            ObjectDataProvider dObject = (ObjectDataProvider)this.FindResource(name);
            Debug.Assert(dObject != null,  name + " is not found or incorrect format");
            var objectInstance = dObject.ObjectInstance;
            Debug.Assert(objectInstance != null,  name + " has incorrect type or null");
            return objectInstance;
        }

        private void RewriteOriginalByDesignMode(string name)
        {
            var objectInstance=CheckInitDataProvider("d:" + name);

            ObjectDataProvider origObject = (ObjectDataProvider)this.FindResource(name);
            Debug.Assert(origObject != null, name + " is not found or incorrect format");
            origObject.ObjectInstance = objectInstance;
        }


        protected override void OnExit(ExitEventArgs e)
        {
            if (container!=null)
                container.Dispose();
            base.OnExit(e);
        }

    }


}
