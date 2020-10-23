using System;
using System.Collections.Generic;
using System.Linq;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        public const int HighestRole = (int)RoleType.Scp93989;

        internal RoleManager() { }

        internal void Init()
        {
            SynapseController.Server.Events.Player.PlayerEscapseEvent += OnEscape;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += OnLeave;
            SynapseController.Server.Events.Player.PlayerEnterFemurEvent += OnFemur;
            SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;
            SynapseController.Server.Events.Player.PlayerDamageEvent += OnDamage;
            SynapseController.Server.Events.Scp.Scp096.Scp096AddTargetEvent += On096Target;
            SynapseController.Server.Events.Scp.Scp106.Scp106ContainmentEvent += On106Containment;
            SynapseController.Server.Events.Player.PlayerGeneratorInteractEvent += OnGenerator;
            SynapseController.Server.Events.Scp.Scp106.PocketDimensionEnterEvent += Scp106OnPocketDimensionEnterEvent;
            SynapseController.Server.Events.Scp.Scp106.PocketDimensionLeaveEvent += Scp106OnPocketDimensionLeaveEvent;
            Server.Get.Events.Round.RoundCheckEvent += CheckEnd;
        }

        public Dictionary<Type, KeyValuePair<string, int>> CustomRoles = new Dictionary<Type, KeyValuePair<string, int>>();


        public IRole GetCustomRole(string name) => (IRole)Activator.CreateInstance(CustomRoles.FirstOrDefault(x => x.Value.Key.ToLower() == name.ToLower()).Key);

        public IRole GetCustomRole(int id) => (IRole)Activator.CreateInstance(CustomRoles.FirstOrDefault(x => x.Value.Value == id).Key);

        public void RegisterCustomRole<TRole>() where TRole : IRole
        {
            var role = (IRole)Activator.CreateInstance(typeof(TRole));

            if (role.GetRoleID() >= 0 && role.GetRoleID() <= HighestRole) throw new Exception("A Plugin tried to register a CustomRole with an Id of an Vanilla RoleType");

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
        private void CheckEnd(RoundCheckEventArgs ev)
        {
            List<Team> teams = Server.Get.Players.Select(x => x.RealTeam).ToList();

            var teamAmounts = 0;
            if (teams.Contains(Team.MTF)) teamAmounts++;
            if (teams.Contains(Team.RSC)) teamAmounts++;
            if (teams.Contains(Team.CHI)) teamAmounts++;
            if (teams.Contains(Team.CDP)) teamAmounts++;
            if (teams.Contains(Team.SCP)) teamAmounts++;

            var roundEnd = teamAmounts < 2;
            if (teamAmounts == 2)
            {
                if (teams.Contains(Team.CHI) && teams.Contains(Team.SCP))
                    roundEnd = Server.Get.Configs.SynapseConfiguration.ChaosScpEnd;

                if (teams.Contains(Team.CHI) && teams.Contains(Team.CDP))
                    roundEnd = true;

                if (teams.Contains(Team.MTF) && teams.Contains(Team.RSC))
                    roundEnd = true;
            }

            foreach (var role in Server.Get.GetPlayers(x => x.CustomRole != null).Select(x => x.CustomRole))
                if (role.GetEnemys().Any(t => teams.Contains(t)))
                    roundEnd = false;

            if (!roundEnd) ev.Allow = false;
            else
            {
                ev.ForceEnd = true;

                if (RoundSummary.escaped_ds + teams.Count(x => x == Team.CDP) > 0)
                {
                    if (!teams.Contains(Team.SCP) && !teams.Contains(Team.CHI))
                        ev.Team = RoundSummary.LeadingTeam.Draw;
                    else
                        ev.Team = RoundSummary.LeadingTeam.ChaosInsurgency;
                }
                else
                {
                    if (teams.Contains(Team.MTF) || teams.Contains(Team.RSC))
                        ev.Team = RoundSummary.escaped_scientists + teams.Count(x => x == Team.RSC) > 0 ? RoundSummary.LeadingTeam.FacilityForces : RoundSummary.LeadingTeam.Draw;
                    else ev.Team = RoundSummary.LeadingTeam.Anomalies;
                }
            }
        }

        private void OnGenerator(Events.SynapseEventArguments.PlayerGeneratorInteractEventArgs ev)
        {
            if(ev.Player.CustomRole != null && ev.Player.CustomRole.GetFriends().Any(x => x == Team.SCP))
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

        private void On096Target(Events.SynapseEventArguments.Scp096AddTargetEventArgument ev)
        {
            if (ev.Player.CustomRole != null && ev.Player.CustomRole.GetTeam() == Team.SCP && !Server.Get.Configs.SynapseConfiguration.ScpTrigger096)
                ev.Allow = false;
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
