using System;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public class SingletonBindingProvider : TransientBindingProvider
    {
        private readonly object m_lock = new object();
        private object m_service;

        public SingletonBindingProvider(Type implementation) 
            : base(implementation)
        {
        }

        public bool IsResolved => m_service != null;
        
        public override object ResolveValue(IResolveRequest request)
        {
            if (m_service != null)
            {
                return m_service;
            }

            lock (m_lock)
            {
                if (m_service == null)
                {
                    m_service = base.ResolveValue(request);
                }
            }

            return m_service;
        }
    }

    [Obsolete("Use SingletonBindingProvider instead.")]
    public class SingletoneBindingProvider : SingletonBindingProvider
    {
        public SingletoneBindingProvider(Type implementation) : base(implementation)
        {
        }
    }
}
