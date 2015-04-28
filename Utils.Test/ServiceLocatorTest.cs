using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.TypeMapping;

namespace Utils.Test
{
    #region TestClasses
    interface ITestClass { }

    class TestClass : ITestClass { }
    
    class TestClassWDefaultCtor : ITestClass
    {
        public TestClassWDefaultCtor() { }
    }

    class TestClassWUseCtorAttribute : ITestClass
    {
        public TestClassWUseCtorAttribute(object a) { }

        public object b;
        [UseConstructor()]
        public TestClassWUseCtorAttribute(int a=2) { b = a; }

        
    }
    
    class TestClassWGenericInjectionCtor : ITestClass
    {
        public object b;
        public TestClassWGenericInjectionCtor(IList<int> a ) { b = a; }
    }

    class TestClassWCtor : ITestClass
    {
        public TestClassWCtor(int a=2) { }
    }

    class TestClassWSimpleInjectionCtor : ITestClass
    {
        public object b;
        public TestClassWSimpleInjectionCtor([InjectValue("2")]int a){ b = a; }
    }

    class TestClassWInjectionCtor : ITestClass
    {
        public TestClassWInjectionCtor([ShoudlInject()]IList<int> a) { }
    }
    
    class TestClassWRetvalCtor : ITestClass
    {
        public TestClassWRetvalCtor([InjectValue("2")]ref int a) { }
    }


    class TestClassWProperty : ITestClass
    {
        public IList<int> Prop { get; set; }
    }

    class TestClassWTestClassProperty : ITestClass
    {
        public ITestClass Prop { get; set; }
    }

    class TestClassWPropertyValueInjection : ITestClass
    {
        [InjectValue("2")]
        public int Prop { get; set; }
    }

    class TestClassWPropertyInjection : ITestClass
    {
        [InjectInstance()]
        public IList<int> Prop{get;set;}
    }

    class TestClassWNamedPropertyInjection : ITestClass
    {
        [InjectInstance("test")]
        public ITestClass Prop { get; set; }
    }

    class TestClassWDispose : ITestClass,IDisposable
    {
        public bool Disposed{get;set;}
        
        public void Dispose()
        {
 	        Disposed=true;
        }
    }
    #endregion TestClasses

