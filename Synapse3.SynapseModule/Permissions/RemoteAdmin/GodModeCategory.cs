﻿using System.Collections.Generic;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

[RaCategory(
    Name = "[ GodMode]",
    Color = "#03f8fc",
    Size = 20,
    Id = 103
    )]
public class GodModeCategory : RemoteAdminCategory
{
    private PlayerService _player;
    private SynapseConfigService _config;

    public GodModeCategory(PlayerService player, SynapseConfigService config)
    {
        _player = player;
        _config = config;
    }

    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var text = "<color=white>Selects all Players that are currently in GodMode:\n";

        foreach (var player in GetPlayers())
        {
            text += player.NickName + "\n";
        }
        
        return text + "</color>";
    }

    public override List<SynapsePlayer> GetPlayers() => _player.GetPlayers(x => x.GodMode, PlayerType.Player);

    public override bool DisplayOnTop => false;

    public override bool CanSeeCategory(SynapsePlayer player) =>
        _config.PermissionConfiguration.BetterRemoteAdminList &&
        _config.PermissionConfiguration.EnableGameModeCategories && GetPlayers().Count > 0;
}