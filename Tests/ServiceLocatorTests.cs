using System;
using MikeAssets.ModularServiceLocator.Runtime;
using NUnit.Framework;
using UnityEngine.Scripting;

namespace MikeAssets.ModularServiceLocator.Tests
{
    public interface ITestService { }
    public class TestService : ITestService { }
    
    public interface ITestParamsService
    {
        ITestService Dependency { get; }
    }

    public class TestParamsService : ITestParamsService
    {
        public ITestService Dependency { get; }

        [Preserve]
        public TestParamsService(ITestService testService)
        {
            Dependency = testService;
        }
    }
    
    public interface ITestCircularService1 { }

    public class TestCircularService1 : ITestCircularService1
    {
        public TestCircularService1(ITestCircularService2 testCircularService2) { }
    }
    
    public interface ITestCircularService2 { }

    public class TestCircularService2 : ITestCircularService2
    {
        public TestCircularService2(ITestCircularService1 testCircularService1) { }
    }

    // A -> B -> C -> B: the cycle does not include the service that was requested.
    public interface ITestIndirectA { }
    public interface ITestIndirectB { }
    public interface ITestIndirectC { }
    public class TestIndirectA : ITestIndirectA { public TestIndirectA(ITestIndirectB b) { } }
    public class TestIndirectB : ITestIndirectB { public TestIndirectB(ITestIndirectC c) { } }
    public class TestIndirectC : ITestIndirectC { public TestIndirectC(ITestIndirectB b) { } }

    public interface ITestMultiCtorService
    {
        bool WasInjected { get; }
    }

    public class TestMultiCtorService : ITestMultiCtorService
    {
        public bool WasInjected { get; }

        public TestMultiCtorService() { }

        public TestMultiCtorService(ITestService testService)
        {
            WasInjected = testService != null;
        }
    }

    public interface ITestSecondContract { }
    public class TestTwoContractsService : ITestService, ITestSecondContract { }

    public class TestCountingService : ITestService
    {
        public static int Created;
        public TestCountingService() { Created++; }
    }

    public class TestModule : LocatorModule
    {
        public override void Load()
        {
            Bind<ITestService>().ToTransient<TestService>();
        }
    }
    
    public class ServiceLocatorTests
    {
        [Test]
        public void ShouldCreateSimpleTransientBinding()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToTransient<TestService>();

            var service = locator.Get<ITestService>();
            Assert.IsNotNull(service);
        }

        [Test]
        public void TransientShouldReturnNewInstanceEachTime()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToTransient<TestService>();

            Assert.AreNotSame(locator.Get<ITestService>(), locator.Get<ITestService>());
        }

        [Test]
        public void ShouldCreateTransientBindingsWithParams()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToTransient<TestService>();
            locator.Bind<ITestParamsService>().ToTransient<TestParamsService>();

            var service = locator.Get<ITestParamsService>();
            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Dependency);
        }

        [Test]
        public void ShouldCreateSingletonBinding()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToTransient<TestService>();
            locator.Bind<ITestParamsService>().ToSingleton<TestParamsService>();

            var service1 = locator.Get<ITestParamsService>();
            var service2 = locator.Get<ITestParamsService>();
            
            Assert.AreSame(service1, service2);
        }

        [Test]
        public void SingletonBindingShouldReportSingletonType()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToSingleton<TestService>();

            var binding = locator.RootBindings[0];
            Assert.AreEqual(BindingType.Singleton, binding.Configuration.BindingType);
        }

#pragma warning disable 618
        [Test]
        public void ObsoleteToSingletoneShouldStillWork()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToSingletone<TestService>();

            Assert.AreSame(locator.Get<ITestService>(), locator.Get<ITestService>());
        }
