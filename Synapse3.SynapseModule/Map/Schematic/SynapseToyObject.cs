using AdminToys;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class SynapseToyObject<TToy> :
    DefaultSynapseObject,
    IHideable
    where TToy : AdminToyBase
{
    public abstract TToy ToyBase { get; }

    public override GameObject GameObject => ToyBase.gameObject;

    public override Vector3 Scale
    {
        get => GameObject.transform.localScale;
        set
        {
            ToyBase.transform.localScale = value;
            ToyBase.NetworkScale = value;
        }
    }

    public void HideFromAll() => ToyBase.netIdentity.UnSpawnForAllPlayers();

    public void ShowAll()
    {
        //Update all var
        ToyBase.syncVarDirtyBits = ~(0uL); 
        ToyBase.netIdentity.SpawnForAllPlayers();
    }

    public void HideFromPlayer(SynapsePlayer player) => ToyBase.netIdentity.UnSpawnForOnePlayer(player);

    public void ShowPlayer(SynapsePlayer player)
    {
        //Update all var
        ToyBase.syncVarDirtyBits = ~(0uL);
        ToyBase.netIdentity.SpawnForOnePlayer(player);
    }
}