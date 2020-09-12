using Synapse.Api;
using UnityEngine;

public static class SynapseExtensions
{
    public static Player GetPlayer(this MonoBehaviour mono) => mono.gameObject.GetComponent<Player>();

    public static Player GetPlayer(this GameObject gameObject) => gameObject.GetComponent<Player>();
}