using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                this.Source = source;
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
            registry.Register(mapper);

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
            registry.Register(mapper);

            registry.Register<ClassW2Properties, ClassW4Properties>();
        }

        [TestMethod]
        public void ShouldResolveDestTypeDescendants()
        {
            var registry = new TypeMapperRegistry();
            var source = new ClassW2Properties { Prop = 1, Prop2 = 2 };

            var mapper = new TypeMapper<ClassW2Properties, ClassW4PropertiesDescendant1>();
            registry.Register(mapper);
            registry.Register<ClassW2Properties, ClassW4PropertiesDescendant2>();
            registry.Register<ClassW2Properties, ClassW4Properties>();

            var dest = registry.ResolveDestTypeDescendants<ClassW4Properties>(source);

            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Count()==3);
            Assert.IsTrue(dest.All(properties => properties.Prop2 == 2));
            Assert.IsTrue(dest.All(properties => properties.Prop == 1));

        }
    }
}
