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

    private bool _raVisble = false;
    public bool RaVisible 
    { 
        get => _raVisble; 
        set => _raVisble = value; 
    }

    public bool DestroyWhenDied { get; set; } = true;

    public override Quaternion Rotation
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            if (mouseLook == null)
                return new Quaternion(0, 0, 0, 0);
            return Quaternion.Euler(mouseLook._syncHorizontal, mouseLook._curVertical, 0f);
        }
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            var euler = value.eulerAngles;
            firstperson.MouseLook._syncHorizontal = euler.y;
            firstperson.MouseLook._curVertical = euler.x;
            firstperson.OnServerPositionOverwritten();
        }
    }

    public override float RotationVectical
    {
        get => FirstPersonMovement?.MouseLook._curVertical ?? 0;
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            firstperson.MouseLook._curVertical = value;
            firstperson.OnServerPositionOverwritten();
        }
    }

    public override float RotationHorizontal 
    {
        get => FirstPersonMovement?.MouseLook._syncHorizontal ?? 0;
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            firstperson.MouseLook._syncHorizontal = value;
            firstperson.OnServerPositionOverwritten();
        }
    }

    public override Vector2 RotationVector2
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            if (mouseLook == null)
                return Vector2.zero;
            return new Vector2(mouseLook._curHorizontal, mouseLook._curVertical);
        }
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            firstperson.MouseLook._curHorizontal = value.x;
            firstperson.MouseLook._curVertical = value.y;
            firstperson.OnServerPositionOverwritten();
        }
    }

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

    public override void OnDestroy()
    {
        var service = Synapse.Get<DummyService>();
        service._dummies.Remove(SynapseDummy);
    }
   
    public SynapseDummy SynapseDummy { get; internal set; }

    public override TTranslation GetTranslation<TTranslation>(TTranslation translation) => translation.Get();
}