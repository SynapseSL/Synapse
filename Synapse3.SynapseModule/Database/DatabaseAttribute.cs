using System;

namespace Synapse3.SynapseModule.Database;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DatabaseAttribute : Attribute
{
    public DatabaseAttribute() { }
    
    public DatabaseAttribute(string name, uint id, uint priority, Type type)
    {
        Name = name;
        Id = id;
        Priority = priority;
        DataBaseType = type;
    }
    
    public string Name { get; set; }
    
    public uint Id { get; set; }
    
    public uint Priority { get; set; }
    
    public Type DataBaseType { get; internal set; }
}