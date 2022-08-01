using System.Collections.Generic;
using System.Net.Http;
using Neuron.Core.Meta;
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
        return new HttpClient().GetAsync("https://pastebin.com/raw/74tLcq5c").GetAwaiter().GetResult()
            .Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }

    public override List<SynapsePlayer> GetPlayers() => null;

    public override bool DisplayOnTop => true;

    public override bool CanSeeCategory(SynapsePlayer player) => true;
}
