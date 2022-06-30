using System;

namespace Synapse3.SynapseModule.Role;

public class RoleInformation : Attribute
{
    public RoleInformation() { }
    
    public RoleInformation(string name, int id, Type script)
    {
        Name = name;
        ID = id;
        RoleScript = script;
    }

    public string Name { get; set; }
    public int ID { get; set; }
    public Type RoleScript { get; internal set; }
}