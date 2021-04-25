namespace Synapse.Api.Modules
{
    public interface ISynapseModule
    {
        string Name { get; }

        void Load();

        void Reload();
    }
}
