using System;

namespace Synapse3.SynapseModule.Map.Rooms;

public class CustomRoomAttribute : Attribute
{
    public CustomRoomAttribute() { }
    
    public CustomRoomAttribute(string name, int id, int schematicId, Type roomType)
    {
        Name = name;
        Id = id;
        SchematicId = schematicId;
        RoomType = roomType;
    }
    
    public string Name { get; set; }
    
    public int Id { get; set; }
    
    public int SchematicId { get; set; }
    
    public Type RoomType { get; internal set; }
}