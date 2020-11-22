using Mirror;
using RemoteAdmin;
using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Dummy
    {
        private ItemType helditem;
        public GameObject GameObject { get; internal set; }

        /// <summary>
        /// Get / Set the current Role of the Dummy
        /// </summary>
        public RoleType Role
        {
            get => GameObject.GetComponent<CharacterClassManager>().CurClass;
            set
            {
                Despawn();
                GameObject.GetComponent<CharacterClassManager>().CurClass = value;
                Spawn();
            }
        }

        /// <summary>
        /// Get / Set the current Name of the Dummy
        /// </summary>
        public string Name
        {
            get => GameObject.GetComponent<NicknameSync>().Network_myNickSync;
            set => GameObject.GetComponent<NicknameSync>().Network_myNickSync = value;
        }

        /// <summary>
        /// Get / Set the current Position of the Dummy
        /// </summary>
        public Vector3 Position
        {
            get => GameObject.transform.position;
            set
            {
                Despawn();
                GameObject.transform.position = value;
                Spawn();
            }
        }

        public Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set
            {
                Despawn();
                GameObject.transform.rotation = value;
                Spawn();
            }
        }

        /// <summary>
        /// Get / Set the Scale of the Dummy
        /// </summary>
        public Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                Despawn();
                GameObject.transform.localScale = value;
                Spawn();
            }
        }

        /// <summary>
        /// Get / Set the current Item the Dummy is holding
        /// </summary>
        public ItemType HeldItem
        {
            get => helditem;
            set
            {
                GameObject.GetComponent<Inventory>().SetCurItem(value);
                helditem = value;
            }
        }

        /// <summary>
        /// Get / Set the BadgeText of the Dummy
        /// </summary>
        public string BadgeName
        {
            get => GameObject.GetComponent<ServerRoles>().MyText;
            set => GameObject.GetComponent<ServerRoles>().SetText(value);
        }

        /// <summary>
        /// Get / Set the BadgeColor of the Dummy
        /// </summary>
        public string BadgeColor
        {
            get => GameObject.GetComponent<ServerRoles>().MyColor;
            set => GameObject.GetComponent<ServerRoles>().SetColor(value);
        }

        /// <summary>
        /// Creates a new Dummy and spawns it
        /// </summary>
        /// <param name="pos">The Position where the Dummy should spawn</param>
        /// <param name="rot">The Rotation of the Dummy</param>
        /// <param name="role">The Role which the Dummy should be</param>
        /// <param name="name">The Name of the Dummy</param>
        /// <param name="badgetext">The displayed BadgeText of the Dummy</param>
        /// <param name="badgecolor">The displayed BadgeColor of the Dummy</param>
        public Dummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "")
        {
            GameObject obj =
                Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));

            if (obj.GetComponent<Player>() == null)
                obj.AddComponent<Player>();

            GameObject = obj;

            obj.GetComponent<CharacterClassManager>().CurClass = role;
            obj.GetComponent<NicknameSync>().Network_myNickSync = name;
            GameObject.GetComponent<ServerRoles>().MyText = badgetext;
            GameObject.GetComponent<ServerRoles>().MyColor = badgecolor;
            obj.transform.localScale = Vector3.one;
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
            obj.GetComponent<QueryProcessor>().PlayerId = 9999;

            NetworkServer.Spawn(obj);
            ReferenceHub.Hubs.Remove(obj);
        }

        public void RotateToPosition(Vector3 pos) => Rotation = Quaternion.LookRotation((pos - Position).normalized);

        /// <summary>
        /// Despawns the Dummy
        /// </summary>
        public void Despawn() => NetworkServer.UnSpawn(GameObject);

        /// <summary>
        /// Spawns the Dummy again after Despawning
        /// </summary>
        public void Spawn() => NetworkServer.Spawn(GameObject);

        /// <summary>
        /// Destroys the Object
        /// </summary>
        public void Destroy() => Object.Destroy(GameObject);
    }
}
