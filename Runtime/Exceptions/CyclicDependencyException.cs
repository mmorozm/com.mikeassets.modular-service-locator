using System;
using System.Collections.Generic;

namespace MikeAssets.ModularServiceLocator
{
    public class CyclicDependencyException : Exception
    {
        public List<Type> Types { get; }
        
        public CyclicDependencyException(Type service1, Type service2) :
            base($"Cannot construct type {service1}, type is in cyclic dependency with {service2}")
        {
            Types = new List<Type> { service1, service2 };
        }
        
        public CyclicDependencyException(Type service1) :
            base($"Cannot construct type {service1}, type is in cyclic dependency")
        {
            Types = new List<Type> { service1 };
        }
    }
}
