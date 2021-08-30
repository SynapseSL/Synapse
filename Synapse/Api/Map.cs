using System;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration;
using Mirror;
using Scp914;
using Synapse.Api.Items;
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

        public void AnnounceScpDeath(string scp)
        {
            var text = $"SCP {scp} SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
            GlitchedCassie(text);
        }

        public void AnnounceScpDeath(string scp, PlayerStats.HitInfo hitInfo)
        {

            Player player = Server.Get.Players.FirstOrDefault(p => p.PlayerId == hitInfo.PlayerId);
            DamageTypes.DamageType damageType = hitInfo.GetDamageType();
            if (damageType == DamageTypes.Tesla)
                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase($". SCP {scp} SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM", 0, 0);
            else if (damageType == DamageTypes.Nuke)
                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase($". SCP {scp} SUCCESSFULLY TERMINATED BY ALPHA WARHEAD", 0, 0);
            else if (damageType == DamageTypes.Decont)
                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase($". SCP {scp} LOST IN DECONTAMINATION SEQUENCE", 0, 0);
            else
            {
                string cause = "TERMINATION CAUSE UNSPECIFIED";
                if (player != null)
                {

                    switch (player.TeamID)
                    {
                        case (int)Team.MTF:
                            Respawning.NamingRules.UnitNamingRule rule;
                            string Unit = Respawning.NamingRules.UnitNamingRules.TryGetNamingRule(Respawning.SpawnableTeamType.NineTailedFox, out rule) ? rule.GetCassieUnitName(player.UnitName) : "UNKNOWN";
                            cause = "CONTAINEDSUCCESSFULLY CONTAINMENTUNIT " + Unit;
                            break;
                        case (int)Team.CHI:
                            cause = "BY CHAOSINSURGENCY";
                            break;
                        case (int)Team.RSC:
                            cause = "BY FACILITY PERSONNEL";
                            break;
                        case (int)Team.CDP:
                            cause = " BY CLASSD PERSONNEL";
                            break;
                        default:
                            cause = "CONTAINMENTUNIT UNKNOWN";
                            break;
                    }
                }
                GlitchedCassie($". SCP {scp} SUCCESSFULLY TERMINATED . {cause}");
            }

        }

        public void Cassie(string words, bool makehold = true, bool makenoise = true)
            => Respawning.RespawnEffectsController.PlayCassieAnnouncement(words, makehold, makenoise);

        public void GlitchedCassie(string words)
        {
            float num2 = (AlphaWarheadController.Host.timeToDetonation <= 0f) ? 3.5f : 1f;
            Server.Get.GetObjectOf<NineTailedFoxAnnouncer>().ServerOnlyAddGlitchyPhrase(words, UnityEngine.Random.Range(0.1f, 0.14f) * num2, UnityEngine.Random.Range(0.07f, 0.08f) * num2);
        }

        public void SpawnGrenade(Vector3 position, Vector3 velocity, float fusetime = 3f, Enum.GrenadeType grenadeType = Enum.GrenadeType.Grenade, Player player = null)
        {
            //TODO: Reimplement this
        }

        public void Explode(Vector3 position, Enum.GrenadeType grenadeType = Enum.GrenadeType.Grenade, Player player = null)
        {
            //TODO: Reimplement this
        }

        public void PlaceBlood(Vector3 pos, int type = 0, float size = 2)
            => Server.Get.Host.ClassManager.RpcPlaceBlood(pos, type, size);

        [Obsolete("Instantiate a Dummy instead", true)]
        public Dummy CreateDummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "")
            => new Dummy(pos, rot, role, name, badgetext, badgecolor);

        [Obsolete("Moved to Workstation.CreateWorkStation()", true)]
        public WorkStation CreateWorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
            => new WorkStation(position, rotation, scale);

        [Obsolete("Moved to Ragdoll.CreateRagdoll()", true)]
        public Ragdoll CreateRagdoll(RoleType roletype, Vector3 pos, Quaternion rot, Vector3 velocity, PlayerStats.HitInfo info, bool allowRecall, Player owner)
            => new Ragdoll(roletype, pos, rot, velocity, info, allowRecall, owner);

        [Obsolete("Moved to Door.SpawnDoorVariant()", true)]
        public Door SpawnDoorVariant(Vector3 position, Quaternion? rotation = null, DoorPermissions permissions = null)
        {
            DoorVariant doorVariant = UnityEngine.Object.Instantiate(Server.Get.Prefabs.DoorVariantPrefab);

            doorVariant.transform.position = position;
            doorVariant.transform.rotation = rotation ?? new Quaternion(0, 0, 0, 0);
            doorVariant.RequiredPermissions = permissions ?? new DoorPermissions();
            var door = new Door(doorVariant);
            Get.Doors.Add(door);
            NetworkServer.Spawn(doorVariant.gameObject);

            return door;
        }

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
        }

        internal void ClearObjects()
        {
            Teslas.Clear();
            Doors.Clear();
            Elevators.Clear();
            Rooms.Clear();
            Generators.Clear();
            WorkStations.Clear();
            Ragdolls.Clear();
            SynapseItem.AllItems.Clear();
            ItemSerialGenerator.Reset();
        }
    }
}
