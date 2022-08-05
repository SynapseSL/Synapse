using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Meta;
using Respawning;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map;

public class CassieService : Service
{
    private PlayerService _player;

    public CassieService(PlayerService player)
    {
        _player = player;
    }
    
    public void Announce(string message)
    {
        RespawnEffectsController.PlayCassieAnnouncement(message, true, false, true);
    }

    public void Announce(string message, params CassieSettings[] settings)
        => Announce(message, 0.3f, 0.2f, settings);

    public void Announce(string message, float glitchChance, float jamChance, params CassieSettings[] settings)
    {
        if (settings.Contains(CassieSettings.Glitched))
        {
            var oldWords = message.Split(' ');
            var newWords = new List<string>();
            
            foreach (var word in oldWords)
            {
                newWords.Add(word);
                
                if (Random.value < glitchChance)
                {
                    newWords.Add(".G" + Random.Range(1, 7));
                }

                if (Random.value < jamChance)
                {
                    newWords.Add($"JAM_{Random.Range(0, 70):000}_{Random.Range(2, 6)}");
                }
            }

            message = string.Empty;
            foreach (var word in newWords)
            {
                message += word + " ";
            }
        }
        
        RespawnEffectsController.PlayCassieAnnouncement(message, settings.Contains(CassieSettings.Break),
            settings.Contains(CassieSettings.Noise), settings.Contains(CassieSettings.DisplayText));
    }

    public void AnnounceScpDeath(string scp, params CassieSettings[] settings)
        => AnnounceScpDeath(scp, ScpContainmentType.Unknown, "Unknown", 0.3f, 0.2f, settings);
    
    public void AnnounceScpDeath(string scp, ScpContainmentType type, string unit = "Unknown",
        float glitchChance = 0.3f, float jamChance = 0.2f, params CassieSettings[] settings)
    {
        var chars = scp.ToArray();
        scp = string.Empty;
        foreach (var key in chars)
        {
            scp += key + " ";
        }

        var message = type switch
        {
            ScpContainmentType.Tesla => $". SCP {scp} SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM",
            ScpContainmentType.Nuke => $". SCP {scp} SUCCESSFULLY TERMINATED BY ALPHA WARHEAD",
            ScpContainmentType.Decontamination => $". SCP {scp} LOST IN DECONTAMINATION SEQUENCE",
            ScpContainmentType.Mtf => $". SCP {scp} SUCCESSFULLY TERMINATED . CONTAINEDSUCCESSFULLY CONTAINMENTUNIT {unit}",
            ScpContainmentType.Chaos => $". SCP {scp} CONTAINEDSUCCESSFULLY BY CHAOSINSURGENCY",
            ScpContainmentType.Scientist => $". SCP {scp} CONTAINEDSUCCESSFULLY BY SCIENCE PERSONNEL",
            ScpContainmentType.ClassD => $". SCP {scp} CONTAINEDSUCCESSFULLY BY CLASSD PERSONNEL",
            ScpContainmentType.Scp => $"TERMINATED BY SCP {unit}",
            ScpContainmentType.Unknown => $". SCP {scp} SUCCESSFULLY TERMINATED . CONTAINMENTUNIT UNKNOWN",
            _ => $". SCP {scp} SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED",
        };

        Announce(message, glitchChance, jamChance, settings);
    }

    public void Broadcast(ushort time, string message)
    {
        foreach (var player in _player.Players)
        {
            player.SendBroadcast(message, time);
        }
    }
}