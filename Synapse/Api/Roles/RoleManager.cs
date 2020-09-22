using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        internal RoleManager()
        {
            //TODO: PlayerHurt,Generator,CheckEnd,Scp106-Containment,PocketEnter,PocketLeave,AddScp096Target
            SynapseController.Server.Events.Player.PlayerEscapseEvent += OnEscape;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += OnLeave;
            SynapseController.Server.Events.Player.PlayerDeathEvent += OnDeath;
            SynapseController.Server.Events.Player.PlayerEnterFemurEvent += OnFemur;
            SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;
        }

        public Dictionary<Type, string> CustomRoles = new Dictionary<Type, string>();

        public IRole GetCustomRole(string name) => (IRole)Activator.CreateInstance(CustomRoles.Keys.FirstOrDefault(x => x.Name.ToLower() == name));

        public void RegisterCustomRole<TRole>(string rolename) where TRole : IRole => CustomRoles.Add(typeof(TRole), rolename);

        #region Events
        private void OnEscape(Events.SynapseEventArguments.PlayerEscapeEventArgs ev)
        {
            if(ev.Player.CustomRole != null)
            {
                var escapeRole = ev.Player.CustomRole.GetEscapeRole();
                if (escapeRole == RoleType.None)
                {
                    ev.Allow = false;
                    return;
                }

                ev.SpawnRole = escapeRole;
                ev.Player.CustomRole.Escape();
            }
        }

        private void OnLeave(Events.SynapseEventArguments.PlayerLeaveEventArgs ev)
        {
            if (ev.Player.CustomRole != null)
                ev.Player.CustomRole.DeSpawn();
        }

        private void OnDeath(Events.SynapseEventArguments.PlayerDeathEventArgs ev)
        {
            if (ev.Victim.CustomRole != null)
                ev.Victim.CustomRole.DeSpawn();
        }

        private void OnRa(Events.SynapseEventArguments.RemoteAdminCommandEventArgs ev)
        {
            string[] args = ev.Command.Split(' ');
            if ((args[0].ToUpper() == "KILL" || args[0].ToUpper() == "FORCECLASS") && args.Count() > 1)
            {
                var ids = args[1].Split('.');
                foreach (var id in ids)
                {
                    if (string.IsNullOrEmpty(id))
                        continue;
                    var player = Server.Get.GetPlayer(int.Parse(id));
                    if (player == null) continue;
                    if (player.CustomRole != null) player.CustomRole.DeSpawn();
                }
            }
        }

        private void OnFemur(Events.SynapseEventArguments.PlayerEnterFemurEventArgs ev)
        {
            if(ev.Player.CustomRole != null)
            {
                if(ev.Player.CustomRole.GetFriends().Any(x => x == Team.SCP))
                {
                    ev.CloseFemur = false;
                    ev.Allow = false;
                    //TODO: Message for Enter Femur => Create Translation File for Synapse
                }
            }
        }
        #endregion
    }
}
