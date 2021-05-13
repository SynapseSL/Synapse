using Synapse.Api.Plugin;

namespace Synapse.Network.Models
{
    public class SerializablePlugin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }

        public static SerializablePlugin FromAttribute(PluginInformation information)
        {
            return new SerializablePlugin
            {
                Name = information.Name,
                Description = information.Description,
                Version = information.Version,
                Author = information.Author
            };
        }
    }
}