using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Win32;
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
            clipboard.OpenRead();
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
                clipboard.OpenRead();
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
            IEnumerable<IClipbordFormatProvider> formats;
            using (var clipboard = new Clipboard())
            {
                clipboard.OpenRead();

                formats = clipboard.GetAvalibleFromats(new []{new UnicodeTextProvider()}).ToList();
                clipboard.Close();
            }
            Assert.IsTrue(formats.Any(provider => provider.FormatId == "CF_UNICODETEXT"));
        }
        
        [TestMethod]
        public void ShoudlReturnUncnowneClipbordFormat()
        {
            Clpbrd.SetText("123", TextDataFormat.UnicodeText);
            //WinformWrapper wrapper= new WinformWrapper();
            //wrapper.CreateWindow();
            //Thread.Sleep(100);
            IList<IClipbordFormatProvider> formats;
            using (var clipboard = new Clipboard())
            {
                clipboard.OpenRead();

                formats = clipboard.GetAvalibleFromats(new IClipbordFormatProvider<int>[] { }).ToList();
                clipboard.Close();
            }
            Assert.IsTrue(formats[0] is UnknownFormatProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ClipboardClosedException))]
        public void ShoudlThrowClipboardClosedExceptionWhenClipboardNotOpened()
        {
            using (var clipboard = new Clipboard())
            {
                //clipboard.OpenRead();

                var formats=clipboard.GetAvalibleFromats(new[] { new UnicodeTextProvider() });
                //clipboard.Close();
            }
            
        }
        [TestMethod]
        [ExpectedException(typeof(ClipboardClosedException))]
        public void ShoudlThrowClipboardClosedExceptionWhenTryingToEnumerateAfterClose()
        {
            using (var clipboard = new Clipboard())
            {
                clipboard.OpenRead();

                var formats=clipboard.GetAvalibleFromats(new[] { new UnicodeTextProvider() });
                clipboard.Close();
                formats.ToList();
            }
            
        }

        private const string TextHtmlData = @"Version:1.0
