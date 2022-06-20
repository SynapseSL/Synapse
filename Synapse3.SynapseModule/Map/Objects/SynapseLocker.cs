using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MapGeneration.Distributors;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseLocker : StructureSyncSynapseObject
{
    public static Dictionary<LockerType, Locker> Prefabs { get; } = new ();


    public Locker Locker { get; }
    public ReadOnlyCollection<SynapseLockerChamber> Chambers { get; private set; }
    public override GameObject GameObject => Locker.gameObject;
    public override NetworkIdentity NetworkIdentity => Locker.netIdentity;
    public override ObjectType Type => ObjectType.Locker;

    public override void OnDestroy()
    {
        Map._synapseLockers.Remove(this);
        Chambers = null;
        base.OnDestroy();
    }

    public SynapseLocker(LockerType lockerType, Vector3 position, Quaternion rotation, Vector3 scale,
        bool removeDefaultItems = false)
    {
        Locker = CreateLocker(lockerType, position, rotation, scale, removeDefaultItems);
        SetUp();
    }

    internal SynapseLocker(Locker locker)
    {
        Locker = locker;
        SetUp();
    }

    private void SetUp()
    {
        Map._synapseLockers.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        var list = new List<SynapseLockerChamber>();

        for (ushort i = 0; i < Locker.Chambers.Count(); i++)
            list.Add(new SynapseLockerChamber(Locker.Chambers[i], this, i));

        Chambers = list.AsReadOnly();
    }
    private Locker CreateLocker(LockerType lockerType, Vector3 position, Quaternion rotation, Vector3 scale,
        bool removeDefaultItems = false)
    {
        var locker = CreateNetworkObject(Prefabs[lockerType], position, rotation, scale);
        
        //TODO: Chambers

        return locker;
    }
    
    public enum LockerType
    {
        StandardLocker,
        LargeGunLocker,
        RifleRackLocker,
        ScpPedestal,
        MedkitWallCabinet,
        AdrenalineWallCabinet
    }
    
    public class SynapseLockerChamber
    {
        public SynapseLockerChamber(LockerChamber chamber, SynapseLocker locker, ushort id)
        {
            LockerChamber = chamber;
            Locker = locker;
            ColliderID = id;
            ByteID = (ushort)(1 << id);
        }
        
        
        public SynapseLocker Locker { get; }
        public LockerChamber LockerChamber { get; }
        public ushort ByteID { get; },
        public ushort ColliderID { get; }

        public GameObject GameObject => LockerChamber.gameObject;

        public string Name => GameObject.name;

        public bool CanInteract => LockerChamber.CanInteract;

        public Vector3 Position => GameObject.transform.position;

        public bool Open
        {
            get => (Locker.Locker.OpenedChambers & ByteID) == ByteID;
            set
            {
                LockerChamber.IsOpen = value;
                Locker.Locker.RefreshOpenedSyncvar();
                if (value)
                    Locker.Locker.OpenedChambers = (ushort)(Locker.Locker.OpenedChambers | ByteID);
                else
                    Locker.Locker.OpenedChambers = (ushort)(Locker.Locker.OpenedChambers & ~ByteID);
            }
        }

        public void SpawnItem(ItemType type, int amount = 1)
            => LockerChamber.SpawnItem(type, amount);
    }
}