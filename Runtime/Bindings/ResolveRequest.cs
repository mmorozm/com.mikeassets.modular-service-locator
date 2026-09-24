using System;
using System.Collections.Generic;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public class ResolveRequest : IResolveRequest
    {
        public IReadOnlyBindingRoot Root { get; }
        
        public IResolveRequest ParentRequest { get; }

        public IList<IResolveRequest> ChildRequests { get; set; }

        public Type Service { get; }

        public ResolveRequest(IReadOnlyBindingRoot root, Type service) : this(root, service, null)
        {
        }

        private ResolveRequest(IReadOnlyBindingRoot root, Type service, IResolveRequest parentRequest)
        {
            Root = root;
            Service = service;
            ParentRequest = parentRequest;
            ChildRequests = new List<IResolveRequest>();

            BuildChildRequestsGraph();
        }

        /// <summary>Returns true if <paramref name="serviceToCheck"/> is already being resolved further up the chain.</summary>
        public bool IsCyclic(Type serviceToCheck)
        {
            for (var request = ParentRequest; request != null; request = request.ParentRequest)
            {
                if (request.Service == serviceToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildChildRequestsGraph()
        {
            if (!Root.TryFindBinding(Service, out var binding))
            {
                throw new MissingBindingException(Service);
            }

            var constructorParams = binding.Configuration.Provider?.GetConstructorParams();
            if (constructorParams == null)
            {
                return;
            }

            foreach (var param in constructorParams)
            {
                if (param.Value == Service || IsCyclic(param.Value))
                {
                    throw new CyclicDependencyException(Service, param.Value);
                }

                if (!Root.TryFindBinding(param.Value, out _))
                {
                    throw new MissingConstructorParamException(Service, param.Key);
                }

                ChildRequests.Add(new ResolveRequest(Root, param.Value, this));
            }
        }
    }
}
