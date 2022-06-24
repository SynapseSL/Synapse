using AdminToys;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms.Attachments;
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
                
                case "PrimitiveObjectToy" when prefab.TryGetComponent<PrimitiveObjectToy>(out var pref):
                    SynapsePrimitive.Prefab = pref;
                    break;

                case "LightSourceToy" when prefab.TryGetComponent<LightSourceToy>(out var lightpref):
                    SynapseLight.Prefab = lightpref;
                    break;

                case "sportTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                    SynapseTarget.Prefabs[SynapseTarget.TargetType.Sport] = target;
                    break;

                case "dboyTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                    SynapseTarget.Prefabs[SynapseTarget.TargetType.DBoy] = target;
                    break;

                case "binaryTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                    SynapseTarget.Prefabs[SynapseTarget.TargetType.Binary] = target;
                    break;
                
                case "Work Station" when prefab.TryGetComponent<WorkstationController>(out var station):
                    SynapseWorkStation.Prefab = station;
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
                
                case "68f13209-e652-6024-2b89-0f75fb88a998" when prefab.Value.TryGetComponent<Locker>(out var locker):

                    SynapseLocker.Prefabs[SynapseLocker.LockerType.ScpPedestal] = locker;
                    break;

                case "5ad5dc6d-7bc5-3154-8b1a-3598b96e0d5b" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.LargeGunLocker] = locker;
                    break;

                case "850f84ad-e273-1824-8885-11ae5e01e2f4" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.RifleRackLocker] = locker;
                    break;

                case "d54bead1-286f-3004-facd-74482a872ad8" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.StandardLocker] = locker;
                    break;

                case "5b227bd2-1ed2-8fc4-2aa1-4856d7cb7472" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.MedkitWallCabinet] = locker;
                    break;

                case "db602577-8d4f-97b4-890b-8c893bfcd553" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.AdrenalineWallCabinet] = locker;
                    break;
            }
        }
        
        foreach (var role in CharacterClassManager._staticClasses)
            if (role != null)
                SynapseRagdoll.Prefabs[role.roleId] = role.model_ragdoll?.GetComponent<Ragdoll>();
    }
}