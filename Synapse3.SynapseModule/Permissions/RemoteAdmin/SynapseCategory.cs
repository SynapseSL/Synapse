using System.Collections.Generic;
using GameCore;
using RemoteAdmin.Communication;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

[RaCategory(
    Name = "[ Synapse 3]",
    Color = "blue",
    Id = 10001
    )]
public class SynapseCategory : RemoteAdminCategory
{
    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var synapseInfo = "<color=white>Welcome to <color=blue>Synapse3<color=white>";
        synapseInfo += "\nSynapse manages Plugins as well as improving many vanilla features";
        synapseInfo += "\n\n";
        synapseInfo += "\nCurrent Version: " + Synapse.GetVersion();
        synapseInfo += "\nBased Game Version: " + Synapse.BasedGameVersion;
        synapseInfo += "\nCurrent Game Version: " + Version.VersionString;

        synapseInfo += "\n\n";
        synapseInfo += "\nDownload: <size=10><i><link=CP_USERID>https://github.com/SynapseSL/Synapse/releases</link></i></size>";
        synapseInfo += "\nDocs: <size=10><i><link=CP_IP>https://docs.synapsesl.xyz/</link></i></size>";
        synapseInfo += "\nDiscord: <size=10><i><link=CP_ID>https://discord.gg/uVtNr9Czng</link></i></size>";
                    
        synapseInfo += "\n\n<size=15>Created by Dimenzio, Helight, Wholesome & Flo";

        if (secondPage)
            synapseInfo += " (and maybe UselessJavaDev MineTech)";
                    
        synapseInfo += "</color>";
                    
        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId,
            "https://github.com/SynapseSL/Synapse/releases");
        RaClipboard.Send(sender,RaClipboard.RaClipBoardType.Ip,"https://docs.synapsesl.xyz/");
        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, "https://discord.gg/uVtNr9Czng");

        return synapseInfo;
    }

    public override List<SynapsePlayer> GetPlayers() => null;

    public override bool DisplayOnTop => true;
    public override bool CanSeeCategory(SynapsePlayer player) => true;

    public override string ExternalURL => "https://synapsesl.xyz/";
}