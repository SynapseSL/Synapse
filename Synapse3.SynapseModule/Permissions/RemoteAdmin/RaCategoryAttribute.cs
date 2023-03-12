using System;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RaCategoryAttribute : Attribute
{
    public RaCategoryAttribute() { }

    public RaCategoryAttribute(string name, uint id, Type categoryType)
    {
        Name = name;
        Id = id;
        CategoryType = categoryType;
    }
    
    public string Name { get; set; }

    public string Color { get; set; } = "white";

    public int Size { get; set; } = 20;
    
    public string RemoteAdminIdentifier { get; set; }
    
    public uint Id { get; set; }
    
    public Type CategoryType { get; internal set; }
}