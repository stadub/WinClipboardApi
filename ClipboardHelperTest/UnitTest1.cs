using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClipboardHelper;
using ClipboardHelperTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clipboard = ClipboardHelper.Clipboard;
using Clpbrd=System.Windows.Forms.Clipboard;

namespace ClipbordHelperTest
{
    [TestClass]
    public class UnitTest1
    {



        private const string testString = "Lorem ipsum dolor sit amet";

        [TestMethod]
        public void ShoudlClearClipbord()
        {
            Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            Assert.AreEqual(Clpbrd.GetText(TextDataFormat.UnicodeText), testString);
            var clipboard = new Clipboard();
            clipboard.Open();
            clipboard.Clear();
            clipboard.Close();
            Assert.IsTrue(!Clpbrd.ContainsText());

        }
        [TestMethod]
        public void ShoudlReceiveUnicodeStringFromClipbord()
        {
            
            Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            Assert.AreEqual(Clpbrd.GetText(TextDataFormat.UnicodeText),testString);
            string data;
            using (var clipboard = new Clipboard())
            {
                var provider = new UnicodeTextProvider();
                var dataAvalable = clipboard.IsDataAvailable(provider);
                Assert.IsTrue(dataAvalable);
                clipboard.Open();
                data = clipboard.GetData(provider);
                clipboard.Close();
            }
            
            Assert.AreEqual(data, testString);
        }

        [TestMethod]
        public void ShoudlSaveToClipboardUnicodeString()
        {
            WinformWrapper wrapper = new WinformWrapper();
            wrapper.CreateWindow();
            Thread.Sleep(100);

            using (var clipboard = new Clipboard())
            {
                var provider = new UnicodeTextProvider();
                clipboard.Open(wrapper.Handle);
                clipboard.Clear();
                clipboard.SetData(testString,provider);
                clipboard.Close();
            }

            Assert.AreEqual(Clpbrd.GetText(TextDataFormat.UnicodeText), testString);
        }

        [TestMethod]
        public void ShoudlReceiveUnicodeClipbordFormat()
        {
            Clpbrd.SetText("123", TextDataFormat.UnicodeText);
            //WinformWrapper wrapper= new WinformWrapper();
            //wrapper.CreateWindow();
            //Thread.Sleep(100);
            List<uint> formats;
            using (var clipboard = new Clipboard())
            {
                clipboard.Open();

                formats = clipboard.GetAvalibleFromats();
                clipboard.Close();
            }
            Assert.IsTrue(formats.Contains((uint)StandartClipboardFormats.UnicodeText));
        }

        [TestMethod]
        [ExpectedException(typeof(ClipboardClosedException))]
        public void ShoudlThrowClipboardClosedExceptionWhenClipboardNotOpened()
        {
            using (var clipboard = new Clipboard())
            {
                //clipboard.Open();

                clipboard.GetAvalibleFromats();
                //clipboard.Close();
            }
            
        }

    }
}
