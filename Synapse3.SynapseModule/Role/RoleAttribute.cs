using System;

namespace Synapse3.SynapseModule.Role;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class RoleAttribute : Attribute
{
    public RoleAttribute() { }
    
    public RoleAttribute(string name, uint id,uint teamId, Type script)
    {
        Name = name;
        Id = id;
        TeamId = teamId;
        RoleScript = script;
    }

    public string Name { get; set; }
    public uint Id { get; set; }
    public uint TeamId { get; set; }
    public Type RoleScript { get; internal set; }
}