namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public bool GlobalRemoteAdmin => ServerRoles.RemoteAdminMode == ServerRoles.AccessMode.GlobalAccess;
    
    public ulong GlobalPerms => ServerRoles._globalPerms;
}