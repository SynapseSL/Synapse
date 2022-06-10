using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration;
using Mirror;
using Scp914;
using Synapse.Api.CustomObjects;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Map
    {
        internal Map() { }

        public static Map Get => Server.Get.Map;

        public Nuke Nuke { get; } = new Nuke();

        public Round Round { get; } = new Round();

        public Decontamination Decontamination { get; } = new Decontamination();

        public Scp914 Scp914 { get; } = new Scp914();

        public HeavyController HeavyController { get; } = new HeavyController();

        public List<Tesla> Teslas { get; } = new List<Tesla>();

        public List<Elevator> Elevators { get; } = new List<Elevator>();

        public List<Door> Doors { get; } = new List<Door>();

        public List<Room> Rooms { get; } = new List<Room>();

        public List<Generator> Generators { get; } = new List<Generator>();

        public List<WorkStation> WorkStations { get; } = new List<WorkStation>();

        public List<Ragdoll> Ragdolls { get; } = new List<Ragdoll>();

        public List<SynapseItem> Items => SynapseItem.AllItems.Values.Where(x => x != null).ToList();

        public List<Dummy> Dummies { get; } = new List<Dummy>();

        public List<Camera> Cameras { get; } = new List<Camera>();

        public List<Locker> Lockers { get; } = new List<Locker>();

        public List<ISynapseObject> SynapseObjects { get; } = new List<ISynapseObject>();

        public string IntercomText
        {
            get => Server.Get.Host.GetComponent<Intercom>().CustomContent;
            set
            {
                var component = Server.Get.Host.GetComponent<Intercom>();
                if (string.IsNullOrEmpty(value))
                {
                    component.CustomContent = null;
                    return;
                }

                component.CustomContent = value;
            }
        }

        public Vector3 RespawnPoint
        {
            get => NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
            set => NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint = value;
        }

        public float WalkSpeed
        {
            get => ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier;
            set => ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier = value;
        }

        public float SprintSpeed
        {
            get => ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier;
            set => ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier = value;
        }

        public int Seed => MapGeneration.SeedSynchronizer.Seed;

        public Room GetRoom(RoomName roomType)
            => Rooms.FirstOrDefault(x => x.RoomType == roomType);

        public Door GetDoor(Enum.DoorType doorType)
            => Doors.FirstOrDefault(x => x.DoorType == doorType);

        public Elevator GetElevator(Enum.ElevatorType elevatorType)
            => Elevators.FirstOrDefault(x => x.ElevatorType == elevatorType);

        public void SendBroadcast(ushort time, string message, bool instant = false)
        {
            foreach (var ply in Server.Get.Players)
                ply.SendBroadcast(time, message, instant);
        }

        public void AnnounceScpDeath(string scp) => AnnounceScpDeath(scp, ScpRecontainmentType.Unknown);

        public void AnnounceScpDeath(string scp, ScpRecontainmentType deathType, string Unit = "UNKNOWN") =>
        GlitchedCassie(deathType switch
        {
            ScpRecontainmentType.Tesla => $". SCP {scp} SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM",
            ScpRecontainmentType.Nuke => $". SCP {scp} SUCCESSFULLY TERMINATED BY ALPHA WARHEAD",
            ScpRecontainmentType.Decontamination => $". SCP {scp} LOST IN DECONTAMINATION SEQUENCE",
            ScpRecontainmentType.Mtf => $". SCP {scp} SUCCESSFULLY TERMINATED . CONTAINEDSUCCESSFULLY CONTAINMENTUNIT {Unit}",
            ScpRecontainmentType.Chaos => $". SCP {scp} SUCCESSFULLY TERMINATED . BY CHAOSINSURGENCY",
            ScpRecontainmentType.Scientist => $". SCP {scp} SUCCESSFULLY TERMINATED . BY SCIENCE PERSONNEL",
            ScpRecontainmentType.ClassD => $". SCP {scp} SUCCESSFULLY TERMINATED . BY CLASSD PERSONNEL",
            ScpRecontainmentType.Unknown => $". SCP {scp} SUCCESSFULLY TERMINATED . CONTAINMENTUNIT UNKNOWN",
            _ => $". SCP {scp} SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED",
        });

        public void Cassie(string words, bool makehold = true, bool makenoise = true)
            => Respawning.RespawnEffectsController.PlayCassieAnnouncement(words, makehold, makenoise);

        public void GlitchedCassie(string words)
        {
            float num2 = (AlphaWarheadController.Host.timeToDetonation <= 0f) ? 3.5f : 1f;
            Server.Get.GetObjectOf<NineTailedFoxAnnouncer>().ServerOnlyAddGlitchyPhrase(words, UnityEngine.Random.Range(0.1f, 0.14f) * num2, UnityEngine.Random.Range(0.07f, 0.08f) * num2);
        }

        public SynapseItem SpawnGrenade(Vector3 position, Vector3 velocity, float fusetime = 3f, Enum.GrenadeType grenadeType = Enum.GrenadeType.Grenade, Player player = null)
        {
            var itemtype = (ItemType)grenadeType;
            var grenadeitem = new SynapseItem(itemtype, position);
            grenadeitem.Throwable.Fuse();
            grenadeitem.Throwable.FuseTime = fusetime;

            if (player == null) player = Server.Get.Host;
            grenadeitem.Throwable.ThrowableItem.PreviousOwner = new Footprinting.Footprint(player.Hub);

            if(grenadeitem.Throwable.ThrowableItem.TryGetComponent<Rigidbody>(out var rgb))
                rgb.velocity = velocity;

            return grenadeitem;
        }

        public NetworkIdentity SpawnOldGrenade(Vector3 position, Quaternion rotation, bool flash = false)
        {
            var prefab = flash ? NetworkClient.prefabs[Guid.Parse("c69da0e5-a829-6a04-c8d9-f404a1073cfe")] : NetworkClient.prefabs[Guid.Parse("8063e113-c1f1-1514-7bc5-840ea8ee5f01")];
            var gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
            NetworkServer.Spawn(gameObject.gameObject);
            return gameObject.GetComponent<NetworkIdentity>();
        }

        public GameObject SpawnTantrum(Vector3 position, float destroy = -1)
        {
            var prefab = NetworkClient.prefabs[Guid.Parse("a0e7ee93-b802-e5a4-38bd-95e27cc133ea")];
            var gameObject = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            NetworkServer.Spawn(gameObject.gameObject);

            if (destroy >= 0)
                MEC.Timing.CallDelayed(destroy,() => NetworkServer.Destroy(gameObject));

            return gameObject;
        }

        public void Explode(Vector3 position, Enum.GrenadeType grenadeType = Enum.GrenadeType.Grenade, Player player = null)
        {
            var itemtype = (ItemType)grenadeType;
            var grenadeitem = new SynapseItem(itemtype, position);
            grenadeitem.Throwable.Fuse();
            if (player != null)
                grenadeitem.Throwable.ThrowableItem.PreviousOwner = new Footprinting.Footprint(player.Hub);
            MEC.Timing.CallDelayed(0.1f, () => grenadeitem.Destroy());
        }

        public void PlaceBlood(Vector3 pos, int type = 0, float size = 2)
            => Server.Get.Host.ClassManager.RpcPlaceBlood(pos, type, size);

        [Obsolete("Instantiate a Dummy instead", true)]
        public Dummy CreateDummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "")
            => new Dummy(pos, rot, role, name, badgetext, badgecolor);

        [Obsolete("Moved to Workstation.CreateWorkStation()", true)]
        public WorkStation CreateWorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
            => new WorkStation(position, rotation, scale);

        //[Obsolete("Moved to Ragdoll.CreateRagdoll()", true)]
        //public Ragdoll CreateRagdoll(RoleType roletype, Vector3 pos, Quaternion rot, DamageHandlerBase handler, Player owner) 
        //    => new Ragdoll(roletype, pos, rot, handler, owner);

        [Obsolete("Moved to Door.SpawnDoorVariant()", true)]
        public Door SpawnDoorVariant(Vector3 position, Quaternion? rotation = null, DoorPermissions permissions = null)
            => Door.SpawnDoorVariant(position, rotation, permissions);

        internal void AddObjects()
        {
            foreach (var room in RoomIdentifier.AllRoomIdentifiers)
            {
                var synRoom = new Room(room);
                Rooms.Add(synRoom);
                Cameras.AddRange(synRoom.Cameras);
            }

            foreach (var tesla in SynapseController.Server.GetObjectsOf<TeslaGate>())
                Teslas.Add(new Tesla(tesla));

            foreach (var station in WorkstationController.AllWorkstations)
                WorkStations.Add(new WorkStation(station));

            foreach (var door in SynapseController.Server.GetObjectsOf<DoorVariant>())
                Doors.Add(new Door(door));

            foreach (var pair in Scp079Interactable.InteractablesByRoomId)
            {
                foreach (var interactable in pair.Value)
                {
                    try
                    {
                        var room = Rooms.FirstOrDefault(x => x.ID == pair.Key);
                        var door = interactable.GetComponentInParent<DoorVariant>();
                        if (room == null || door == null) continue;
                        var sdoor = door.GetDoor();
                        sdoor.Rooms.Add(room);
                        room.Doors.Add(sdoor);
                    }
                    catch { }
                }
            }

            Scp914.Scp914Controller = UnityEngine.Object.FindObjectOfType<Scp914Controller>();

            SynapseController.Server.Map.Elevators.RemoveAll(x => x.GameObject == null);
        }

        internal void ClearObjects()
        {
            Room.networkIdentities = null;
            Teslas.Clear();
            Doors.Clear();
            Elevators.Clear();
            Rooms.Clear();
            Generators.Clear();
            Lockers.Clear();
            WorkStations.Clear();
            Ragdolls.Clear();
            SynapseObjects.Clear();
            SynapseItem.AllItems.Clear();
            ItemSerialGenerator.Reset();
        }
    }
}
