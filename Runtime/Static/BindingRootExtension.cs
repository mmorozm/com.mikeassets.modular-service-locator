using System;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    internal static class BindingRootExtension
    {
        public static bool TryFindBinding(this IReadOnlyBindingRoot root, Type service, out IBinding binding)
        {
            if (root is BindingRoot bindingRoot)
            {
                return bindingRoot.TryGetBinding(service, out binding);
            }

            foreach (var candidate in root.RootBindings)
            {
                if (candidate.Service == service)
                {
                    binding = candidate;
                    return true;
                }
            }

            binding = null;
            return false;
        }
    }
}