#pragma warning restore 618
        
        [Test]
        public void ShouldCreateSingletonBindingOnResolveCalled()
        {
            TestCountingService.Created = 0;
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToSingleton<TestCountingService>();

            locator.ResolveSingletons();
            Assert.AreEqual(1, TestCountingService.Created);

            var service = locator.Get<ITestService>();
            Assert.NotNull(service);
            Assert.AreEqual(1, TestCountingService.Created);
        }

        [Test]
        public void TwoContractSingletonShouldShareInstance()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService, ITestSecondContract>().ToSingleton<TestTwoContractsService>();

            var first = locator.Get<ITestService>();
            var second = locator.Get<ITestSecondContract>();
            Assert.AreSame(first, second);
        }

        [Test]
        public void ConstantShouldReturnGivenInstance()
        {
            var locator = new ServiceLocator();
            var instance = new TestService();
            locator.Bind<ITestService>().ToConstant(instance);

            Assert.AreSame(instance, locator.Get<ITestService>());
        }
        
        [Test]
        public void ShouldFailWithAnExceptionIfCircularDependencyIsPresent()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestCircularService1>().ToTransient<TestCircularService1>();
            locator.Bind<ITestCircularService2>().ToTransient<TestCircularService2>();

            Assert.Throws<CyclicDependencyException>(() => locator.Get<ITestCircularService2>());
            Assert.Throws<CyclicDependencyException>(() => locator.Get<ITestCircularService1>());
        }

        [Test]
        public void ShouldDetectIndirectCircularDependency()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestIndirectA>().ToTransient<TestIndirectA>();
            locator.Bind<ITestIndirectB>().ToTransient<TestIndirectB>();
            locator.Bind<ITestIndirectC>().ToTransient<TestIndirectC>();

            Assert.Throws<CyclicDependencyException>(() => locator.Get<ITestIndirectA>());
        }

        [Test]
        public void GetOnUnboundTypeShouldThrowMissingBinding()
        {
            var locator = new ServiceLocator();

            var exception = Assert.Throws<MissingBindingException>(() => locator.Get<ITestService>());
            Assert.AreEqual(typeof(ITestService), exception.Service);
        }

        [Test]
        public void MissingConstructorDependencyShouldThrow()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestParamsService>().ToTransient<TestParamsService>();

            var exception = Assert.Throws<MissingConstructorParamException>(() => locator.Get<ITestParamsService>());
            Assert.AreEqual("testService", exception.ParameterName);
            Assert.IsNotNull(exception.Service);
        }

        [Test]
        public void TryGetShouldNotThrowForUnboundType()
        {
            var locator = new ServiceLocator();

            Assert.IsFalse(locator.TryGet<ITestService>(out var missing));
            Assert.IsNull(missing);

            locator.Bind<ITestService>().ToTransient<TestService>();
            Assert.IsTrue(locator.TryGet<ITestService>(out var found));
            Assert.IsNotNull(found);
        }

        [Test]
        public void ShouldUseConstructorWithMostParameters()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToTransient<TestService>();
            locator.Bind<ITestMultiCtorService>().ToTransient<TestMultiCtorService>();

            Assert.IsTrue(locator.Get<ITestMultiCtorService>().WasInjected);
        }

        [Test]
        public void UnbindShouldRemoveBinding()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService>().ToTransient<TestService>();
            locator.Unbind<ITestService>();

            Assert.IsFalse(locator.IsBound<ITestService>());
            Assert.Throws<MissingBindingException>(() => locator.Get<ITestService>());
        }

        [Test]
        public void UnbindTwoContractsShouldRemoveBoth()
        {
            var locator = new ServiceLocator();
            locator.Bind<ITestService, ITestSecondContract>().ToSingleton<TestTwoContractsService>();
            locator.Unbind<ITestService, ITestSecondContract>();

            Assert.IsFalse(locator.IsBound<ITestService>());
            Assert.IsFalse(locator.IsBound<ITestSecondContract>());
        }

        [Test]
        public void RebindShouldReplacePreviousBinding()
        {
            var locator = new ServiceLocator();
            var first = new TestService();
            var second = new TestService();
            locator.Bind<ITestService>().ToConstant(first);
            locator.Bind<ITestService>().ToConstant(second);

            Assert.AreSame(second, locator.Get<ITestService>());
        }

        [Test]
        public void ModuleBindingsShouldBeAddedAndRemoved()
        {
            var locator = new ServiceLocator();
            var module = new TestModule();

            locator.RegisterModule(module);
            Assert.IsTrue(locator.IsModuleRegistered(module.Name));
            Assert.IsNotNull(locator.Get<ITestService>());

            locator.UnregisterModule(module.Name);
            Assert.IsFalse(locator.IsModuleRegistered(module.Name));
            Assert.IsFalse(locator.IsBound<ITestService>());
        }

        [Test]
        public void UnregisterModuleShouldKeepRootBindingThatWonOverModule()
        {
            var locator = new ServiceLocator();
            var rootInstance = new TestService();
            locator.Bind<ITestService>().ToConstant(rootInstance);

            var module = new TestModule();
            locator.RegisterModule(module);
            Assert.AreSame(rootInstance, locator.Get<ITestService>());

            locator.UnregisterModule(module.Name);
            Assert.AreSame(rootInstance, locator.Get<ITestService>());
        }
    }
}
