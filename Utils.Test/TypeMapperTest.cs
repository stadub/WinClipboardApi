using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.TypeMapping;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.TypeBuilders;
using Utils.TypeMapping.TypeMappers;
using Utils.TypeMapping.ValueResolvers;

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
                Source = source;
            }
            public ClassW2Properties Source { get; private set; }
        }


        class ClassWithSourceProp
        {
            public ClassWithSourceProp(int prop2)
            {
                Prop2 = prop2;
            }

            public int Prop2 { get; private set; }
        }


        interface ITestClass { }

        class TestClassWIntListProperty : ITestClass
        {
            public IList<int> Prop { get; set; }
        } 
        
        class TestClassWStringListProperty : ITestClass
        {
            public IList<string> Prop { get; set; }
        }

        class ClassWSourcePropertyMapAttribute
        {
            [MapSourceProperty(Name="Prop2")]
            public int Prop { get; set; }
            public int Prop2 { get; set; }
        } 
        
        class ClassWSourcePropertyPathAttribute
        {
            [MapSourceProperty(Path = "Source.Prop2")]
            public int Prop { get; set; }
        }


        class ClassWSourcePropertyInitalizer
        {
            [UseInitalizer(Name = "InitProp")]
            public int Prop { get; private set; }

            public void InitProp(int value)
            {
                Prop = value;
            }
        }

        class ClassWSourcePropertyInitalizerAndPropertyPath
        {
            [MapSourceProperty(Path = "Source.Prop2")] [UseInitalizer(Name = "InitProp")]
            public int Prop { get; private set; }

            public void InitProp(int value)
            {
                Prop = value;
            }
        }

        class TestTypeBuilder<TSource, TDest> : MappingTypeBuilder<TSource, TDest>
        {
            public PropertyInfo PropertyToResolve
            {
                get { return injectResolver.PropertyToResolve; }
                set { injectResolver.PropertyToResolve = value; }
            }

            public object ProertyValue
            {
                get { return injectResolver.ProertyValue; }
                set { injectResolver.ProertyValue=value; }
            }

            private TestInjectValueResolver injectResolver;

            public TestTypeBuilder(bool priorResolver=false): base()
            {
                injectResolver = new TestInjectValueResolver();
                if (priorResolver)
                    RegisterPriorSourceResolver(injectResolver);
                else
                    RegisterSourceResolver(injectResolver);
            }

            public override void CreateBuildingContext()
            {
                base.Context= new TestTypeBuilerContext<TSource, TDest>();
            }

            public override void InitBuildingContext()
            {
                
            }
        }

        class TestTypeBuilerContext<TSource, TDest> : TypeMapperContext<TSource, TDest> 
        {
            public TestTypeBuilerContext(): base(new Dictionary<PropertyInfo, ITypeMapper>()){}
        }

        class TestInjectValueResolver : SourceMappingResolverBase
        {
            public PropertyInfo PropertyToResolve { get; set; }
            public object ProertyValue { get; set; }

            protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
            {
                return PropertyToResolve.Name == memberInfo.Name;
            }

            protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
            {
                if (PropertyToResolve.Name == memberInfo.Name)
                {
                    return OperationResult.Successful(ProertyValue);
                }
                return OperationResult.Failed();
            }

        }

        class TestTypeMapper<TSource, TDest> : TypeMapper<TSource, TDest>
        {
            private readonly Func<TestTypeBuilder<TSource, TDest>> createBuilderFunc;

            public TestTypeMapper(Func<TestTypeBuilder<TSource, TDest>> createBuilderFunc)
            {
                this.createBuilderFunc = createBuilderFunc;
            }

            protected override MappingTypeBuilder<TSource, TDest> CreateTypeBuilder()
            {
                return createBuilderFunc();
            }


        }

        public class ClassWStringProperty
        {
            public string SourceURL { get; set; }
        }

        public class ClassWProperty
        {
            public Uri SourceURL { get; set; }
        }

        #endregion TestClasses

        [TestMethod]
        public void ShouldMapIdenticallyNamedTypeProperties()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();
            var dest = mapper.Map(new ClassW2Properties { Prop = 1,Prop2 = 2});
            Assert.IsNotNull(dest);
            Assert.AreEqual(1, dest.Prop);
            Assert.AreEqual(2,dest.Prop2);
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
            Assert.AreEqual(dest.Source, source);
        }


        [TestMethod]
        public void ShouldInjectPropertyValue()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();
            mapper.RegistrationInfo.InjectPropertyValue(cl => cl.Prop3,3);
            var source = new ClassW2Properties();
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(3, dest.Prop3);
        }

        [TestMethod]
        public void ShouldIgnoreProperty()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>();

            mapper.RegistrationInfo.IgnoreProperty(cl => cl.Prop);
            var source = new ClassW2Properties{ Prop = 1,Prop2 = 2};
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(0, dest.Prop);
            Assert.AreEqual(2, dest.Prop2);
        }

        [TestMethod]
        public void ShouldResolvePropertySetByBaseResolver()
        {
            var destType = typeof (ClassW4Properties);
            var testBuilder = new TestTypeBuilder<ClassW2Properties,ClassW4Properties>();
            testBuilder.PropertyToResolve = destType.GetProperty("Prop3");
            testBuilder.ProertyValue = 3;

            var mapper = new TestTypeMapper<ClassW2Properties, ClassW4Properties>(() => testBuilder);
            var source = new ClassW2Properties();
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(3, dest.Prop3);
        }

        [TestMethod]
        public void ShouldUserTopLevelBuilder()
        {
            var destType = typeof(ClassW4Properties);
            var testBuilder = new TestTypeBuilder<ClassW2Properties, ClassW4Properties>();
            testBuilder.PropertyToResolve = destType.GetProperty("Prop2");
            testBuilder.ProertyValue = 3;

            var mapper = new TestTypeMapper<ClassW2Properties, ClassW4Properties>(() => testBuilder);
            var source = new ClassW2Properties { Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(2, dest.Prop2);
        }
        
        [TestMethod]
        public void ShouldUserPriorBuilder()
        {
            var destType = typeof(ClassW4Properties);
            var testBuilder = new TestTypeBuilder<ClassW2Properties, ClassW4Properties>();
            testBuilder.PropertyToResolve = destType.GetProperty("Prop2");
            testBuilder.ProertyValue = 3;

            var mapper = new TestTypeMapper<ClassW2Properties, ClassW4Properties>(() => testBuilder);
            var source = new ClassW2Properties { Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(2, dest.Prop2);
        }

        [TestMethod]
        public void ShouldResolvePropertiesByServiceLocator()
        {
            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWIntListProperty>()
                .InjectProperty(classWProp => classWProp.Prop);

            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var mapper = new TypeMapper<ClassW2Properties, ClassW4Properties>(locator);

            mapper.RegistrationInfo.IgnoreProperty(cl => cl.Prop);
            mapper.LocatorMappingInfo.InjectProperty(properties => properties.Prop4);

            var source = new ClassW2Properties { Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(2, dest.Prop2);


            Assert.IsNotNull(dest.Prop4);
            var instance = dest.Prop4 as TestClassWIntListProperty;

            Assert.IsTrue(ReferenceEquals(instance.Prop, list));
        }

        [TestMethod]
        public void ShouldResolveCorrectPropertyWithSourcePropertyAttribute()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassWSourcePropertyMapAttribute>();
            var source = new ClassW2Properties {Prop = 1, Prop2 = 2};
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(2, dest.Prop);
            Assert.AreEqual(2, dest.Prop2);
        }

        [TestMethod]
        public void ShouldResolvePropertyPathForPropertyWAttribute()
        {
            var mapper = new TypeMapper<ClassWithSourceCtor, ClassWSourcePropertyPathAttribute>();
            var source = new ClassWithSourceCtor(new ClassW2Properties {Prop = 1, Prop2 = 2});
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(2, dest.Prop);
        }

        [TestMethod]
        public void ShouldResolvePropertyByInitalizer()
        {
            var mapper = new TypeMapper<ClassW2Properties, ClassWSourcePropertyInitalizer>();
            var source = new ClassW2Properties { Prop = 1, Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(1, dest.Prop);
        }
        
        [TestMethod]
        public void ShouldResolvePropertyByInitalizerWithPathSet()
        {
            var mapper = new TypeMapper<ClassWithSourceCtor, ClassWSourcePropertyInitalizerAndPropertyPath>();
            var source = new ClassWithSourceCtor(new ClassW2Properties {Prop = 1, Prop2 = 2});
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsTrue(dest.Prop == 2);
        }
        
        [TestMethod]
        public void ShouldNotMapNullValues()
        {
            var mapper = new TypeMapper<ClassW4Properties, ClassW4Properties>();
            var source = new ClassW4Properties { Prop = 1, Prop2 = 2 };
            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(1, dest.Prop);
            Assert.AreEqual(2, dest.Prop2);
            Assert.AreEqual(0, dest.Prop3);
            Assert.AreEqual(null, dest.Prop4);
        }
        
        
        [TestMethod]
        public void ShouldMapProperyWArray()
        {
            var mapper = new TypeMapper<TestClassWIntListProperty, TestClassWStringListProperty>();


            var arrMapper = new ArrayTypeMapper<int, string>(new MappingFunc<int, string>(i => i.ToString()));

            mapper.PropertyMappingInfo.MapProperty((property, ints) => property.Prop,arrMapper);


            var arr = new[] {1, 2, 3, 4, 5, 6, 7, 8};

            var source = new TestClassWIntListProperty { Prop = new List<int>(arr) };

            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);


            for (int i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(arr[i].ToString(), dest.Prop[i]);
            }
           
        }
        
        
        [TestMethod]
        public void ShouldMapPropertyByConstructing()
        {
            var source = new ClassWStringProperty();
            source.SourceURL = "http://test.link/";

            var mapper = new TypeMapper<ClassWStringProperty, ClassWProperty>();

            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.IsNotNull(dest.SourceURL);
            Assert.AreEqual(source.SourceURL,dest.SourceURL.AbsoluteUri);
           
        } 
        
        [TestMethod]
        public void ShouldMapTypeByDictionary()
        {
            var source = new Dictionary<string, int>();

            source.Add("Prop",1);
            source.Add("Prop2", 1);
            source.Add("Prop3", 1);
            var mapper = new DictionaryMapper<int, ClassW4Properties>();

            var dest = mapper.Map(source);
            Assert.IsNotNull(dest);
            Assert.AreEqual(source["Prop"], dest.Prop);
            Assert.AreEqual(source["Prop2"], dest.Prop2);
            Assert.AreEqual(source["Prop3"], dest.Prop3);
           
        }
    }
}
