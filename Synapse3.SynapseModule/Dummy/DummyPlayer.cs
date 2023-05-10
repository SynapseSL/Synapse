using MEC;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Dummy;

public class DummyPlayer : SynapsePlayer
{
    public override PlayerType PlayerType => PlayerType.Dummy;

    public bool RaVisible { get; set; } = false;

    public bool SpectatorVisible { get; set; } = false;

    public bool DestroyWhenDied { get; set; } = true;

    public override Quaternion Rotation
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            return mouseLook == null
                ? new Quaternion(0, 0, 0, 0)
                : Quaternion.Euler(mouseLook._syncHorizontal, mouseLook._curVertical, 0f);
        }
    }

    public override float RotationVertical
    {
        get => FirstPersonMovement?.MouseLook._curVertical ?? 0;
    }
    
    public void SetRotation(Quaternion rotation)
    {
        var firstPerson = FirstPersonMovement;
        if (firstPerson == null) return;
        var euler = rotation.eulerAngles;
        firstPerson.MouseLook._syncHorizontal = euler.y;
        firstPerson.MouseLook._curVertical = euler.x;
        firstPerson.OnServerPositionOverwritten();
    }

    public void SetRotation(Vector2 rotation)
    {
        var firstPerson = FirstPersonMovement;
        if (firstPerson == null) return;
        firstPerson.MouseLook._curHorizontal = rotation.x;
        firstPerson.MouseLook._curVertical = rotation.y;
        firstPerson.OnServerPositionOverwritten();
    }

    public void SetRotationVertical(float rotation)
    {
        var firstPerson = FirstPersonMovement;
        if (firstPerson == null) return;
        firstPerson.MouseLook._curVertical = rotation;
        firstPerson.OnServerPositionOverwritten();
    }

    public void SetRotationHorizontal(float rotation)
    {
        var firstPerson = FirstPersonMovement;
        if (firstPerson == null) return;
        firstPerson.MouseLook._syncHorizontal = rotation;
        firstPerson.OnServerPositionOverwritten();
    }

    public override float RotationHorizontal 
    {
        get => FirstPersonMovement?.MouseLook._syncHorizontal ?? 0;
    }

    public override Vector2 RotationVector2
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            return mouseLook == null ? Vector2.zero : new Vector2(mouseLook._curHorizontal, mouseLook._curVertical);
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

    public override void SendWindowMessage(string text) { }

    public SynapseDummy SynapseDummy { get; internal set; }

    public override TTranslation GetTranslation<TTranslation>(TTranslation translation) => translation.Get();
}