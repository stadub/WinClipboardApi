using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace Utils.Test
{
    [TestClass]
    public class EnumerableTest
    {
        [TestMethod]
        public void ForEachShouldEnumerateSameSequenceAsSentToInput()
        {
            var resultList= new List<int>();
            var items=Enumerable.Range(0, 10).ToList();
            items.ForEach(x=>resultList.Add(x));

            Assert.IsTrue(resultList.Count == items.Count);
            for (int i = 0; i < resultList.Count; i++)
            {
                Assert.AreEqual(resultList[i],items[i]);
            }
        }
    }
}
