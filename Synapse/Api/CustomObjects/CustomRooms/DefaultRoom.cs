namespace Synapse.Api.CustomObjects.CustomRooms
{
    public class DefaultRoom : CustomRoom
    {
        public override string Name => "DefaultRoom";
        public override int ID => 0;
        public override int ZoneID => 5;
        public override int SchematicID => 1;
    }
}