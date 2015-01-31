using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Utils.Test
{
    [TestClass]
    public class StringsTest
    {
        [TestMethod]
        public void ShouldSplitStringToTwoIdenticalStrings()
        {
            var text = "test1|test1";
            var data = text.SplitString('|');
            Assert.AreEqual(data.Item1,data.Item2);
        }

        [TestMethod]
        public void ShouldSplitStringToNumbers()
        {
            var text = "1|2";
            var data = text.SplitString('|');
            Assert.AreEqual(int.Parse(data.Item1), 1);
            Assert.AreEqual(int.Parse(data.Item2), 2);
        }
    }
}
