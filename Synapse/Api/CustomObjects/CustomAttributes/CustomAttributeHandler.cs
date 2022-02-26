namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public class CustomAttributeHandler
    {
        public Breakable Breakable { get; } = new Breakable();

        public SchematicDoor SchematicDoor { get; } = new SchematicDoor();

        internal void Init()
        {
            Breakable.Init();
            SchematicDoor.Init();
        }
    }
}
