using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public abstract class BindingRoot : IBindingRoot
    {
        private readonly ConcurrentDictionary<string, LocatorModule> m_modules;
        protected readonly ConcurrentDictionary<Type, IBinding> m_bindings;

        protected BindingRoot()
        {
            m_bindings = new ConcurrentDictionary<Type, IBinding>();
            m_modules = new ConcurrentDictionary<string, LocatorModule>();
        }

        public List<IBinding> RootBindings => m_bindings.Values.ToList();

        /// <summary>Looks up the binding registered for <paramref name="service"/> without copying the binding list.</summary>
        public bool TryGetBinding(Type service, out IBinding binding)
        {
            return m_bindings.TryGetValue(service, out binding);
        }

        /// <summary>Adds a binding. A binding already registered for the same service is replaced (last bind wins).</summary>
        public virtual void AddBinding(IBinding binding)
        {
            m_bindings[binding.Service] = binding;
        }

        public virtual void RemoveBinding(IBinding binding)
        {
            RemoveIfSame(binding);
        }

        public IBindingBuilder<T> Bind<T>()
        {
            var binding = new Binding(typeof(T));
            
            AddBinding(binding);
            
            return new BindingBuilder<T>(binding.Configuration);
        }

        public IBindingBuilder<T1, T2> Bind<T1, T2>()
        {
            var binding = new Binding(typeof(T1));
            var secondBinding = new Binding(typeof(T2), binding.Configuration);
            
            AddBinding(binding);
            AddBinding(secondBinding);
            
            return new BindingBuilder<T1, T2>(binding.Configuration);
        }

        public void Unbind<T>()
        {
            m_bindings.TryRemove(typeof(T), out _);
        }

        public void Unbind<T1, T2>()
        {
            m_bindings.TryRemove(typeof(T1), out _);
            m_bindings.TryRemove(typeof(T2), out _);
        }

        protected bool IsModuleExists(string name)
        {
            return m_modules.ContainsKey(name);
        }
        
        protected virtual void RegisterModuleInternal(LocatorModule module)
        {
            if (!m_modules.TryAdd(module.Name, module))
            {
                return;
            }
            
            module.Load();
            var bindings = module.RootBindings;

            foreach (var binding in bindings)
            {
                // Bindings that already exist in the root take precedence over module bindings.
                m_bindings.TryAdd(binding.Service, binding);
            }
        }

        protected virtual void UnregisterModuleInternal(string name)
        {
            if (!m_modules.TryGetValue(name, out var module))
            {
                return;
            }

            var bindings = module.RootBindings;

            foreach (var binding in bindings)
            {
                // Only remove the binding if it is still the one this module registered.
                RemoveIfSame(binding);
            }
            
            module.Unload();
            m_modules.TryRemove(name, out _);
        }

        private bool RemoveIfSame(IBinding binding)
        {
            ICollection<KeyValuePair<Type, IBinding>> collection = m_bindings;
            return collection.Remove(new KeyValuePair<Type, IBinding>(binding.Service, binding));
        }
    }
}
