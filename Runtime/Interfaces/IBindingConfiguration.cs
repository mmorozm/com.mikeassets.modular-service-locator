namespace MikeAssets.ModularServiceLocator.Runtime
{
    public interface IBindingConfiguration
    {
        BindingType BindingType { get; set; }
        
        IBindingProvider Provider { get; set; }
    }
}
