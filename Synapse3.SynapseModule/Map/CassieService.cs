using System.Collections.Generic;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Respawning;
using Synapse3.SynapseModule.Enums;
using UnityEngine;

namespace Synapse3.SynapseModule.Map;

public class CassieService : Service
{
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
}