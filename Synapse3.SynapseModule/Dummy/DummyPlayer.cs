using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.SpawnData;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Dummy;

public class DummyPlayer : SynapsePlayer
{
    //TODO:
    
    public override PlayerType PlayerType => PlayerType.Dummy;

    public override RoleTypeId RoleType//TODO
    {
        set
        {
            RoleManager.InitializeNewRole(value, RoleChangeReason.RemoteAdmin);//Try first like this
            //RoleManager.InitializeNewRole(value, (byte)9);//Use 9 for Synapse API Reason

            //SpawnPlayerModel()
            //TODO: Set Dummy Role
            //NetworkServer.UnSpawn(gameObject);
            //ClassManager.CurClass = value;
            //NetworkServer.Spawn(gameObject);
        }
    }

   /* public override Quaternion Rotation
    {
        get => transform.localRotation;
        set
        {
            var euler = value.eulerAngles;
            PlayerMovementSync.Rotations = new Vector2(euler.x, euler.y);
            transform.localRotation = value;
        }
    }*/

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