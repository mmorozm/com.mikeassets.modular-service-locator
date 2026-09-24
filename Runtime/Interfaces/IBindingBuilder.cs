using System;

namespace MikeAssets.ModularServiceLocator.Runtime
{
    public interface IBindingBuilder<T>
    {
        /// <summary>A new instance is created on every resolve.</summary>
        void ToTransient<TImplementation>() where TImplementation : T;

        /// <summary>The given instance is returned on every resolve.</summary>
        void ToConstant(object constant);

        /// <summary>One instance is created lazily on first resolve (or by ResolveSingletons) and reused.</summary>
        void ToSingleton<TImplementation>() where TImplementation : T;

        [Obsolete("Use ToSingleton<TImplementation>() instead.")]
        void ToSingletone<TImplementation>() where TImplementation : T;
    }
    
    public interface IBindingBuilder<T1, T2>
    {
        /// <summary>A new instance is created on every resolve.</summary>
        void ToTransient<TImplementation>() where TImplementation : T1, T2;

        /// <summary>The given instance is returned on every resolve.</summary>
        void ToConstant(object constant);

        /// <summary>One instance is created lazily on first resolve (or by ResolveSingletons) and shared by both contracts.</summary>
        void ToSingleton<TImplementation>() where TImplementation : T1, T2;

        [Obsolete("Use ToSingleton<TImplementation>() instead.")]
        void ToSingletone<TImplementation>() where TImplementation : T1, T2;
    }
}
