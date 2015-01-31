using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utils.Test
{
    [TestClass]
    public class TypeMapperTest
    {
        class A
        {
             public bool a { get; set; }
        }
        class B
        {
            public bool a { get; set; }
        }
        [TestMethod]
        public void ShouldMapType()
        {
            var mapper = new TypeMapper();
            var b=mapper.MapTo<B>(new A {a = true});
            Assert.IsNotNull(b);
            Assert.IsTrue(b.a==true);
        }
    }
}
