using System;
using System.Linq;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public static class ResolutionRootExtension
    {
        /// <summary>Resolves <typeparamref name="T"/>. Throws <see cref="MissingBindingException"/> if it is not bound.</summary>
        public static T Get<T>(this IResolutionRoot resolutionRoot)
        {
            return (T)resolutionRoot.Get(typeof(T));
        }

        /// <summary>Resolves <paramref name="service"/>. Throws <see cref="MissingBindingException"/> if it is not bound.</summary>
        public static object Get(this IResolutionRoot resolutionRoot, Type service)
        {
            if (!resolutionRoot.Root.TryFindBinding(service, out var binding) || binding.Configuration.Provider == null)
            {
                throw new MissingBindingException(service);
            }

            var request = new ResolveRequest(resolutionRoot.Root, service);
            return binding.Configuration.Provider.ResolveValue(request);
        }

        /// <summary>Resolves <typeparamref name="T"/> if it is bound; returns false instead of throwing when it is not.</summary>
        public static bool TryGet<T>(this IResolutionRoot resolutionRoot, out T service)
        {
            if (!resolutionRoot.IsBound<T>())
            {
                service = default;
                return false;
            }

            service = resolutionRoot.Get<T>();
            return true;
        }

        public static bool IsBound<T>(this IResolutionRoot resolutionRoot)
        {
            return resolutionRoot.Root.TryFindBinding(typeof(T), out var binding) && binding.Configuration.Provider != null;
        }

        /// <summary>Eagerly creates every singleton binding that has not been created yet.</summary>
        public static void ResolveSingletons(this IResolutionRoot resolutionRoot)
        {
            var toResolve = resolutionRoot.Bindings
                .Where(binding => binding.Configuration.Provider is SingletonBindingProvider)
                .ToList();

            foreach (var binding in toResolve)
            {
                var request = new ResolveRequest(resolutionRoot.Root, binding.Service);
                binding.Configuration.Provider.ResolveValue(request);
            }
        }
    }
}
