using MEC;
using Mirror;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.SpawnData;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Dummy;

public class DummyPlayer : SynapsePlayer
{

    public override PlayerType PlayerType => PlayerType.Dummy;

    public override Vector3 Position
    {
        set
        {
            var fps = FirstPersonMovement;
            if (fps == null) return;

            fps.Position = value;
            fps.OnServerPositionOverwritten();
        }
    }


    public override RoleTypeId RoleType//TODO
    {
        set
        {
            Hub.roleManager.ServerSetRole(value, RoleChangeReason.None);
            var fps = FirstPersonMovement;
            NeuronLogger.For<Synapse>().Debug("fps == null:" + (fps == null));
            if (fps == null) return;
            fps.Position = Position;
            fps.OnServerPositionOverwritten();
        }
    }

    public override float RotationHorizontal
    {
        set
        {
            var fps = FirstPersonMovement;
            if (fps == null) return;
            fps.MouseLook.CurrentHorizontal = value;
            fps.OnServerPositionOverwritten();
        }
    }

    public override float RotationVectical
    {
        set
        {
            var fps = FirstPersonMovement;
            if (fps == null) return;
            fps.MouseLook.CurrentVertical = value;
            fps.OnServerPositionOverwritten();
        }
    }

    //TODO: Fist do the player Rotation
    /*    public override Vector2 RotationVector2
        {
            get => base.RotationVector2;
            set => ReceiveRotation(value);
        }

        public override float RotationFloat
        {
            get => base.RotationFloat;
            set => ReceiveRotation(new Vector2(0f, value));
        }

        public override PlayerMovementSync.PlayerRotation PlayerRotation
        {
            get => base.PlayerRotation;
            set => ReceiveRotation(new Vector2(value.x ?? 0f, value.y ?? 0f));
        }

        private void ReceiveRotation(Vector2 rotation)
        {
            PlayerMovementSync.Rotations = rotation;
            transform.localRotation = Quaternion.Euler(0f, PlayerMovementSync.Rotations.y, 0f);
        }*/


    public override void Awake()
    {
        var service = Synapse.Get<DummyService>();
        //This need to wait one Frame or else it will be executed before Synapse can set SynapseDummy
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            if (service._dummies.Contains(SynapseDummy)) return;

            service._dummies.Add(SynapseDummy);
        });
    }

    /*
    [2022-12-18 15:46:30.194 +01:00] [Error] Synapse: Sy3 Command: PlayerKeyPress failed
                                 System.NullReferenceException
                                   at (wrapper managed-to-native) UnityEngine.Component.get_transform(UnityEngine.Component)
                                   at PlayerRoles.PlayerRoleManager.InitializeNewRole (PlayerRoles.RoleTypeId targetId, PlayerRoles.RoleChangeReason reason, Mirror.NetworkReader data) [0x00039] in <bf179521d4cd490bbb453145064bf4e5>:0
                                   at PlayerRoles.PlayerRoleManager.ServerSetRole (PlayerRoles.RoleTypeId newRole, PlayerRoles.RoleChangeReason reason) [0x0005f] in <bf179521d4cd490bbb453145064bf4e5>:0
                                   at Synapse3.SynapseModule.Dummy.DummyPlayer.set_RoleType (PlayerRoles.RoleTypeId value) [0x00007] in <3885dead808c40f09b08e71eb052a0e8>:0
                                   at Synapse3.SynapseModule.DebugService.OnKeyPress (Synapse3.SynapseModule.Events.KeyPressEvent ev) [0x00091] in <3885dead808c40f09b08e71eb052a0e8>:0
                                   at Neuron.Core.Events.EventReactor`1[T].Raise (T evt) [0x0000b] in <1e0795032c2f42809e2854dedfdb3bae>:0
                                   at Synapse3.SynapseModule.Command.SynapseCommands.KeyPressCommand.Execute (Synapse3.SynapseModule.Command.SynapseContext context, Neuron.Modules.Commands.CommandResult& result) [0x00165] in <3885dead808c40f09b08e71eb052a0e8>:0
    */

    private void SpawnPlayerModel(RoleTypeId role, bool firstSpawn)
    {   //Note: No client probaly no whriter
        PlayerRoleBase roleBase = RoleManager.GetRoleBase(role);
        Transform obj = roleBase.transform;
        obj.parent = base.transform;
        obj.localPosition = Vector3.zero;
        obj.localRotation = Quaternion.identity;
        CurrentRole = roleBase;
        roleBase.Init(Hub, RoleChangeReason.RemoteAdmin);//IDK who choice and the impacte
        roleBase.SpawnPoolObject();
        //EventManager.ExecuteEvent(ServerEventType.PlayerSpawn, Hub, CurrentRole.RoleTypeId); TODO: Need to use it for NW API? Or not? Reflexion?

        if (firstSpawn)
        {
            //PlayerRoleManager.OnRoleChanged?.Invoke(Hub, playerRoleBase, CurrentRole); TODO: use reflexion?
        }

        //SpawnProtected.TryGiveProtection(Hub); No spawn Protection for dummy (for me logic) 
    }

    public override void OnDestroy()
    {
        var service = Synapse.Get<DummyService>();
        service._dummies.Remove(SynapseDummy);
    }
   
    public SynapseDummy SynapseDummy { get; internal set; }

    public override TTranslation GetTranslation<TTranslation>(TTranslation translation) => translation.Get();
}