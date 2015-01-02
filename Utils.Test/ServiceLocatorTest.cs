using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utils.Test
{
    [TestClass]
    public class ServiceLocatorTest
    {
        [TestMethod]
        public void ShouldResolveSingleton()
        {
            var locator = new ServiceLocator();
        }
    }
}
