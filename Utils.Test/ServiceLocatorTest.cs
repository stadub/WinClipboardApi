using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Utils.Test
{
    interface ITestClass { }

    class TestClass : ITestClass { }
    
    class TestClassWDefaultCtor : ITestClass
    {
        public TestClassWDefaultCtor() { }
    }

    class TestClassWUseCtorAttribute : ITestClass
    {
        public object b;
        [UseConstructor()]
        public TestClassWUseCtorAttribute(int a=2) { b = a; }

        public TestClassWUseCtorAttribute(object a) { }
    }

    class TestClassWCtor : ITestClass
    {
        public TestClassWCtor(int a=2) { }
    }

    class TestClassWSimpleInjectionCtor : ITestClass
    {
        public object b;
        public TestClassWCtor([Inject("2")]int a){ b = a; }
    }

    class TestClassWInjectionCtor : ITestClass
    {
        public TestClassWCtor([Inject()]IList<int> a) { }
    }


    class TestClassWPropertyInjection : ITestClass
    {
        [Inject()]
        public IList<int> Prop{get;set;}
    }


    [TestClass]
    public class ServiceLocatorTest
    {
        [TestMethod]
        public void ShouldResolveSingleton()
        {
            var locator = new ServiceLocator();
            var list = new List<int>();
            locator.Register(list);

            var reslut = locator.Resolve<List<int>>();

            Assert.ReferenceEquals(reslut, list);
        }

        [TestMethod]
        public void ShouldResolveGenericClass()
        {
            var locator = new ServiceLocator();
            locator.Register<IList<int>, List<int>>();

            var reslut = locator.Resolve<IList<int>>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotResolvedException))]
        public void ShouldThrowNotresolvedException_WhenClassNotRegistered()
        {
            var locator = new ServiceLocator();
            locator.Register<IList<int>, List<int>>();

            var reslut = locator.ResolveInstance<IList<int>>();

            Assert.IsNotNull(reslut);
        }
           
        [TestMethod]
        [ExpectedException(typeof(TypeAllreadyRegisteredException))]
        public void ShouldThrowTypeAllreadyRegisteredException_WhenInterfaceInstanceAllreadyRegistered()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClass>();
            locator.Register<ITestClass, TestClassWDefaultCtor>();
        }

        [TestMethod]
        public void ShouldResolveClass()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClass>();

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWDefaultCtor()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWDefaultCtor>();

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWDefaultCtor()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWDefaultCtor>();
            locator.Register<ITestClass, TestClassWDefaultCtor>();

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }


        [TestMethod]
        public void ShouldResolveTestClassWUseCtorAttribute()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWUseCtorAttribute>();
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance=reslut as TestClassWUseCtorAttribute;
            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.b==2);
        }

        [TestMethod]
        public void ShouldResolveTestClassWCtor()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWCtor>();
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }

        [TestMethod]
        public void ShouldResolveTestClassWUseCtorAttribute()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWSimpleInjectionCtor>();
            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance=reslut as TestClassWSimpleInjectionCtor;
            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.b==2);
        }


        [TestMethod]
        public void ShouldResolveTestClassWInjectionCtor()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWUseCtorAttribute>();
            var list = new List<int>();
            locator.Register(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
        }


        [TestMethod]
        public void ShouldResolveTestClassWInjectionCtor()
        {
            var locator = new ServiceLocator();
            locator.Register<ITestClass, TestClassWPropertyInjection>();
            var list = new List<int>();
            locator.Register(list);

            var reslut = locator.Resolve<ITestClass>();

            Assert.IsNotNull(reslut);
            var instance=reslut as TestClassWPropertyInjection;

            Assert.ReferenceEquals(instance.Prop, list);
        }
    }
}