StartHTML:000000194
EndHTML:000001170
StartFragment:000000493
EndFragment:000001112
StartSelection:000000507
EndSelection:000001108
SourceURL:res://iesetup.dll/HardAdmin.htm
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN\"">
<HTML dir=ltr><HEAD><TITLE>Lorem ipsum dolor sit amet</TITLE><LINK 
rel=stylesheet type=text/css href=""adipiscing.css"" media=screen></HEAD>
<BODY>
<TABLE>
<TBODY>
<TR>
<TD class=ie><!--StartFragment-->
<P id=Text1>Pellentesque imperdiet consequat lectus sed accumsan. <A id=porta
title=""consectetur"" href=""http://www.w3.org"">Cras et arcu id dui eleifend euismod.</A>.</P>
<P id=Text2> Praesent eu turpis sem.</P><!--EndFragment--></TD></TR></TBODY></TABLE></BODY></HTML> ";

        [TestMethod]
        public void ShoudlDeserializeSampleHtmlData()
        {
            UnicodeStringSerializer serializer=new UnicodeStringSerializer();
            var bytes=serializer.Serialize(TextHtmlData);

            HtmlFormatProvider provider= new HtmlFormatProvider();
            var result=provider.Deserialize(bytes);

            Assert.AreEqual(result.StartHTML, 000000194);
            Assert.AreEqual(result.EndHTML,000001170);
            Assert.AreEqual(result.StartFragment,000000493);
            Assert.AreEqual(result.EndFragment,000001112);
            Assert.AreEqual(result.StartSelection,000000507);
            Assert.AreEqual(result.EndSelection,000001108);
            Assert.AreEqual(result.SourceURL, new Uri("res://iesetup.dll/HardAdmin.htm"));
        }
        public static Regex htmlDoc = new Regex(
            "^(<.*)",
          RegexOptions.Multiline
          | RegexOptions.Singleline
          | RegexOptions.CultureInvariant
          | RegexOptions.Compiled
          );
        [TestMethod]
        public void ShoudlSerializeSampleHtmlData()
        {
            string[] results = htmlDoc.Split(TextHtmlData);

            var data = new HtmlClipboardFormatData
            {
                Version = "1.0",
                StartHTML = 000000194,
                EndHTML = 000001170,
                StartFragment = 000000493,
                EndFragment = 000001112,
                StartSelection = 000000507,
                EndSelection = 000001108,
                SourceURL = new Uri("res://iesetup.dll/HardAdmin.htm"),
                Html = results[1]
            };

            HtmlFormatProvider provider= new HtmlFormatProvider();
            byte[] result = provider.Serialize(data);
            UnicodeStringSerializer serializer = new UnicodeStringSerializer();
            var text = serializer.Deserialize(result);
            Assert.AreEqual(text, TextHtmlData);
        }

        private const String skypeQuote =
            "<quote author=\"pater_patriae1\" " +
            "authorname=\"Marcus Tullius Cicero\" " +
            "conversation=\"#Cicero/$f65b360f0397c5fab\" " +
            "guid=\"xbd317d212319792a09a7b384726393691325aa0005a15acfb6bc58a0c29b8c31\" " +
            "timestamp=\"1035923063776\">" +
                "<legacyquote>[1/10/32 1:10:23 AM] de Finibus Bonorum et Malorum: </legacyquote>" +
                "Delorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
                "<legacyquote>&lt;&lt;&lt; </legacyquote>" +
            "</quote>";

        [TestMethod]
        public void ShoudlSerializeSampleSkypeQuoteData()
        {

            SkypeQuote data = new SkypeQuote
            {
                Author = "pater_patriae1",
                AuthorName = "Marcus Tullius Cicero",
                Conversation = "#Cicero/$f65b360f0397c5fab",
                Guid = "xbd317d212319792a09a7b384726393691325aa0005a15acfb6bc58a0c29b8c31",
                Timestamp = 1035923063776
            };
            data.AddQuoteText("[1/10/32 1:10:23 AM] de Finibus Bonorum et Malorum: ", quote: true);
            data.AddQuoteText("Delorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
            data.AddQuoteText("<<< ", quote: true);

            SkypeFormatProvider provider= new SkypeFormatProvider();
            byte[] result = provider.Serialize(data);
            UnicodeStringSerializer serializer = new UnicodeStringSerializer();
            var text = serializer.Deserialize(result);
            Assert.AreEqual(skypeQuote, text);
        }
        [TestMethod]
        public void ShoudlDeserializeSampleSkypeQuoteData()
        {
            UnicodeStringSerializer serializer = new UnicodeStringSerializer();
            var bytes = serializer.Serialize(skypeQuote);

            SkypeFormatProvider provider = new SkypeFormatProvider();
            var result = provider.Deserialize(bytes);

            Assert.AreEqual(result.Author , "pater_patriae1");
            Assert.AreEqual(result.AuthorName , "Marcus Tullius Cicero");
            Assert.AreEqual(result.Conversation , "#Cicero/$f65b360f0397c5fab");
            Assert.AreEqual(result.Guid , "xbd317d212319792a09a7b384726393691325aa0005a15acfb6bc58a0c29b8c31");
            Assert.AreEqual(result.Timestamp , 1035923063776);
            var quoteText1 = result.LegacyQuote[0];
            var quoteText2 = result.LegacyQuote[1];
            var quoteText3 = result.LegacyQuote[2];

            Assert.AreEqual(quoteText1.Text, "[1/10/32 1:10:23 AM] de Finibus Bonorum et Malorum: ");
            Assert.AreEqual(quoteText1.Quote, true);

            Assert.IsTrue(quoteText2.Text.StartsWith("Delorem ipsum dolor sit amet, consectetur adipisicing elit"));
            Assert.AreEqual(quoteText2.Quote, false);

            Assert.AreEqual(quoteText3.Text, "<<< ");
            Assert.AreEqual(quoteText3.Quote, true);

        }

        [TestMethod]
        public void WatcherTest()
        {
            ClipbordWatcher watcher = new ClipbordWatcher();
            watcher.Start();


            Thread tr = new Thread(() =>
            {
                Thread.Sleep(100);
                Clpbrd.SetText(testString, TextDataFormat.UnicodeText);
            });
            tr.SetApartmentState(ApartmentState.STA);
            tr.Start();


            foreach (var a in  watcher.WaitClipboardData())
            {
                
            }
           


        }

    }
    
}
