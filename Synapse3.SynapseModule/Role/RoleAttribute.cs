using System;

namespace Synapse3.SynapseModule.Role;

public class RoleAttribute : Attribute
{
    public RoleAttribute() { }
    
    public RoleAttribute(string name, int id, Type script)
    {
        Name = name;
        Id = id;
        RoleScript = script;
    }

    public string Name { get; set; }
    public int Id { get; set; }
    public Type RoleScript { get; internal set; }
}