using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class GetSetAttribute : GetterSetterAttribute
{
    
    public override string Shortcut { get; }

    public GetSetAttribute() { }
        
    public GetSetAttribute(string shortcut = null)
    {
        Shortcut = shortcut;
    }
}
