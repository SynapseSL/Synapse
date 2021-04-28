using System;

namespace Synapse.Api.Roles
{
    public class RoleInformation
    {
        public RoleInformation(string name, int id, Type script)
        {
            Name = name;
            ID = id;
            RoleScript = script;
        }

        public string Name { get; }
        public int ID { get; }
        public Type RoleScript { get; }
    }
}
