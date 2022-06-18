using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer : MonoBehaviour
{
    /// <summary>
    /// Returns true if this SynapsePlayer object is the Dedicated Server
    /// </summary>
    public virtual bool IsServer => false;

    internal SynapsePlayer()
    {
        Hub = GetComponent<ReferenceHub>();
        GameConsoleTransmission = GetComponent<GameConsoleTransmission>();
        DissonanceUserSetup = GetComponent<Assets._Scripts.Dissonance.DissonanceUserSetup>();
        Radio = GetComponent<Radio>();
        Escape = GetComponent<Escape>();
    }
}