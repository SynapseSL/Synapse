using System;

namespace Synapse3.SynapseModule.Database;

public class DataBaseAttribute : Attribute
{
    public DataBaseAttribute() { }
    
    public DataBaseAttribute(string name, uint id, uint priority, Type type)
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