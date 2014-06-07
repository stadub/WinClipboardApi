using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
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
        public void ShoudlDeserializeSampleString()
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
        public void ShoudlSerializeSampleString()
        {
            HtmlClipboardFormatData data = new HtmlClipboardFormatData();
            data.Version= "1.0";
            data.StartHTML= 000000194;
            data.EndHTML=000001170;
            data.StartFragment=000000493;
            data.EndFragment=000001112;
            data.StartSelection=000000507;
            data.EndSelection=000001108;
            data.SourceURL= new Uri("res://iesetup.dll/HardAdmin.htm");
            string[] results = htmlDoc.Split(TextHtmlData);
            data.Html = results[1];

            HtmlFormatProvider provider= new HtmlFormatProvider();
            byte[] result = provider.Serialize(data);
            UnicodeStringSerializer serializer = new UnicodeStringSerializer();
            var text = serializer.Deserialize(result);
            Assert.AreEqual(text, TextHtmlData);
        }

    }
}
