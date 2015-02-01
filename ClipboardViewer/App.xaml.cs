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

            container = (ServiceLocator)this.FindResource("ServiceLocator");

            Debug.Assert(container != null, "ServiceLocator doesn't exist in application resources");
#if CheckInit
            ObjectDataProvider vmLocatorObject = (ObjectDataProvider)this.FindResource("ViewModelLocator");
            Debug.Assert(vmLocatorObject != null, "ViewModelLocator doesn't exist in application resources");
            ViewModelLocator vmLocator = (ViewModelLocator)vmLocatorObject.ObjectInstance;
            Debug.Assert(vmLocator != null, "vmLocatorObject has incorrect type");
#endif
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                RewriteOriginalByDesignMode("Bootstraper");
                RewriteOriginalByDesignMode("ViewModelLocator");
               
            }

            ObjectDataProvider bootstraperObject = (ObjectDataProvider)this.FindResource("Bootstraper");
            Debug.Assert(bootstraperObject != null, "Bootstraper doesn't exist in application resources");
            Bootstraper bootstraper = (Bootstraper)bootstraperObject.ObjectInstance;
            Debug.Assert(bootstraper != null, "Bootstraper has incorrect type");

            base.OnStartup(e);

        }


        private void RewriteOriginalByDesignMode(string name)
        {
            ObjectDataProvider dObject = (ObjectDataProvider)this.FindResource("d:"+name);
            Debug.Assert(dObject != null, "d:"+name +" is not found or incorrect format");
            var objectInstance = dObject.ObjectInstance;
            Debug.Assert(objectInstance != null, "d:"+name + "   has incorrect type or null");


            ObjectDataProvider origObject = (ObjectDataProvider)this.FindResource(name);
            Debug.Assert(origObject != null, name + " is not found or incorrect format");
            origObject.ObjectInstance = objectInstance;
        }


        protected override void OnExit(ExitEventArgs e)
        {
            container.Dispose();
            base.OnExit(e);
        }

    }


}
