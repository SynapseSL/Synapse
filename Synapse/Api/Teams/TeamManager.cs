using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Synapse.Api.Teams
{
    public class TeamManager
    {
        public static TeamManager Get => Server.Get.TeamManager;

        internal TeamManager() { }

        private readonly List<ISynapseTeam> teams = new List<ISynapseTeam>();

        public void RegisterTeam<TTeam>() where TTeam : ISynapseTeam
        {
            var team = Activator.CreateInstance(typeof(TTeam)) as ISynapseTeam;
            if (team.Info == null)
                team.Info = typeof(TTeam).GetCustomAttribute<SynapseTeamInformation>();

            if (IsIDRegistered(team.Info.ID)) throw new Exception("A Plugin tried to register a CustomTeam with an already used Id");

            teams.Add(team);
            team.Initialise();
        }

        public bool IsIDRegistered(int id) => (id >= (int)Team.SCP && id <= (int)Team.TUT) || teams.Any(x => x.Info.ID == id);

        public bool IsDefaultID(int id) => id >= (int)Team.SCP && id <= (int)Team.TUT;

        public bool IsDefaultSpawnableID(int id) => id == (int)Team.MTF || id == (int)Team.CHI;

        public void SpawnTeam(int id, List<Player> players)
        {
            if (IsDefaultSpawnableID(id))
            {
                Round.Get.MtfRespawn(id == (int)Team.MTF);
                return;
            }
            var team = teams.FirstOrDefault(x => x.Info.ID == id);
            if (team == null) return;
            team.Spawn(players);
        }

        public ISynapseTeam GetTeam(int id) => teams.FirstOrDefault(x => x.Info.ID == id);
    }
}
