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
            SynapseController.Server.Events.Player.PlayerLeaveEvent += OnLeave;
            SynapseController.Server.Events.Player.PlayerEnterFemurEvent += OnFemur;
            SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;
            SynapseController.Server.Events.Player.PlayerDamageEvent += OnDamage;
            SynapseController.Server.Events.Scp.Scp106.Scp106ContainmentEvent += On106Containment;
            SynapseController.Server.Events.Player.PlayerGeneratorInteractEvent += OnGenerator;
            SynapseController.Server.Events.Scp.Scp106.PocketDimensionEnterEvent += Scp106OnPocketDimensionEnterEvent;
            SynapseController.Server.Events.Scp.Scp106.PocketDimensionLeaveEvent += Scp106OnPocketDimensionLeaveEvent;
        }

        public Dictionary<Type, KeyValuePair<string, int>> CustomRoles = new Dictionary<Type, KeyValuePair<string, int>>();


        public IRole GetCustomRole(string name) => (IRole)Activator.CreateInstance(CustomRoles.FirstOrDefault(x => x.Value.Key.ToLower() == name.ToLower()).Key);

        public IRole GetCustomRole(int id) => (IRole)Activator.CreateInstance(CustomRoles.FirstOrDefault(x => x.Value.Value == id).Key);

        public void RegisterCustomRole<TRole>() where TRole : IRole
        {
            var role = (IRole)Activator.CreateInstance(typeof(TRole));

            if (role.GetRoleID() >= 0 && role.GetRoleID() <= HighestRole) throw new Exception("A Plugin tried to register a CustomRole with an Id of a vanilla RoleType");

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

        private void OnGenerator(Events.SynapseEventArguments.PlayerGeneratorInteractEventArgs ev)
        {
            if (ev.GeneratorInteraction == Enum.GeneratorInteraction.TabletInjected || ev.GeneratorInteraction == Enum.GeneratorInteraction.Unlocked)
                if (ev.Player.CustomRole != null && ev.Player.CustomRole.GetFriends().Any(x => x == Team.SCP))
                {
                    ev.Allow = false;
                    ev.Player.InstantBroadcast(3, Server.Get.Configs.SynapseTranslation.GetTranslation("scpteam"));
                }
        }

        private void OnDamage(Events.SynapseEventArguments.PlayerDamageEventArgs ev)
        {
            var info = ev.HitInfo;

            if (ev.Victim.CustomRole != null && ev.Victim.RealTeam == Team.SCP && ev.HitInfo.GetDamageType() == DamageTypes.Pocket)
                info.Amount = 0;

            if (ev.Killer == null || ev.Victim == ev.Killer)
                return;

            if (ev.Victim.CustomRole != null && ev.Victim.CustomRole.GetFriends().Any(x => x == ev.Killer.RealTeam))
            {
                info.Amount = 0;
                ev.Killer.InstantBroadcast(3, Server.Get.Configs.SynapseTranslation.GetTranslation("sameteam"));
            }

            if (ev.Killer.CustomRole != null && ev.Killer.CustomRole.GetFriends().Any(x => x == ev.Victim.RealTeam))
            {
                info.Amount = 0;
                ev.Killer.InstantBroadcast(3, Server.Get.Configs.SynapseTranslation.GetTranslation("sameteam"));
            }

            ev.HitInfo = info;
        }

        private void OnEscape(Events.SynapseEventArguments.PlayerEscapeEventArgs ev)
        {
            if (ev.Player.CustomRole == null) return;
            var escapeRole = ev.Player.CustomRole.GetEscapeRole();
            if (escapeRole == RoleType.None)
            {
                ev.Allow = false;
                return;
            }

            ev.SpawnRole = escapeRole;
            ev.Player.CustomRole.Escape();
        }
        
        private void Scp106OnPocketDimensionEnterEvent(PocketDimensionEnterEventArgs ev)
        {
            if (ev.Player.CustomRole != null && ev.Player.CustomRole.GetFriends().Any(x => x == Team.SCP))
                ev.Allow = false;
        }
        
        private void Scp106OnPocketDimensionLeaveEvent(PocketDimensionLeaveEventArgs ev)
        {
            if (ev.Player.CustomRole != null && ev.Player.CustomRole.GetFriends().Any(x => x == Team.SCP))
                ev.TeleportType = PocketDimensionTeleport.PDTeleportType.Exit;
        }

        private void On106Containment(Events.SynapseEventArguments.Scp106ContainmentEventArgs ev)
        {
            if (ev.Player.CustomRole == null || ev.Player.CustomRole.GetFriends().All(x => x != Team.SCP)) return;
            ev.Allow = false;
            ev.Player.InstantBroadcast(2, Server.Get.Configs.SynapseTranslation.GetTranslation("scpteam"));
        }

        private void OnLeave(Events.SynapseEventArguments.PlayerLeaveEventArgs ev)
        {
            if (ev.Player.CustomRole != null)
                ev.Player.CustomRole = null;
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

        private void OnFemur(Events.SynapseEventArguments.PlayerEnterFemurEventArgs ev)
        {
            if (ev.Player.CustomRole == null) return;
            if (ev.Player.CustomRole.GetFriends().All(x => x != Team.SCP)) return;
            ev.CloseFemur = false;
            ev.Allow = false;
            ev.Player.Broadcast(3, Server.Get.Configs.SynapseTranslation.GetTranslation("scpteam"));
        }
        #endregion
    }
}
