using Synapse.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        public static RoleManager Get
            => Server.Get.RoleManager;

        public static readonly int HighestRole = (int)RoleType.ChaosMarauder;

        internal RoleManager()
            => CustomRoles = new List<RoleInformation>();

        internal void Init()
            => SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;

        public List<RoleInformation> CustomRoles { get; }

        public string GetRoleName(int id)
        {
            if (id >= -1 && id <= HighestRole)
            {
                return ((RoleType)id).ToString();
            }
            else if (IsIDRegistered(id))
            {
                return CustomRoles.FirstOrDefault(x => x.ID == id).Name;
            }
            else
            {
                throw new SynapseRoleNotFoundException("A Role was requested that is not registered in Synapse.Please check your configs and plugins", id);
            }
        }

        public IRole GetCustomRole(string name)
        {
            var roleinformation = CustomRoles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (roleinformation is null)
                throw new SynapseRoleNotFoundException("A Role was requested that is not registered in Synapse.Please check your configs and plugins", name);

            var ctors = roleinformation.RoleScript.GetConstructors();
            var preferredCtor = ctors.FirstOrDefault(x => x.GetParameters().FirstOrDefault()?.ParameterType == typeof(int));

            if (preferredCtor != null)
            {
                return (IRole)Activator.CreateInstance(roleinformation.RoleScript, new object[] { roleinformation.ID });
            }
            else
            {
                return (IRole)Activator.CreateInstance(roleinformation.RoleScript);
            }
        }

        public IRole GetCustomRole(int id)
        {
            var roleinformation = CustomRoles.FirstOrDefault(x => x.ID == id);
            if (roleinformation is null)
                throw new SynapseRoleNotFoundException("A Role was requested that is not registered in Synapse. Please check your configs and plugins", id);

            var ctors = roleinformation.RoleScript.GetConstructors();
            var preferredCtor = ctors.FirstOrDefault(x => x.GetParameters().FirstOrDefault()?.ParameterType == typeof(int));

            if (preferredCtor != null)
            {
                return (IRole)Activator.CreateInstance(roleinformation.RoleScript, new object[] { roleinformation.ID });
            }
            else
            {
                return (IRole)Activator.CreateInstance(roleinformation.RoleScript);
            }
        }

        public void RegisterCustomRole<TRole>() where TRole : IRole
        {
            var role = (IRole)Activator.CreateInstance(typeof(TRole));
            var info = new RoleInformation(role.GetRoleName(), role.GetRoleID(), typeof(TRole));

            if (role.GetRoleID() >= 0 && role.GetRoleID() <= HighestRole)
                throw new SynapseRoleAlreadyRegisteredException("A Plugin tried to register a CustomRole with an Id of a vanilla RoleType", info);
            if (IsIDRegistered(role.GetRoleID()))
                throw new SynapseRoleAlreadyRegisteredException("A Role was registered with an already registered ID", info);

            CustomRoles.Add(info);
        }

        public void RegisterCustomRole(RoleInformation role)
        {
            if (role.ID >= 0 && role.ID <= HighestRole)
                throw new SynapseRoleAlreadyRegisteredException("A Plugin tried to register a CustomRole with an Id of a vanilla RoleType", role);
            if (IsIDRegistered(role.ID))
                throw new SynapseRoleAlreadyRegisteredException("A Role was registered with an already registered ID", role);

            CustomRoles.Add(role);
        }

        public void UnRegisterCustomRole(int id)
        {
            var role = CustomRoles.FirstOrDefault(x => x.ID == id);
            if (role != null)
                _ = CustomRoles.Remove(role);
        }

        public bool IsIDRegistered(int id)
            => CustomRoles.Any(x => x.ID == id) || (id >= 0 && id <= HighestRole);

        #region Events
        private void OnRa(Events.SynapseEventArguments.RemoteAdminCommandEventArgs ev)
        {
            var args = ev.Command.Split(' ');
            if ((!args[0].Equals("OVERWATCH", StringComparison.InvariantCultureIgnoreCase) && !args[0].Equals("KILL", StringComparison.InvariantCultureIgnoreCase) && !args[0].Equals("FORCECLASS", StringComparison.InvariantCultureIgnoreCase)) || args.Length <= 1)
                return;
            var ids = args[1].Split('.');
            foreach (var id in ids)
            {
                if (String.IsNullOrEmpty(id))
                    continue;
                var player = Server.Get.GetPlayer(Int32.Parse(id));
                if (player is null)
                    continue;

                if (player.CustomRole != null)
                    player.CustomRole = null;
            }
        }
        #endregion
    }
}
