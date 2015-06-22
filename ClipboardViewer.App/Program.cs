using System;
using Utils;

namespace ClipboardViewer.Boot
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            var bootstraper= new Bootstraper();
            bootstraper.InitServiceLocator();

            var vmLocator = new ViewModelLocator(bootstraper.Container);

            var a = new App();
            a.InitializeComponent();
            a.Resources["ServiceLocator"] = bootstraper.Container;
            a.Resources["ViewModelLocator"] = vmLocator;

            a.Run();
            bootstraper.Dispose();
        }

    }
}