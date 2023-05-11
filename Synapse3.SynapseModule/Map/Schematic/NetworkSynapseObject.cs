using Mirror;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class NetworkSynapseObject :  DefaultSynapseObject, IRefreshable, IHideable
{
    public abstract NetworkIdentity NetworkIdentity { get; }

    protected abstract NetworkBehaviour NetworkObject { get; }

    public virtual void Refresh() => NetworkIdentity.UpdatePositionRotationScale();
    public bool Update { get; set; } = false;
    public float UpdateFrequency { get; set; } = 0;

    public override Vector3 Position
    {
        get => base.Position;
        set
        {
            NetworkIdentity.transform.position = value;
            Refresh();
        }
    }

    public override Quaternion Rotation
    {
        get => base.Rotation;
        set
        {
            NetworkIdentity.transform.rotation = value;
            Refresh();
        }
    }

    public override Vector3 Scale
    {
        get => base.Scale;
        set
        {
            NetworkIdentity.transform.localScale = value;
            Refresh();
        }
    }

    protected virtual TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale) where TComponent : NetworkBehaviour
    {
        var gameObject = Object.Instantiate(component, pos, rot);
        gameObject.transform.localScale = scale;
        NetworkServer.Spawn(gameObject.gameObject);
        return gameObject;
    }

    public void HideFromAll()
    {
        //Update All var
        NetworkObject.syncVarDirtyBits = ~(0uL);
        NetworkIdentity.UnSpawnForAllPlayers();
    }

    public void ShowAll()
    {
        //Update All var
        NetworkObject.syncVarDirtyBits = ~(0uL);
        Refresh();
    }
    public void HideFromPlayer(SynapsePlayer player) => NetworkIdentity.UnSpawnForOnePlayer(player);

    public void ShowPlayer(SynapsePlayer player) => NetworkIdentity.SpawnForOnePlayer(player);
}