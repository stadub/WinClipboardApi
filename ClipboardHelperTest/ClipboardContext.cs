using ClipboardHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardHelperTest
{
    public class ClipboardContext : SpecificationContext
    {
        private Clipboard clipboardApi;
        public override void Given()
        {
            base.Given();
            clipboardApi = Clipboard.CreateReadOnly();
        }

        public override void When()
        {
            base.When();
            System.Windows.Forms.Clipboard.Clear();
            System.Windows.Forms.Clipboard.SetText("12345");
        }


        [TestMethod]
        public void ShouldGetClipboardText()
        {
            clipboardApi.OpenReadOnly();
            clipboardApi.Close();
        }
    }

}
