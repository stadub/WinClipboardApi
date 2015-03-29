using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utils.Test
{
    [TestClass]
    public class TypeMapperTest
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

        class ClassWithSourceCtor
        {
            public ClassWithSourceCtor(ClassW2Properties source)
            {
                this.Source = source;
            }
            public ClassW2Properties Source { get; set; }
        }


        class ClassWithSourceProp
        {
            public ClassWithSourceProp(int prop2)
            {
                this.Prop2 = prop2;
            }

            public int Prop2 { get; private set; }
        }


        interface ITestClass { }

        class TestClassWProperty : ITestClass
        {
            public IList<int> Prop { get; set; }
        }

        class ClassWSourcePropertyMapAttribute
        {
            [SourceProperty(Name="Prop2")]
            public int Prop { get; set; }
            public int Prop2 { get; set; }
        } 
        
        class ClassWSourcePropertyPathAttribute
        {
            [SourceProperty(Path = "Source.Prop2")]
            public int Prop { get; set; }
            public int Prop2 { get; set; }
        }

        class TestTypeBuilder : TypeBuilderStub
        {
            public PropertyInfo PropertyToResolve { get; set; }
            public object ProertyValue { get; set; }
            public TestTypeBuilder(Type destType)
                : base(destType)
            {
            }

            protected override bool ResolvePublicNotIndexedProperty(PropertyInfo property, out object value)
            {
                if (property == PropertyToResolve)
                {
                    value = ProertyValue;
                    return true;
                }
                return base.ResolvePublicNotIndexedProperty(property, out value);
            }
        }

        #endregion TestClasses

        [TestMethod]
        public void ShouldMapIdenticallyNamedTypeProperties()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();
            var dest = mapper.Map(new ClassW2Properties { Prop = 1,Prop2 = 2});
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 1);
            Assert.IsTrue(dest.Prop2 == 2);
        }

        
        [TestMethod]
        public void ShouldMapCtorProperty()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassWithSourceProp>();
            var dest = mapper.Map(new ClassW2Properties { Prop = 1, Prop2 = 2 });
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop2 == 2);
        }

        [TestMethod]
        public void ShouldMapSourceTypeToDestConstructor()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassWithSourceCtor>();
            var source = new ClassW2Properties();
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Source == source);
        }


        [TestMethod]
        public void ShouldInjectPropertyValue()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();
            mapper.MappingInfo.InjectPropertyValue(cl => cl.Prop3,3);
            var source = new ClassW2Properties();
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop3 == 3);
        }

        [TestMethod]
        public void ShouldIgnoreProperty()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();

            mapper.MappingInfo.IgnoreProperty(cl => cl.Prop);
            var source = new ClassW2Properties{ Prop = 1,Prop2 = 2};
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 0);
            Assert.IsTrue(dest.Prop2 == 2);
        }

        [TestMethod]
        public void ShouldResolvePropertySetByBaseResolver()
        {
            var destType = typeof (ClassW4Properties);
            var testBuilder = new TestTypeBuilder(destType);
            testBuilder.PropertyToResolve = destType.GetProperty("Prop3");
            testBuilder.ProertyValue = 3;
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>(testBuilder);

            mapper.MappingInfo.IgnoreProperty(cl => cl.Prop);
            var source = new ClassW2Properties();
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop3 == 3);
        }

        [TestMethod]
        public void ShouldUserTopLevelBuilder()
        {
            var destType = typeof(ClassW4Properties);
            var testBuilder = new TestTypeBuilder(destType);
            testBuilder.PropertyToResolve = destType.GetProperty("Prop2");
            testBuilder.ProertyValue = 3;
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>(testBuilder);
            var source = new ClassW2Properties { Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop2 == 2);
        }

        [TestMethod]
        public void ShouldResolvePropertiesByServiceLocator()
        {
            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWProperty>()
                .InjectProperty(classWProp => classWProp.Prop);

            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>(locator);

            mapper.MappingInfo.IgnoreProperty(cl => cl.Prop);
            mapper.LocatorMappingInfo.InjectProperty(properties => properties.Prop4);

            var source = new ClassW2Properties { Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop2 == 2);


            Assert.IsNotNull(dest.Prop4);
            var instance = dest.Prop4 as TestClassWProperty;

            Assert.IsTrue(ReferenceEquals(instance.Prop, list));
        }

        [TestMethod]
        public void ShouldResolveCorrectPropertyWithSourcePropertyAttribute()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassWSourcePropertyMapAttribute>();
            var source = new ClassW2Properties {Prop = 1, Prop2 = 2};
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 2);
            Assert.IsTrue(dest.Prop2 == 2);
        }

        [TestMethod]
        public void ShouldResolvePropertyPathForPropertyWAttribute()
        {
            var mapper = new TypeMapper<ClassWithSourceCtor, ClassWSourcePropertyPathAttribute>();
            var source = new ClassWithSourceCtor(new ClassW2Properties {Prop = 1, Prop2 = 2});
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 2);
        }
    }
}
