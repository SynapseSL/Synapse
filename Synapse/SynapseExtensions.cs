using Synapse.Api.Components;
using UnityEngine;

public static class SynapseExtensions
{
    public static Player GetPlayer(this MonoBehaviour mono) => mono.gameObject.GetComponent<Player>();
}