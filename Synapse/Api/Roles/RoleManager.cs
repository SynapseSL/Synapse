using System;
using System.Collections.Generic;
using System.Linq;
using Utf8Json.Internal.DoubleConversion;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        internal RoleManager()
        {
            //TODO: Hook Events and implements Check for everything (see MoreRolesManager)
        }

        public Dictionary<Type, string> CustomRoles = new Dictionary<Type, string>();

        public IRole GetCustomRole(string name) => (IRole)Activator.CreateInstance(CustomRoles.Keys.FirstOrDefault(x => x.Name.ToLower() == name));

        public void AddCustomRole<TRole>(string rolename) where TRole : IRole => CustomRoles.Add(typeof(TRole), rolename);
    }
}
