using System.Collections.Generic;
using Footprinting;
using PlayerRoles.PlayableScps.Scp106;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class Scp106Controller
{
    //TODO:
    
    private readonly SynapsePlayer _player;
    
    internal Scp106Controller(SynapsePlayer player)
    {
        _player = player;
    }

    public Scp106Role Scp106PlayerScript => _player.CurrentRole as Scp106Role;

    public bool IsUsingPortal => Scp106PlayerScript.Sinkhole.IsDuringAnimation;
    public bool IsInGround => Scp106PlayerScript.IsSubmerged;

    public HashSet<SynapsePlayer> PlayersInPocket { get; } = new();
/*
    public void UsePortal() => Scp106PlayerScript.UserCode_CmdUsePortal();

    public void DeletePortal() => Scp106PlayerScript.DeletePortal();

    public void UpdatePortal() => Scp106PlayerScript.CreatePortalInCurrentPosition();

    public void Contain(SynapsePlayer recontainer = null) =>
        Scp106PlayerScript.Contain(new Footprint(recontainer ?? _player));

    public void CapturePlayer(SynapsePlayer player) =>
        Scp106PlayerScript.UserCode_CmdMovePlayer(player.gameObject, ServerTime.time);*/
        
}