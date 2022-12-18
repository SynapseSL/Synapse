using System;
using System.Collections.Generic;
using InventorySystem.Items;
using InventorySystem.Items.MicroHID;
using MEC;
using Microsoft.Extensions.Logging;
using Mirror;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using RemoteAdmin;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Dummy;

public class SynapseDummy : DefaultSynapseObject, IRefreshable
{
    private readonly MapService _map;
    private readonly DummyService _dummy;
    private readonly PlayerService _player;
    public MovementDirection Direction { get; set; }

    public float SneakSpeed 
    { 
        get => Player.SneakSpeed;
        set => Player.SneakSpeed = value;
    }

    public float WalkSpeed 
    {
        get => Player.WalkSpeed;
        set => Player.WalkSpeed = value;
    }

    public float RunSpeed 
    {
        get => Player.RunSpeed;
        set => Player.RunSpeed = value;
    }

    public override GameObject GameObject { get; }
    public override ObjectType Type => ObjectType.Dummy;

    public override Vector3 Position
    {
        get => Player.Position;
        set => Player.Position = value;
    }

    public override Quaternion Rotation
    {
        get => Player.Rotation;
        set => Player.Rotation = value;
    }

    public Vector2 RotationVector2
    {
        get => Player.RotationVector2;
        set => Player.RotationVector2 = value;
    }

    public override Vector3 Scale
    {
        get => Player.Scale;
        set => Player.Scale = value;
    }

    public string Name
    {
        get => Player.NickName;
        set => Player.NicknameSync.Network_myNickSync = value;
    }

    public ItemType HeldItem
    {
        get => Player.VanillaInventory.NetworkCurItem.TypeId;
        set => Player.VanillaInventory.NetworkCurItem = new ItemIdentifier(value, 0);
    }
    
    public PlayerMovementState Movement
    {
        get => Player.AnimationController.MoveState;
        set
        {
            Player.AnimationController.MoveState = value;
            Player.AnimationController.RpcReceiveState((byte)value);
        }
    }

    public DummyPlayer Player { get; }
    
    public bool PlayerUpdate { get; set; }

    public SynapseDummy(Vector3 position, Quaternion rotation, RoleTypeId role, string name, string badge = "",
        string badgeColor = ""): this(position, role, name, badge, badgeColor)
    {
        PlayerUpdate = false;
        //TODO:
        Rotation = rotation;
        NetworkServer.Spawn(GameObject);
    }

    public SynapseDummy(Vector3 position, Vector2 rotation, RoleTypeId role, string name, string badge = "",
        string badgeColor = "") : this(position, role, name, badge, badgeColor)
    {
        PlayerUpdate = true;
        //TODO:
        RotationVector2 = rotation;
        NetworkServer.Spawn(GameObject);
    }

    private SynapseDummy(Vector3 position, RoleTypeId role, string name, string badge, string badgeColor)
    {
        _map = Synapse.Get<MapService>();
        _dummy = Synapse.Get<DummyService>();
        _player = Synapse.Get<PlayerService>();

        Player = Object.Instantiate(NetworkManager.singleton.playerPrefab, _dummy._dummyParent)
            .GetComponent<DummyPlayer>();
        GameObject = Player.gameObject;

        var hub = GameObject.GetComponent<ReferenceHub>();
        //hub.PlayerCameraReference = GameObject.AddComponent<Camera>().transform;//found better solution

        var comp = GameObject.AddComponent<SynapseObjectScript>();//found other solution
        comp.Object = this;

        Player.SynapseDummy = this;
        Player.transform.localScale = Vector3.one;
        Player.QueryProcessor._ipAddress = Synapse.Get<PlayerService>().Host.IpAddress;
        Player.RoleType = role;
        Player.Health = Player.MaxHealth;
        Player.NicknameSync.Network_myNickSync = name;
        Player.RankName = badge;
        Player.RankColor = badgeColor;
        Player.Position = position;


        if (Player.ExistsInSpace)
        {
            Player.FirstPersonMovement.IsGrounded = true;
        }

        _ = Timing.RunCoroutine(UpdateMovement()); 
        
        MoveInElevator = true;
    }

