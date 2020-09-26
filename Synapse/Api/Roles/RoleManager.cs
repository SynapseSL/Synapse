using Synapse.Patches.SynapsePatches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Roles
{
    public class RoleManager
    {
        internal RoleManager()
        {
            //TODO: Generator,Scp106-Containment,PocketEnter,PocketLeave
            SynapseController.Server.Events.Player.PlayerEscapseEvent += OnEscape;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += OnLeave;
            SynapseController.Server.Events.Player.PlayerDeathEvent += OnDeath;
            SynapseController.Server.Events.Player.PlayerEnterFemurEvent += OnFemur;
            SynapseController.Server.Events.Server.RemoteAdminCommandEvent += OnRa;
            SynapseController.Server.Events.Player.PlayerDamageEvent += OnDamage;
            SynapseController.Server.Events.Round.RoundCheckEvent += CheckEnd;
            SynapseController.Server.Events.Scp.Scp096.Scp096AddTargetEvent += On096Target;
        }

        public Dictionary<Type, string> CustomRoles = new Dictionary<Type, string>();

        public IRole GetCustomRole(string name) => (IRole)Activator.CreateInstance(CustomRoles.Keys.FirstOrDefault(x => x.Name.ToLower() == name));

        public void RegisterCustomRole<TRole>(string rolename) where TRole : IRole => CustomRoles.Add(typeof(TRole), rolename);

        #region Events
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
                //TODO: Translation
                //ev.Killer.InstantBroadcast(3, plugin.Translation.GetTranslation("sameteam"));
            }

            if (ev.Killer.CustomRole != null && ev.Killer.CustomRole.GetFriends().Any(x => x == ev.Victim.RealTeam))
            {
                info.Amount = 0;
                //TODO: Translation
                //ev.Killer.InstantBroadcast(3, plugin.Translation.GetTranslation("sameteam"));
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

        private void CheckEnd(Events.SynapseEventArguments.RoundCheckEventArgs ev)
        {
            //TODO: implement this into CheckRoundEndPatch
            List<Team> teams = Server.Get.Players.Select(x => x.RealTeam).ToList();

            int teamamounts = 0;
            if (teams.Contains(Team.MTF)) teamamounts++;
            if (teams.Contains(Team.RSC)) teamamounts++;
            if (teams.Contains(Team.CHI)) teamamounts++;
            if (teams.Contains(Team.CDP)) teamamounts++;
            if (teams.Contains(Team.SCP)) teamamounts++;

            bool roundend = false;
            if (teamamounts < 2) roundend = true;
            if (teamamounts == 2)
            {
                if (teams.Contains(Team.CHI) && teams.Contains(Team.SCP))
                    roundend = Server.Get.Configs.SynapseConfiguration.ChaosScpEnd;

                if (teams.Contains(Team.CHI) && teams.Contains(Team.CDP))
                    roundend = true;

                if (teams.Contains(Team.MTF) && teams.Contains(Team.RSC))
                    roundend = true;
            }

            foreach (var role in Server.Get.GetPlayers(x => x.CustomRole != null).Select(x => x.CustomRole))
                if (role.GetEnemys().Any(t => teams.Contains(t)))
                    roundend = false;

            if (!roundend) ev.Allow = false;
            else
            {
                ev.ForceEnd = true;

                if (RoundSummary.escaped_ds + teams.Where(x => x == Team.CDP).Count() > 0)
                {
                    if (!teams.Contains(Team.SCP) && !teams.Contains(Team.CHI))
                        ev.Team = RoundSummary.LeadingTeam.Draw;
                    else
                        ev.Team = RoundSummary.LeadingTeam.ChaosInsurgency;
                }
                else
                {
                    if (teams.Contains(Team.MTF) || teams.Contains(Team.RSC))
                    {
                        if (RoundSummary.escaped_scientists + teams.Where(x => x == Team.RSC).Count() > 0)
                            ev.Team = RoundSummary.LeadingTeam.FacilityForces;
                        else
                            ev.Team = RoundSummary.LeadingTeam.Draw;
                    }
                    else ev.Team = RoundSummary.LeadingTeam.Anomalies;
                }
            }
        }
        #endregion
    }
}
