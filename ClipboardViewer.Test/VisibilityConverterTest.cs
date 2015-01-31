using System;
using System.Windows;
using System.Windows.Controls;
using ClipboardViewer.MvvmBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardViewer.Test
{
    [TestClass]
    public class VisibilityConverterTest
    {
        [TestMethod]
        public void ShouldBeVisibleOnTrueValue()
        {
            var converter = new VisibilityConverter();
            Visibility result = (Visibility) converter.Convert(true, typeof(Visibility), false, null);
            Assert.IsTrue(result == Visibility.Visible);
        }

        [TestMethod]
        public void ShouldNotBeVisibleOnFalseValue()
        {
            var converter = new VisibilityConverter();
            Visibility result = (Visibility)converter.Convert(false, typeof(Visibility), false, null);
            Assert.IsTrue(result == Visibility.Collapsed);
        }

        [TestMethod]
        public void ShouldBeVisibleOnFalseReverseValue()
        {
            var converter = new VisibilityConverter();
            Visibility result = (Visibility)converter.Convert(false, typeof(Visibility), "true", null);
            Assert.IsTrue(result == Visibility.Visible);
        }

        [TestMethod]
        public void ShouldNotBeVisibleOnTrueReverseValue()
        {
            var converter = new VisibilityConverter();
            Visibility result = (Visibility)converter.Convert(true, typeof(Visibility), "true", null);
            Assert.IsTrue(result == Visibility.Collapsed);
        }
    }
}
