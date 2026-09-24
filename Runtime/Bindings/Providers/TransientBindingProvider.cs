using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public class TransientBindingProvider : BindingProviderBase
    {
        private static readonly HashSet<Type> s_preserveWarningShown = new HashSet<Type>();

        private ConstructorInfo m_constructor;

        public Type Implementation { get; }

        public TransientBindingProvider(Type implementation)
        {
            Implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
        }
        
        public override object ResolveValue(IResolveRequest request)
        {
            if (!Contracts.Contains(request.Service))
            {
                return null;
            }

            if (request.IsCyclic(request.Service))
            {
                throw new CyclicDependencyException(request.Service);
            }

            var constructor = GetConstructor();
            CheckPreserveAttribute(constructor);

            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                return constructor.Invoke(null);
            }

            var childRequests = request.ChildRequests;
            var arguments = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var childRequest = childRequests != null && i < childRequests.Count ? childRequests[i] : null;

                if (childRequest == null || childRequest.Service != parameter.ParameterType
                    || !request.Root.TryFindBinding(parameter.ParameterType, out var binding))
                {
                    throw new MissingConstructorParamException(Implementation, parameter.Name);
                }

                arguments[i] = binding.Configuration.Provider.ResolveValue(childRequest);
            }

            return constructor.Invoke(arguments);
        }

        public override Dictionary<string, Type> GetConstructorParams()
        {
            return GetConstructor().GetParameters().ToDictionary(pr => pr.Name, pr => pr.ParameterType);
        }

        /// <summary>
        /// The public constructor with the most parameters is used, so a class can keep a
        /// parameterless constructor (e.g. for serialization) next to its injection constructor.
        /// </summary>
        private ConstructorInfo GetConstructor()
        {
            if (m_constructor != null)
            {
                return m_constructor;
            }

            var constructors = Implementation.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot construct type {Implementation}: it has no public constructor.");
            }

            m_constructor = constructors.OrderByDescending(ct => ct.GetParameters().Length).First();
            return m_constructor;
        }

        private void CheckPreserveAttribute(ConstructorInfo constructor)
        {
            if (!Application.isEditor)
            {
                return;
            }

            lock (s_preserveWarningShown)
            {
                if (!s_preserveWarningShown.Add(Implementation))
                {
                    return;
                }
            }

            if (!Attribute.IsDefined(constructor, typeof(PreserveAttribute)))
            {
                Debug.LogWarning($"Type {Implementation}: constructor {constructor} does not have the " +
                    $"{typeof(PreserveAttribute).FullName} attribute and could be removed when managed code stripping is enabled.");
            }
        }
    }
}
