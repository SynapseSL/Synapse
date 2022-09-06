namespace Synapse3.SynapseModule.Enums;

public enum InvisibleMode
{
    /// <summary>
    /// The Player is Visible for everyone
    /// </summary>
    None,
    /// <summary>
    /// The Player is only visual Invisible but SCP-079, SCP-096 and SCP-939 are still able to scan/feel/hear them
    /// </summary>
    Visual,
    /// <summary>
    /// The Player is invisible for every living being but Spectator can see them
    /// </summary>
    Alive,
    /// <summary>
    /// The Player can only be seen by Spectators and can't trigger SCP-173/SCP-096 or teslas
    /// </summary>
    Ghost,
    /// <summary>
    /// The Player is invisible for all that doesn't have the synapse.see.invisible permission
    /// </summary>
    Admin,
    /// <summary>
    /// No one can see the Player except the Player himself
    /// </summary>
    Full
}