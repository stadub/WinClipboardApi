

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.TypeMapping.TypeMappers;

namespace Utils.Test
{
    [TestClass]
    public class ArrayTypeMapperTest
    {
        [TestMethod]
        public void ShouldMapArray()
        {
            var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            var arrMapper = new ArrayTypeMapper<int, string>(new MappingFunc<int, string>(i => i.ToString()));

            var dest=arrMapper.Map(arr);

            Assert.IsNotNull(dest);


            for (int i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(arr[i].ToString(), dest[i]);
            }

        }
    }
}
