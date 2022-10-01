using AdminToys;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class SynapseToyObject<TToy> : DefaultSynapseObject where TToy : AdminToyBase
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

    public override void HideFromAll() => ToyBase.netIdentity.UnSpawnForAllPlayers();

    public override void ShowAll() => ToyBase.netIdentity.SpawnForAllPlayers();

    public override void HideFromPlayer(SynapsePlayer player) => ToyBase.netIdentity.UnSpawnForOnePlayer(player);

    public override void ShowPlayer(SynapsePlayer player) => ToyBase.netIdentity.SpawnForOnePlayer(player);
}