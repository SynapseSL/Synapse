using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Mimicry;
using PlayerRoles.PlayableScps.Scp939.Ripples;
using System.Collections.Generic;
using UnityEngine;
using static PlayerRoles.PlayableScps.Scp939.Mimicry.MimicryRecorder;

namespace Synapse3.SynapseModule.Player;

public class Scp939Controller : ScpShieldControler<Scp939Role>
{
    public Scp939Controller(SynapsePlayer player) : base(player) { }

    public override HumeShieldModuleBase SheildModule => Role?.HumeShieldModule;

    public Scp939AmnesticCloudAbility AmnesticCloudAbility => Role?.GetSubroutine<Scp939AmnesticCloudAbility>();
    public Scp939LungeAbility LungeAbility => Role?.GetSubroutine<Scp939LungeAbility>();
    public EnvironmentalMimicry MimicryAbility => Role?.GetSubroutine<EnvironmentalMimicry>();
    public MimicryRecorder MimicryRecorder => Role?.GetSubroutine<MimicryRecorder>();

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


    //TODO:
    public void Sound(Vector3 postion, float range)
    {


        //Fake player movement
    }
}