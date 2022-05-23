﻿using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class RoundCheckEventArgs : EventHandler.ISynapseEventArgs
    {
        public bool EndRound { get; set; }

        public RoundSummary.LeadingTeam Team { get; set; }
    }

    public class SpawnPlayersEventArgs : EventHandler.ISynapseEventArgs
    {
        /// <summary>
        /// Determines which Player has which RoleID
        /// </summary>
        public Dictionary<Player, int> SpawnPlayers { get; set; }

        public bool Allow { get; set; } = true;
    }

    public class WarheadDetonationCanceledEventArgs : EventHandler.ISynapseEventArgs
    {
        public GameObject Disabler { get; set; }

        public bool Allow { get; set; } = true;
    }

    public class TeamRespawnEventArgs : EventHandler.ISynapseEventArgs
    {
        private int team;

        public Respawning.SpawnableTeamType Team
        {
            get
            {
                return team switch
                {
                    1 => Respawning.SpawnableTeamType.NineTailedFox,
                    2 => Respawning.SpawnableTeamType.ChaosInsurgency,
                    _ => Respawning.SpawnableTeamType.None,
                };
            }
            set
            {
                team = value switch
                {
                    Respawning.SpawnableTeamType.NineTailedFox => 1,
                    Respawning.SpawnableTeamType.ChaosInsurgency => 2,
                    _ => -1,
                };
            }
        }

        public int TeamID { get => team; set => team = value; }

        public List<Player> Players { get; set; }

        public bool Allow { get; set; } = true;
    }
}