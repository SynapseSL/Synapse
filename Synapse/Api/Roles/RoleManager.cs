using System;
using System.Collections.Generic;
using System.Linq;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        public static readonly int HighestRole = (int)RoleType.Scp93989;

        internal RoleManager() { }

        internal void Init()
        {
            SynapseController.Server.Events.Player.PlayerEscapesEvent += OnEscape;
            SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;
        }

        public Dictionary<Type, KeyValuePair<string, int>> CustomRoles = new Dictionary<Type, KeyValuePair<string, int>>();


        public string GetRoleName(int id)
        {
            if (id >= -1 && id <= HighestRole)
                return ((RoleType)id).ToString();

            if (!IsIDRegistered(id)) throw new Exception("Plugin tried to get the Name of a non registered Role");

            return CustomRoles.Values.First(x => x.Value == id).Key;
        }

        public IRole GetCustomRole(string name) => (IRole)Activator.CreateInstance(CustomRoles.FirstOrDefault(x => x.Value.Key.ToLower() == name.ToLower()).Key);

        public IRole GetCustomRole(int id) => (IRole)Activator.CreateInstance(CustomRoles.FirstOrDefault(x => x.Value.Value == id).Key);

        public void RegisterCustomRole<TRole>() where TRole : IRole
        {
            var role = (IRole)Activator.CreateInstance(typeof(TRole));

            if (role.GetRoleID() >= 0 && role.GetRoleID() <= HighestRole) throw new Exception("A Plugin tried to register a CustomRole with an Id of a vanilla RoleType");
            if (!Server.Get.TeamManager.IsIDRegistered(role.GetTeamID())) Logger.Get.Warn($"The role {role.GetRoleName()} is using a not registered Team");

            var pair = new KeyValuePair<string, int>(role.GetRoleName(), role.GetRoleID());

            CustomRoles.Add(typeof(TRole), pair);
        }

        public bool IsIDRegistered(int id)
        {
            if (id >= 0 && id <= HighestRole) return true;

            if (CustomRoles.Any(x => x.Value.Value == id)) return true;

            return false;
        }

        #region Events
        private void OnEscape(Events.SynapseEventArguments.PlayerEscapeEventArgs ev)
        {
            if (ev.Player.CustomRole == null) return;
            var escapeRole = ev.Player.CustomRole.GetEscapeRole();
            if (escapeRole == -1)
            {
                ev.Allow = false;
                return;
            }

            ev.Player.CustomRole.Escape();
            ev.Player.RoleID = escapeRole;
        }

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

                if (player.CustomRole != null)
                    player.CustomRole = null;
            }
        }
        #endregion
    }
}
