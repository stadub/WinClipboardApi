using System.Threading;
using System.Windows.Forms;
using ClipboardHelper;
using ClipboardHelper.Win32;
using ClipboardHelperTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clipboard = ClipboardHelper.Clipboard;
using Clpbrd = System.Windows.Forms.Clipboard;

namespace ClipbordHelperTest
{
    [TestClass]
    public class ClipbordWatcherTest
    {
        private const string testString = "Lorem ipsum dolor sit amet";

        [TestMethod]
        public void WatcherTest()
        {
            ClipbordWatcher watcher = new ClipbordWatcher();
            watcher.Start();

            watcher.OnClipboarCopyDataSent += (sender, args) => { };
            watcher.OnClipboardContentChanged += (sender, args) => { };
            watcher.OnClipboardContentDestroy += (sender, args) => { };
            watcher.OnMessageWindowHwndReceived += (sender, args) => { };
            watcher.OnRenderFormatRequested += (sender, args) => { };

            Clpbrd.SetText(testString, TextDataFormat.UnicodeText);

            WinformWrapper wrapper = new WinformWrapper();
            wrapper.CloseWindow();
            wrapper.CreateWindow();
            Thread.Sleep(100);

            using (var clipboard = Clipboard.CreateReadWrite(wrapper.Handle))
            {
                var provider = new UnicodeTextProvider();
                clipboard.Open();
                clipboard.Clear();
                clipboard.SetData(testString, provider);
                clipboard.Close();
            }
            Thread tr = new Thread(() =>
            {
                Thread.Sleep(100);
                Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            });
            tr.SetApartmentState(ApartmentState.STA);
            tr.Start();


            foreach (var a in watcher.WaitClipboardData())
            {

            }


            watcher.Dispose();
        }
    }
}
