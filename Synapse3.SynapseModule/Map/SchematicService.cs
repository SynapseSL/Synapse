using Interactables.Interobjects;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;

namespace Synapse3.SynapseModule.Map;

public class SchematicService : Service
{
    private RoundEvents _round;

    public SchematicService(RoundEvents round)
    {
        _round = round;
    }
    
    public override void Enable()
    {
        _round.RoundWaiting.Subscribe(LateInit);
        
        foreach (var prefab in NetworkManager.singleton.spawnPrefabs)
        {
            switch (prefab.name)
            {
                case "EZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                    SynapseDoor.Prefab[SynapseDoor.SpawnableDoorType.EZ] = door;
                    break;

                case "HCZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                    SynapseDoor.Prefab[SynapseDoor.SpawnableDoorType.HCZ] = door;
                    break;

                case "LCZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                    SynapseDoor.Prefab[SynapseDoor.SpawnableDoorType.LCZ] = door;
                    break;
            }
        }
    }

    private void LateInit(RoundWaitingEvent ev)
    {
        if(!ev.FirstTime) return;

        foreach (var prefab in NetworkClient.prefabs)
        {
            switch (prefab.Key.ToString())
            {
                case "daf3ccde-4392-c0e4-882d-b7002185c6b8" when prefab.Value.TryGetComponent<Scp079Generator>(out var gen):
                    SynapseGenerator.GeneratorPrefab = gen;
                    break;
            }
        }
    }
}