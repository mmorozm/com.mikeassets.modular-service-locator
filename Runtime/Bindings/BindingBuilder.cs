using System;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public class BindingBuilder<T> : BindingBuilderBase, IBindingBuilder<T>
    {
        public BindingBuilder(IBindingConfiguration bindingConfiguration) : base(bindingConfiguration)
        {
        }

        public void ToTransient<TImplementation>() where TImplementation : T
        {
            SetProvider(BindingType.Transient, new TransientBindingProvider(typeof(TImplementation)));
        }

        public void ToConstant(object constant)
        {
            SetProvider(BindingType.Constant, new ConstantBindingProvider(constant));
        }

        public void ToSingleton<TImplementation>() where TImplementation : T
        {
            SetProvider(BindingType.Singleton, new SingletonBindingProvider(typeof(TImplementation)));
        }

        [Obsolete("Use ToSingleton<TImplementation>() instead.")]
        public void ToSingletone<TImplementation>() where TImplementation : T
        {
            ToSingleton<TImplementation>();
        }

        void SetProvider(BindingType bindingType, IBindingProvider provider)
        {
            provider.Contracts.Add(typeof(T));

            m_configuration.BindingType = bindingType;
            m_configuration.Provider = provider;
        }
    }
    
    public class BindingBuilder<T1, T2> : BindingBuilderBase, IBindingBuilder<T1, T2>
    {
        public BindingBuilder(IBindingConfiguration bindingConfiguration) : base(bindingConfiguration)
        {
        }

        public void ToTransient<TImplementation>() where TImplementation : T1, T2
        {
            SetProvider(BindingType.Transient, new TransientBindingProvider(typeof(TImplementation)));
        }

        public void ToConstant(object constant)
        {
            SetProvider(BindingType.Constant, new ConstantBindingProvider(constant));
        }

        public void ToSingleton<TImplementation>() where TImplementation : T1, T2
        {
            SetProvider(BindingType.Singleton, new SingletonBindingProvider(typeof(TImplementation)));
        }

        [Obsolete("Use ToSingleton<TImplementation>() instead.")]
        public void ToSingletone<TImplementation>() where TImplementation : T1, T2
        {
            ToSingleton<TImplementation>();
        }

        void SetProvider(BindingType bindingType, IBindingProvider provider)
        {
            provider.Contracts.Add(typeof(T1));
            provider.Contracts.Add(typeof(T2));

            m_configuration.BindingType = bindingType;
            m_configuration.Provider = provider;
        }
    }
}
