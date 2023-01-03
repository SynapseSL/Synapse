using System;

namespace Synapse3.SynapseModule.Map.Rooms;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CustomRoomAttribute : Attribute
{
    public CustomRoomAttribute() { }
    
    public CustomRoomAttribute(string name, uint id, uint schematicId, Type roomType)
    {
        Name = name;
        Id = id;
        SchematicId = schematicId;
        RoomType = roomType;
    }
    
    public string Name { get; set; }
    
    public uint Id { get; set; }
    
    public uint SchematicId { get; set; }
    
    public Type RoomType { get; internal set; }
}