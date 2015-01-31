using System;
using System.Threading;
using System.Windows.Forms;
using ClipboardHelper;
using ClipboardHelper.Watcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clipboard = ClipboardHelper.Clipboard;
using Clpbrd = System.Windows.Forms.Clipboard;

namespace ClipboardHelperTest
{
    [TestClass]
    public class ClipbordWatcherTest
    {
        private const string testString = "Lorem ipsum dolor sit amet";

        public ClipbordWatcher StartWatcher()
        {
            var watcher = new ClipbordWatcher();
            Clpbrd.Clear();
            watcher.StartListen();
            return watcher;
        }

        public void WaitWatcher(ClipbordWatcher watcher,Action Action, int milliseconds)
        {
            ManualResetEvent waiter= new ManualResetEvent(false);
            var tr = new Thread(() =>
            {
                if (!watcher.IsListenerStarted)
                    watcher.StartListen();
                
                Action();
                waiter.Set();
            });
            tr.SetApartmentState(ApartmentState.STA);
            tr.Start();

            WaitHandle.WaitAny(new WaitHandle[] {waiter}, milliseconds);
            tr.Abort();
            watcher.Stop();
        }


        [TestMethod]
        public void ShouldReceiveClipbordChageEvent()
        {
            var watcher = StartWatcher();

            bool received = false;

            uint curr = ClipbordWatcher.SequenceNumber;

            WaitWatcher(watcher, () =>
            {
                watcher.OnClipboardContentChanged += (sender, args) => received = args.Value > curr;
                Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            }, 1000);

            Assert.IsTrue(received);
        }


        [TestMethod]
        public void ShouldReceiveDestroyMessage()
        {
            var watcher = StartWatcher();

            using (var clipboard = Clipboard.CreateReadWrite(watcher))
            {
                var provider = new UnicodeTextProvider();
                clipboard.Open();
                clipboard.Clear();
                provider.Text = testString;
                clipboard.SetData(provider);
            }

            bool destroyMsgReceived = false;

            WaitWatcher(watcher, () =>{
                watcher.OnClipboardContentDestroy += (sender, args) =>
                {
                    destroyMsgReceived = true;
                };
                Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            },1000);

            Assert.IsTrue(destroyMsgReceived);
        }

        [TestMethod]
        public void ShouldReceiveFormatRequestedMessage()
        {
            var watcher = new ClipbordWatcher();

            watcher.StartListen();
            string text=null;
            using (var clipboard = Clipboard.CreateReadWrite(watcher))
            {
                var provider = new UnicodeTextProvider();
                clipboard.Open();
                clipboard.Clear();
                clipboard.EnrolDataFormat(provider);
            }

            bool renderFormatRequested = false;


            WaitWatcher(watcher, () =>
            {
                watcher.OnRenderFormatRequested += (sender, args) =>
                {
                    renderFormatRequested = true;
                };
                text = Clpbrd.GetText();
            }, 1000);

            Assert.IsTrue(string.IsNullOrWhiteSpace(text));
        }
        
        
        [TestMethod]
        public void ShouldRequestRenderFormat()
        {
            var watcher = new ClipbordWatcher();

            watcher.StartListen();
            string text=null;
            var provider = new UnicodeTextProvider();

            using (var clipboard = Clipboard.CreateReadWrite(watcher))
            {
                
                clipboard.Open();
                clipboard.Clear();
                clipboard.EnrolDataFormat(provider);
            }

            bool renderFormatRequested = false;

           WaitWatcher(watcher, () =>
            {
                                      
                watcher.OnRenderFormatRequested += (sender, args) =>
                {
                    renderFormatRequested = true;
                    var clipboard = Clipboard.CreateReadWrite(watcher);
                    provider.Text = testString;
                    clipboard.SetRequestedData(provider);
                };
                Thread.Sleep(100);
                text = Clpbrd.GetText(TextDataFormat.UnicodeText);
                Thread.Sleep(100);
   
            },1000);

            Assert.IsTrue(text == testString);
        }

        [TestMethod]
        public void ShouldReceiveDestroyMessageOnTimeout()
        {
            var watcher = StartWatcher();

            using (var clipboard = Clipboard.CreateReadWrite(watcher))
            {
                var provider = new UnicodeTextProvider();
                clipboard.Open();
                clipboard.Clear();
                provider.Text = testString;
                clipboard.SetData(provider);
            }

            bool destroyMsgReceived = false;

            WaitWatcher(watcher, () =>
            {
                watcher.OnClipboardContentDestroy += (sender, args) =>
                {
                    destroyMsgReceived = true;
                };
                Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            }, 1000);

            Assert.IsTrue(destroyMsgReceived);
        }

    }
}
