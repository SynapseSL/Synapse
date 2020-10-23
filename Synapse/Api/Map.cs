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

        public int Seed => RandomSeedSync.staticSeed;

        public Dummy CreateDummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "") 
            => new Dummy(pos, rot, role, name, badgetext, badgecolor);

        public WorkStation CreateWorkStation(Vector3 position, Vector3 rotation, Vector3 scale) => new WorkStation(position, rotation, scale);

        public Ragdoll CreateRagdoll(RoleType roletype, Vector3 pos, Quaternion rot, Vector3 velocity, PlayerStats.HitInfo info, bool allowRecall, Player owner) => new Ragdoll(roletype, pos, rot, velocity, info, allowRecall, owner);

        public void SendBroadcast(ushort time,string message,bool instant)
        {
            foreach (var ply in Server.Get.Players)
                ply.SendBroadcast(time, message, instant);
        }

        public void AnnounceScpDeath(string scp)
        {
            float num2 = (AlphaWarheadController.Host.timeToDetonation <= 0f) ? 3.5f : 1f;
            var text = $"SCP {scp} SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";

            Server.Get.GetObjectOf<NineTailedFoxAnnouncer>().ServerOnlyAddGlitchyPhrase(text, UnityEngine.Random.Range(0.1f, 0.14f) * num2, UnityEngine.Random.Range(0.07f, 0.08f) * num2);
        }

        internal void AddObjects()
        {
            foreach (var tesla in SynapseController.Server.GetObjectsOf<TeslaGate>())
                SynapseController.Server.Map.Teslas.Add(new Tesla(tesla));

            foreach (var room in SynapseController.Server.GetObjectsOf<Transform>().Where(x => x.CompareTag("Room") || x.name == "Root_*&*Outside Cams" || x.name == "PocketWorld" || x.name == "Start Positions"))
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
