using Mirror;
using RemoteAdmin;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Synapse.Api
{
    public class Dummy
    {
        private ItemType helditem;
        public GameObject GameObject { get; internal set; }

        public Player Player { get; internal set; }

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
            get => Player.Position;
            set => Player.Position = value;
        }

        public Vector2 Rotation
        {
            get => Player.Rotation;
            set => Player.Rotation = value;
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

        public PlayerMovementState Movement
        {
            get => (PlayerMovementState)Player.AnimationController.Network_curMoveState;
            set => Player.AnimationController.Network_curMoveState = (byte)value;
        }

        public float Speed { get; set; } = 0f;

        private IEnumerator<float> Update()
        {
            for(; ; )
            {
                yield return MEC.Timing.WaitForSeconds(0.1f);
                if (GameObject == null) yield break;
                if (Speed == 0f) continue;

                Player.AnimationController.Networkspeed = new Vector2(Speed, 0f);

                var pos = Position + Player.CameraReference.forward / 10 * Speed;
                
                if (!Physics.Linecast(Position, pos, Player.PlayerMovementSync.CollidableSurfaces))
                {
                    Player.PlayerMovementSync.OverridePosition(pos, 0f, true);
                }
                else
                {
                    Speed = 0f;
                    Player.AnimationController.Networkspeed = new Vector2(0f, 0f);
                }
            }
        }

        public Dummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "") : this(pos, new Vector2(rot.eulerAngles.x, rot.eulerAngles.y),role,name,badgetext,badgecolor) { }

        /// <summary>
        /// Creates a new Dummy and spawns it
        /// </summary>
        /// <param name="pos">The Position where the Dummy should spawn</param>
        /// <param name="rot">The Rotation of the Dummy</param>
        /// <param name="role">The Role which the Dummy should be</param>
        /// <param name="name">The Name of the Dummy</param>
        /// <param name="badgetext">The displayed BadgeText of the Dummy</param>
        /// <param name="badgecolor">The displayed BadgeColor of the Dummy</param>
        public Dummy(Vector3 pos, Vector2 rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "")
        {
            GameObject obj =
                Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));

            GameObject = obj;
            Player = GameObject.GetPlayer();
            Player.IsDummy = true;


            Player.transform.localScale = Vector3.one;
            Player.transform.position = pos;
            Player.PlayerMovementSync.RealModelPosition = pos;
            Rotation = rot;
            Player.QueryProcessor.NetworkPlayerId = QueryProcessor._idIterator;
            Player.QueryProcessor._ipAddress = Server.Get.Host.IpAddress;
            Player.ClassManager.CurClass = role;
            Player.Health = Player.ClassManager.Classes.SafeGet((int)Player.RoleType).maxHP;
            Player.NicknameSync.Network_myNickSync = name;
            Player.RankName = badgetext;
            Player.RankColor = badgecolor;
            Player.Health = 100f;
            Player.GodMode = true;
            MEC.Timing.RunCoroutine(Update());

            NetworkServer.Spawn(GameObject);
            Map.Get.Dummies.Add(this);
        }

        public void RotateToPosition(Vector3 pos)
        {
            var rot = Quaternion.LookRotation((pos - Position).normalized);
            Rotation = new Vector2(rot.eulerAngles.x, rot.eulerAngles.y);
        }

        /// <summary>
        /// Despawns the Dummy
        /// </summary>
        public void Despawn()
        {
            NetworkServer.UnSpawn(GameObject);
            Map.Get.Dummies.Remove(this);
        }

        /// <summary>
        /// Spawns the Dummy again after Despawning
        /// </summary>
        public void Spawn() => NetworkServer.Spawn(GameObject);

        /// <summary>
        /// Destroys the Object
        /// </summary>
        public void Destroy()
        {
            Object.Destroy(GameObject);
            Map.Get.Dummies.Remove(this);
        }

        public static Dummy CreateDummy(Vector3 pos, Quaternion rot, RoleType role = RoleType.ClassD, string name = "(null)", string badgetext = "", string badgecolor = "")
            => new Dummy(pos, rot, role, name, badgetext, badgecolor);
    }
}
