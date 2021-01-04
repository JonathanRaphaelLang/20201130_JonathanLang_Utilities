using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class GetterSetterAttribute : Attribute
{
    [CanBeNull] public abstract string Shortcut { get; }
}


