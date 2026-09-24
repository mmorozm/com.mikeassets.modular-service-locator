namespace MikeAssets.ModularServiceLocator.Runtime
{
    public class BindingConfiguration : IBindingConfiguration
    {
        public BindingType BindingType { get; set; }

        public IBindingProvider Provider { get; set; }
    }
}
