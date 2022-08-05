using System;

namespace Synapse3.SynapseModule.Role;

public class RoleAttribute : Attribute
{
    public RoleAttribute() { }
    
    public RoleAttribute(string name, uint id, Type script)
    {
        Name = name;
        Id = id;
        RoleScript = script;
    }

    public string Name { get; set; }
    public uint Id { get; set; }
    public Type RoleScript { get; internal set; }
}