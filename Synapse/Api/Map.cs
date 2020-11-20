using Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
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

        public List<Tesla> Teslas { get; } = new List<Tesla>();

        public List<Elevator> Elevators { get; } = new List<Elevator>();

        public List<Door> Doors { get; } = new List<Door>();

        public List<Room> Rooms { get; } = new List<Room>();

        public List<Generator> Generators { get; } = new List<Generator>();

        public List<WorkStation> WorkStations { get; } = new List<WorkStation>();

        public List<Ragdoll> Ragdolls { get; } = new List<Ragdoll>();

        public List<Items.SynapseItem> Items { get; } = new List<Items.SynapseItem>();

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

        public int Seed => RandomSeedSync.staticSeed;

        public Dummy CreateDummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "") 
            => new Dummy(pos, rot, role, name, badgetext, badgecolor);

        public WorkStation CreateWorkStation(Vector3 position, Vector3 rotation, Vector3 scale) => new WorkStation(position, rotation, scale);

        public Ragdoll CreateRagdoll(RoleType roletype, Vector3 pos, Quaternion rot, Vector3 velocity, PlayerStats.HitInfo info, bool allowRecall, Player owner) => new Ragdoll(roletype, pos, rot, velocity, info, allowRecall, owner);

        public void SendBroadcast(ushort time,string message,bool instant = false)
        {
            foreach (var ply in Server.Get.Players)
                ply.SendBroadcast(time, message, instant);
        }

        public void AnnounceScpDeath(string scp)
        {
            var text = $"SCP {scp} SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
            GlitchedCassie(text);
        }

        public void Cassie(string words, bool makehold = true, bool makenoise = true) => Respawning.RespawnEffectsController.PlayCassieAnnouncement(words, makehold, makenoise);

        public void GlitchedCassie(string words)
        {
            float num2 = (AlphaWarheadController.Host.timeToDetonation <= 0f) ? 3.5f : 1f;
            Server.Get.GetObjectOf<NineTailedFoxAnnouncer>().ServerOnlyAddGlitchyPhrase(words, UnityEngine.Random.Range(0.1f, 0.14f) * num2, UnityEngine.Random.Range(0.07f, 0.08f) * num2);
        }

        public void Explode(Vector3 position) 
        {
            var component = Server.Get.Host.GetComponent<Grenades.GrenadeManager>();
            var component2 = Object.Instantiate(component.availableGrenades[0].grenadeInstance).GetComponent<Grenades.Grenade>();
            component2.FullInitData(component, position, Quaternion.identity, Vector3.zero, Vector3.zero,Team.RIP);
            component2.NetworkfuseTime = 0.10000000149011612;
            NetworkServer.Spawn(component2.gameObject);
        }

        internal void AddObjects()
        {
            foreach (var tesla in SynapseController.Server.GetObjectsOf<TeslaGate>())
                SynapseController.Server.Map.Teslas.Add(new Tesla(tesla));

            foreach (var room in SynapseController.Server.GetObjectsOf<Transform>().Where(x => x.CompareTag("Room") || x.name == "Root_*&*Outside Cams" || x.name == "PocketWorld"))
                Rooms.Add(new Room(room.gameObject));

            foreach (var station in Server.Get.GetObjectsOf<global::WorkStation>())
                WorkStations.Add(new WorkStation(station));
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
            Items.Clear();
        }
    }
}