    [TestClass]
    public class ServiceLocatorTest
    {
        [TestMethod]
        public void ShouldResolveSingleton()
        {
            var locator = new ServiceLocator();
            var list = new List<int>();
            locator.RegisterInstance<IList<int>,List<int>>(list);

            var reslut = locator.Resolve<IList<int>>();

            ReferenceEquals(reslut, list);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotSupportedException))]
        public void ShouldThrowExceptionOnGenericClassRegistration()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<IList<int>, List<int>>();

            var reslut = locator.Resolve<IList<int>>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotResolvedException))]
        public void ShouldThrowNotresolvedException_WhenClassNotRegistered()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClass>();

            var reslut = locator.ResolveInstance<TestClassWDefaultCtor>();

            Assert.IsNotNull(reslut);
        }
           
        [TestMethod]
        [ExpectedException(typeof(TypeAllreadyRegisteredException))]
        public void ShouldThrowTypeAllreadyRegisteredException_WhenInterfaceInstanceAllreadyRegistered()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClass>();
            locator.RegisterType<ITestClass, TestClassWDefaultCtor>();
        }

        [TestMethod]
        public void ShouldResolveClass()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClass>();

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWDefaultCtor()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWDefaultCtor>();

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWUseCtorAttribute()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWUseCtorAttribute>();
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance=reslut as TestClassWUseCtorAttribute;
            Assert.IsNotNull(instance);
            Assert.IsTrue((int)instance.b==2);
        }

        [TestMethod]
        public void ShouldResolveTestClassWCtor()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWCtor>();
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWSimpleInjectionCtor()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWSimpleInjectionCtor>();
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance=reslut as TestClassWSimpleInjectionCtor;
            Assert.IsNotNull(instance);
            Assert.IsTrue((int)instance.b == 2);
        }


        [TestMethod]
        public void ShouldResolveTestClassWGenericInjectionCtor()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWGenericInjectionCtor>();
            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }
        [TestMethod]
        public void ShouldResolveTestClassWInjectionCtor()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWInjectionCtor>();
            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotSupportedException))]
        public void ShouldThrowExceptionOnRetvalCtorValue()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWRetvalCtor>();
            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWPropertyInjection()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWPropertyInjection>();
            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance=reslut as TestClassWPropertyInjection;

            ReferenceEquals(instance.Prop, list);
        }

        [TestMethod]
        public void ShouldResolveTestClassWNamedPropertyInjection()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWNamedPropertyInjection>();
            locator.RegisterType<ITestClass, TestClassWDispose>("test");

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance = reslut as TestClassWNamedPropertyInjection;

            Assert.IsTrue(instance.Prop is TestClassWDispose);
        }

        [TestMethod]
        public void ShouldResolveSingetonForTestClassWNamedPropertyInjection()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWNamedPropertyInjection>();
            locator.RegisterInstance<ITestClass, TestClassWDispose>(new TestClassWDispose(),"test");

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance = reslut as TestClassWNamedPropertyInjection;

            Assert.IsTrue(instance.Prop is TestClassWDispose);
        }

        [TestMethod]
        public void ShouldResolveTestClassWPropertyValueInjection()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWPropertyValueInjection>();

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance = reslut as TestClassWPropertyValueInjection;

            Assert.IsTrue(instance.Prop== 2);
        }

        [TestMethod]
        public void ShouldDistroyDisposableTypes()
        {
            var locator = new ServiceLocator();
            var disbposble=new TestClassWDispose();
            locator.RegisterInstance<ITestClass,TestClassWDispose>(disbposble);
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            locator.Dispose();

            Assert.IsTrue(disbposble.Disposed);
        }

        [TestMethod]
        public void ShouldResolveInitializer()
        {
            var locator = new ServiceLocator();
            locator.RegisterInitializer<ITestClass>(() => new TestClassWDispose());
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionOnTryToInjectMethodInsteadProperty()
        {
            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWProperty>()
                .InjectProperty(classWProp => classWProp.GetType());
        }

        [TestMethod]
        public void ShouldResolveTestClassWPropertyInjectionByExtension()
        {

            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWProperty>()
                .InjectProperty(classWProp => classWProp.Prop);
            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance = reslut as TestClassWProperty;

            Assert.IsTrue(ReferenceEquals(instance.Prop, list));
        }

        [TestMethod]
        public void ShouldResolveTestClassWNamedPropertyInjectionByExtension()
        {

            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWTestClassProperty>()
                .InjectNamedProperty(classWProp => classWProp.Prop,"TestProp");
            locator.RegisterType<ITestClass, TestClassWDispose>("TestProp");

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance = reslut as TestClassWTestClassProperty;

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.Prop is TestClassWDispose);
        }

        [TestMethod]
        public void ShouldResolveTestClassWPropertyValueInjectionByExtension()
        {
            var list = new List<int>();
            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWProperty>()
                .InjectPropertyValue(classWProp => classWProp.Prop, list);
            
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance = reslut as TestClassWProperty;

            ReferenceEquals(instance.Prop, list);
        }

        [TestMethod]
        public void ShouldResolveTestClassByNamedResolver()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWProperty>("class1");
            var reslut = locator.ResolveType<ITestClass>("class1");
            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotResolvedException))]
        public void ShouldThrowExceptionOnTryResolveDefaultTypeWhenRegisteredNamed()
        {

            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWProperty>("class1");

            locator.ResolveType<ITestClass>();
        }


        [TestMethod]
        public void ShouldResolveCorrectTestClassWhenNamedAndDefaultRegistered()
        {

            var locator = new ServiceLocator();
            locator
                .RegisterType<ITestClass, TestClassWProperty>()
                .InjectProperty(classWProp => classWProp.Prop);
            var list = new List<int>();
            locator.RegisterInstance<IList<int>, List<int>>(list);
            locator.RegisterType<ITestClass, TestClassWProperty>("class1");

            var reslut = locator.ResolveType<ITestClass>();
            var reslut2 = locator.ResolveType<ITestClass>("class1");

            Assert.IsNotNull(reslut);
            Assert.IsNotNull(reslut2);
            var instance = reslut as TestClassWProperty;
            var instance2 = reslut2 as TestClassWProperty;

            ReferenceEquals(instance.Prop, list);

            Assert.IsNull(instance2.Prop);
        }


        [TestMethod]
        public void ShouldResolveInteger()
        {

            var locator = new ServiceLocator();
            locator.RegisterType<int, int>();

            var defaultValue=locator.ResolveType<int>();

            Assert.IsTrue((int)defaultValue == 0);
        }
    }
}
