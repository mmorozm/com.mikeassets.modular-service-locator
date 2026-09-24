using System;

namespace MikeAssets.ModularServiceLocator
{
    public class MissingBindingException : Exception
    {
        public Type Service { get; }

        public MissingBindingException(Type service) :
            base($"No binding registered for type {service}")
        {
            Service = service;
        }
    }
}
