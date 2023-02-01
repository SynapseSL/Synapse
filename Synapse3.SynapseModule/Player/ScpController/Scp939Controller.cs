using System.Collections.Generic;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Mimicry;
using PlayerRoles.PlayableScps.Scp939.Ripples;
using RelativePositioning;
using UnityEngine;
using static PlayerRoles.PlayableScps.Scp939.Mimicry.MimicryRecorder;

namespace Synapse3.SynapseModule.Player.ScpController;

public class Scp939Controller : ScpShieldController<Scp939Role>
{
    public Scp939Controller(SynapsePlayer player) : base(player) { }

    public Scp939AmnesticCloudAbility AmnesticCloudAbility => Role?.GetSubroutine<Scp939AmnesticCloudAbility>();
    public Scp939LungeAbility LungeAbility => Role?.GetSubroutine<Scp939LungeAbility>();
    public EnvironmentalMimicry MimicryAbility => Role?.GetSubroutine<EnvironmentalMimicry>();
    public MimicryRecorder MimicryRecorder => Role?.GetSubroutine<MimicryRecorder>();
    public FirearmRippleTrigger RippleTrigger => Role?.GetSubroutine<FirearmRippleTrigger>();

    //TODO:
    public List<SynapsePlayer> VisiblePlayers { get; } = new();

    public Scp939LungeState State 
    {
        get => LungeAbility.State;
        set => LungeAbility.State = value;
    }
    
    public float AmnesticCloudCooldown
    {
        get => AmnesticCloudAbility?.Cooldown.Remaining ?? 0;
        set => AmnesticCloudAbility?.Cooldown.Trigger(value);
    }

    public float MimicryCloudCooldown
    {
        get => MimicryAbility?.Cooldown.Remaining ?? 0;
        set => MimicryAbility?.Cooldown.Trigger(value);
    }

    public List<MimicryRecording> VoicesSave => MimicryRecorder.SavedVoices;

    public bool MinicryPointPositioned => MimicryAbility._mimicPoint.Active;

    public Vector3 MinicryPointPosition
        => MinicryPointPositioned ?
        MimicryAbility._mimicPoint.transform.position :
        Vector3.zero;

    public void TriggerLunge() => LungeAbility.TriggerLunge();

    public void Sound(Vector3 postion)
    {
        var ripple = RippleTrigger;
        if (ripple == null) return;
        ripple._syncRoleColor = RoleTypeId.ChaosRepressor;
        ripple._syncRipplePos = new RelativePosition(postion);
        Role.GetSubroutine<FirearmRippleTrigger>().ServerSendRpc(_player);
    }

    public override RoleTypeId ScpRole => RoleTypeId.Scp939;
}