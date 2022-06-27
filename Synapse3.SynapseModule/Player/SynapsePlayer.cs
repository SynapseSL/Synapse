﻿using Synapse3.SynapseModule.Enums;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer : MonoBehaviour
{
    /// <summary>
    /// The Type of Player this is. It can be a normal Player the Server itself or a Dummy
    /// </summary>
    public virtual PlayerType PlayerType => PlayerType.Player;

    internal SynapsePlayer()
    {
        Hub = GetComponent<ReferenceHub>();
        GameConsoleTransmission = GetComponent<GameConsoleTransmission>();
        DissonanceUserSetup = GetComponent<Assets._Scripts.Dissonance.DissonanceUserSetup>();
        Radio = GetComponent<Radio>();
        Escape = GetComponent<Escape>();
        Inventory = new(this);
        ActiveBroadcasts = new(this);
    }
}