    internal SynapseDummy(SchematicConfiguration.DummyConfiguration configuration, SynapseSchematic schematic) :
        this(configuration.Position, configuration.Rotation,
            configuration.Role, configuration.Name,
            configuration.Badge, configuration.BadgeColor)
    {
        Parent = schematic;
        schematic._dummies.Add(this);
        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }

    public void Refresh()
    {
        Position = base.Position;
        Rotation = base.Rotation;
        Scale = base.Scale;
    }

    public bool Update { get; set; } = false;

    public float UpdateFrequency { get; set; }

    public void RotateToPosition(Vector3 pos)
    {
        Rotation = Quaternion.LookRotation((pos - GameObject.transform.position).normalized);
    }

    public void DeSpawn()
        => NetworkServer.UnSpawn(GameObject);

    public void Spawn()
        => NetworkServer.Spawn(GameObject);

    private Vector3 GetMoveVector() //TODO: use patch and the use the local player code
    {
        if (Player.ExistsInSpace)
        {
            float RgtLft = 0.0f;
            float FwdBwk = 0.0f;
            if ((Direction & MovementDirection.Forward) != 0)
                ++FwdBwk;
            if ((Direction & MovementDirection.BackWards) != 0)
                --FwdBwk;
            if ((Direction & MovementDirection.Right) != 0)
                ++RgtLft;
            if ((Direction & MovementDirection.Left) != 0)
                --RgtLft;
            return Player.FirstPersonMovement.Motor.CachedTransform.forward * FwdBwk + Player.FirstPersonMovement.Motor.CachedTransform.right * RgtLft;
        }
        return Vector3.zero;
    }



    //Thanks to GameHunt.I used some of his code for the Dummy API https://github.com/gamehunt/CustomNPCs
    private IEnumerator<float> UpdateMovement()
    {
        for (;;)
        {
            yield return Timing.WaitForSeconds(0.1f);

            try
            {
                if (GameObject == null) yield break;
                if (Direction == MovementDirection.None)
                {
                    continue;
                }

                var wall = false;
                var speed = 0f;

                switch (Movement)
                {
                    case PlayerMovementState.Sneaking:
                        speed = SneakSpeed;
                        break;

                    case PlayerMovementState.Sprinting:
                        speed = RunSpeed;
                        break;

                    case PlayerMovementState.Walking:
                        speed = WalkSpeed;
                        break;
                        
                }

                switch (Direction)
                {
                    
                    case MovementDirection.Forward:
                        var pos = Position + Player.CameraReference.forward / 10 * speed;

                        if (!Physics.Linecast(Position, pos, MicroHIDItem.WallMask))
                            Player.Position = pos;
                        else wall = true;
                        break;

                    case MovementDirection.BackWards:
                        pos = Position - Player.CameraReference.forward / 10 * speed;

                        if (!Physics.Linecast(Position, pos, MicroHIDItem.WallMask))
                            Player.Position = pos;
                        else wall = true;
                        break;

                    case MovementDirection.Right:
                        pos = Position + Quaternion.AngleAxis(90, Vector3.up) * Player.CameraReference.forward / 10 *
                            speed;

                        if (!Physics.Linecast(Position, pos, MicroHIDItem.WallMask))
                            Player.Position = pos;
                        else wall = true;
                        break;

                    case MovementDirection.Left:
                        pos = Position - Quaternion.AngleAxis(90, Vector3.up) * Player.CameraReference.forward / 10 *
                            speed;

                        if (!Physics.Linecast(Position, pos, MicroHIDItem.WallMask))
                            Player.Position = pos;
                        else wall = true;
                        break;
                }

                if (wall)
                {
                    Direction = MovementDirection.None;
                }
            }
            catch (Exception e)
            {
                NeuronLogger.For<Synapse>().Error($"Sy3 Dummy: Update Failed:\n{e}");
            }
        }
    }
    
    public override void HideFromAll() => DeSpawn();

    public override void ShowAll() => Spawn();

    //TODO:
    public override void HideFromPlayer(SynapsePlayer player) { }

    public override void ShowPlayer(SynapsePlayer player) { }
}