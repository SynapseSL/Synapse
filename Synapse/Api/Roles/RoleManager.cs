using System;
using System.Collections.Generic;
using System.Linq;
using Synapse.Api.Exceptions;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        public static RoleManager Get => Server.Get.RoleManager;

        public static readonly int HighestRole = (int)RoleType.ChaosMarauder;

        internal RoleManager() { }

        internal void Init()
        {
            SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;
        }

        public List<RoleInformation> CustomRoles { get; } = new List<RoleInformation>();

        public string GetRoleName(int id)
        {
            if (id >= -1 && id <= HighestRole)
                return ((RoleType)id).ToString();

            if (!IsIDRegistered(id)) throw new SynapseRoleNotFoundException("A Role was requested that is not registered in Synapse.Please check your configs and plugins", id);

            return CustomRoles.FirstOrDefault(x => x.ID == id).Name;
        }

        public IRole GetCustomRole(string name)
        {
            var roleinformation = CustomRoles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (roleinformation is null)
                throw new SynapseRoleNotFoundException("A Role was requested that is not registered in Synapse.Please check your configs and plugins", name);

            if (roleinformation.RoleScript.GetConstructors().Any(x => x.GetParameters().Count() == 1 && x.GetParameters().First().ParameterType == typeof(int)))
                return (IRole)Activator.CreateInstance(roleinformation.RoleScript, new object[] { roleinformation.ID });

            return (IRole)Activator.CreateInstance(roleinformation.RoleScript);
        }

        public IRole GetCustomRole(int id)
        {
            if (!IsIDRegistered(id)) throw new SynapseRoleNotFoundException("A Role was requested that is not registered in Synapse.Please check your configs and plugins", id);

            var roleinformation = CustomRoles.FirstOrDefault(x => x.ID == id);

            if (roleinformation.RoleScript.GetConstructors().Any(x => x.GetParameters().Count() == 1 && x.GetParameters().First().ParameterType == typeof(int)))
                return (IRole)Activator.CreateInstance(roleinformation.RoleScript, new object[] { roleinformation.ID });

            return (IRole)Activator.CreateInstance(roleinformation.RoleScript);
        }

        public void RegisterCustomRole<TRole>() where TRole : IRole
        {
            var role = (IRole)Activator.CreateInstance(typeof(TRole));
            var info = new RoleInformation(role.GetRoleName(), role.GetRoleID(), typeof(TRole));

            if (role.GetRoleID() >= 0 && role.GetRoleID() <= HighestRole) throw new SynapseRoleAlreadyRegisteredException("A Plugin tried to register a CustomRole with an Id of a vanilla RoleType", info);
            if (IsIDRegistered(role.GetRoleID())) throw new SynapseRoleAlreadyRegisteredException("A Role was registered with an already registered ID", info);

            CustomRoles.Add(info);
        }

        public void RegisterCustomRole(RoleInformation role)
        {
            if (role.ID >= 0 && role.ID <= HighestRole) throw new SynapseRoleAlreadyRegisteredException("A Plugin tried to register a CustomRole with an Id of a vanilla RoleType", role);
            if (IsIDRegistered(role.ID)) throw new SynapseRoleAlreadyRegisteredException("A Role was registered with an already registered ID", role);

            CustomRoles.Add(role);
        }

        public void UnRegisterCustomRole(int id)
        {
            var role = CustomRoles.FirstOrDefault(x => x.ID == id);
            if (role != null)
                CustomRoles.Remove(role);
        }

        public bool IsIDRegistered(int id)
        {
            if (id >= 0 && id <= HighestRole) return true;

            if (CustomRoles.Any(x => x.ID == id)) return true;

            return false;
        }

        #region Events
        private void OnRa(Events.SynapseEventArguments.RemoteAdminCommandEventArgs ev)
        {
            var args = ev.Command.Split(' ');
            if (args[0].ToUpper() != "OVERWATCH" && args[0].ToUpper() != "KILL" && args[0].ToUpper() != "FORCECLASS" || args.Count() <= 1) return;
            var ids = args[1].Split('.');
            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                var player = Server.Get.GetPlayer(int.Parse(id));
                if (player == null) continue;

                if (player.CustomRole is not null)
                    player.CustomRole = null;
            }
        }
        #endregion
    }
}