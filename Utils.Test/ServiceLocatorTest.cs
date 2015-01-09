﻿using System;
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
        public TestClassWSimpleInjectionCtor([Inject("2")]int a){ b = a; }
    }

    class TestClassWInjectionCtor : ITestClass
    {
        public TestClassWInjectionCtor([Inject()]IList<int> a) { }
    }
    
    class TestClassWRetvalCtor : ITestClass
    {
        public TestClassWRetvalCtor([Inject("2")]ref int a) { }
    }

    
    class TestClassWPropertyInjection : ITestClass
    {
        [Inject()]
        public IList<int> Prop{get;set;}
    }

    class TestClassWDispose : ITestClass,IDisposable
    {
        public bool Disposed{get;set;}
        
        public void Dispose()
        {
 	        Disposed=true;
        }
    }

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

            Assert.ReferenceEquals(reslut, list);
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
        public void ShouldResolveTestClassWInjectionCtor()
        {
            var locator = new ServiceLocator();
            locator.RegisterType<ITestClass, TestClassWGenericInjectionCtor>();
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

            Assert.ReferenceEquals(instance.Prop, list);
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
    }
}