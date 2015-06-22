using System;
using System.Threading;
using System.Windows.Forms;
using ClipboardHelper.FormatProviders;
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
            var clipboard = new Clipboard();
            var provider = new UnicodeTextProvider();
            provider.Text = testString;

            using (var clipboardWriter = clipboard.CreateWriter(watcher))
            {
                clipboardWriter.Clear();

                clipboardWriter.SetData(provider);
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
            var clipboard = new Clipboard();
            using (var clipboardWriter = clipboard.CreateWriter(watcher))
            {
                var provider = new UnicodeTextProvider();
                clipboardWriter.Clear();
                clipboardWriter.EnrolDataFormat(provider);
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

            var clipboard = new Clipboard();
            using (var clipboardWriter = clipboard.CreateWriter(watcher))
            {
                clipboardWriter.Clear();
                clipboardWriter.EnrolDataFormat(provider);
            }

            bool renderFormatRequested = false;

           WaitWatcher(watcher, () =>
            {
                                      
                watcher.OnRenderFormatRequested += (sender, args) =>
                {
                    renderFormatRequested = true;
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

            var clipboard = new Clipboard();
            using (var clipboardWriter = clipboard.CreateWriter(watcher))
            {
                var provider = new UnicodeTextProvider();
                clipboardWriter.Clear();
                provider.Text = testString;
                clipboardWriter.SetData(provider);
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
