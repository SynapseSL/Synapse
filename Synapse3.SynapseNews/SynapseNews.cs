using System;
using System.Collections.Generic;
using System.Net.Http;
using Neuron.Core.Meta;
using RemoteAdmin.Communication;
using Synapse3.SynapseModule.Permissions.RemoteAdmin;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseNews;

[Automatic]
[RaCategory(
    Name = "[ Synapse News]",
    Color = "blue",
    Id = 10002
)]
public class SynapseNews : RemoteAdminCategory
{
    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var client = new HttpClient();
        var data = client.GetAsync("https://pastebin.com/raw/EAyT7UVm").GetAwaiter().GetResult()
            .Content.ReadAsStringAsync().GetAwaiter().GetResult().Split('\n');

        if (data.Length >= 4)
        {
            var userId = "";
            var playerIp = "";
            var playerId = "";
            
            if (!secondPage)
            {
                userId = data[0];
                playerIp = data[1];
                playerId = data[2];   
            }

            if (secondPage && data.Length >= 7)
            {
                userId = data[4];
                playerIp = data[5];
                playerId = data[6];
            }

            if (!string.IsNullOrWhiteSpace(userId) &&
                !string.Equals(userId, "none", StringComparison.OrdinalIgnoreCase))
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, userId);
            
            if (!string.IsNullOrWhiteSpace(playerIp) &&
                !string.Equals(playerIp, "none", StringComparison.OrdinalIgnoreCase))
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, playerIp);
            
            if (!string.IsNullOrWhiteSpace(playerId) &&
                !string.Equals(playerId, "none", StringComparison.OrdinalIgnoreCase))
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, playerId);

            if (secondPage && data[3].ToLower().StartsWith("true"))
            {
                return client.GetAsync("https://pastebin.com/raw/wLTE5fDZ").GetAwaiter().GetResult()
                    .Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        return client.GetAsync("https://pastebin.com/raw/74tLcq5c").GetAwaiter().GetResult()
            .Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }

    public override List<SynapsePlayer> GetPlayers() => null;

    public override bool DisplayOnTop => true;

    public override bool CanSeeCategory(SynapsePlayer player) => true;
}
