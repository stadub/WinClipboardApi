using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.TypeMapping;
using Utils.TypeMapping.MappingInfo;

namespace Utils.Test
{
    [TestClass]
    public class TypeMappeRegistryrTest
    {
        #region TestClasses
        class ClassW2Properties
        {
             public int Prop { get; set; }
             public int Prop2 { get; set; }
        }
        class ClassW4Properties
        {
            public int Prop { get; set; }
            public int Prop2 { get; set; }
            public int Prop3 { get; set; }
            public ITestClass Prop4 { get; set; }
        }

        class ClassW4PropertiesDescendant1 : ClassW4Properties
        {
            
        }

        class ClassW4PropertiesDescendant2 : ClassW4Properties
        {

        }
        class ClassW4PropertiesDescendant3 : ClassW4Properties
        {

        }

        class ClassWithSourceCtor
        {
            public ClassWithSourceCtor(ClassW2Properties source)
            {
                Source = source;
            }
            public ClassW2Properties Source { get; set; }
        }

      
#endregion
        [TestMethod]
        public void ShouldMapIdenticallyNamedTypeProperties()
        {
            var registry = new TypeMapperRegistry();
            var source = new ClassW2Properties {Prop = 1, Prop2 = 2};

            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();
            registry.Register<ClassW2Properties, ClassW4Properties>(mapper);

            var dest = registry.Resolve<ClassW4Properties>(source);

            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 1);
            Assert.IsTrue(dest.Prop2 == 2);
        }

        [TestMethod]
        public void ShouldMapInjectedProperties()
        {
            var registry = new TypeMapperRegistry();
            var source = new ClassW2Properties { Prop = 1, Prop2 = 2 };

            var propertiesMapper=registry.Register<ClassW2Properties, ClassW4Properties>();
            propertiesMapper.InjectPropertyValue(properties => properties.Prop3,3);

            ClassW4Properties dest = registry.Resolve(source, typeof(ClassW4Properties)) as ClassW4Properties;

            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 1);
            Assert.IsTrue(dest.Prop3 == 3);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotResolvedException))]
        public void ShouldNotResolveNotRegisteredType()
        {
            var registry = new TypeMapperRegistry();
            var source = new ClassW2Properties { Prop = 1, Prop2 = 2 };

            var propertiesMapper = registry.Register<ClassW2Properties, ClassW4Properties>();
            propertiesMapper.InjectPropertyValue(properties => properties.Prop3, 3);

            registry.Resolve(source, typeof(ClassWithSourceCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TypeAllreadyRegisteredException))]
        public void ShouldThorErrorOnRegisteringSameType()
        {
            var registry = new TypeMapperRegistry();

            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();
            registry.Register<ClassW2Properties, ClassW4Properties>(mapper);

            registry.Register<ClassW2Properties, ClassW4Properties>();
        }

        [TestMethod]
        public void ShouldResolveDestTypeDescendants()
        {
            var registry = new TypeMapperRegistry();
            var source = new ClassW2Properties { Prop = 1, Prop2 = 2 };

            var mapper = new TypeMapper<ClassW2Properties, ClassW4PropertiesDescendant1>();
            registry.Register<ClassW2Properties, ClassW4PropertiesDescendant1>(mapper);
            registry.Register<ClassW2Properties, ClassW4PropertiesDescendant2>();
            registry.Register<ClassW2Properties, ClassW4Properties>();

            var dest = registry.ResolveDescendants<ClassW4Properties>(source);

            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Count()==3);
            Assert.IsTrue(dest.All(properties => properties.Prop2 == 2));
            Assert.IsTrue(dest.All(properties => properties.Prop == 1));

        }
    }
}
