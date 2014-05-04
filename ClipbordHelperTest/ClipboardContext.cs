using System;
using System.Windows.Forms;
using ClipbordHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipbordHelperTest
{
    public class ClipbordContext : SpecificationContext
    {
        private ClipboardWinApi clipboardApi;
        public override void Given()
        {
            base.Given();
            clipboardApi = new ClipboardWinApi();
        }

        public override void When()
        {
            base.When();
            Clipboard.Clear();
            Clipboard.SetText("12345");
        }


        [TestMethod]
        public void ShouldGetClipbordText()
        {
            clipboardApi.Open();
            clipboardApi.
            clipboardApi.Close();
        }
    }

}
