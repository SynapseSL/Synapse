using Hints;
using InventorySystem.Items.Firearms.Attachments;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// Kicks the Player from the Server with a specified Reason
    /// </summary>
    public void Kick(string message) => ServerConsole.Disconnect(gameObject, message);

    /// <summary>
    /// Bans the Player
    /// </summary>
    public void Ban(int duration, string reason, string issuer = "Plugin") => Synapse.GetObjectOf<BanPlayer>().BanUser(gameObject, duration, reason, issuer);
    
    /// <summary>
    /// Returns a uint Value that corresponds to the players favorite Attachments for a specific Weapon
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public uint GetPreference(ItemType item)
    {
        if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(Hub, out var dict) && dict.TryGetValue(item, out var result))
            return result;

        return 0;
    }
    
    /// <summary>
    /// Displays a hint on the Player's screen
    /// </summary>
    public void GiveTextHint(string message, float duration = 5f)
    {
        Hub.hints.Show(new TextHint(message, new HintParameter[]
        {
            new StringHintParameter("")
        }, HintEffectPresets.FadeInAndOut(duration), duration));
    }
    
    //TODO: Broadcast Stuff
    
    public void SendConsoleMessage(string message, string color = "red") => ClassManager.TargetConsolePrint(Connection, message, color);

    public void SendRaConsoleMessage(string message, bool success = true, RaCategory type = RaCategory.None) => CommandSender.RaMessage(message, success, type);

    public void GiveEffect(Effect effect, byte intensity = 1, float duration = -1f) => PlayerEffectsController.ChangeByString(effect.ToString().ToLower(), intensity, duration);

    //TODO:
    /*
    public void RaLogin()
    {
        ServerRoles.RemoteAdmin = true;
        ServerRoles.Permissions = SynapseGroup.GetVanillaPermissionValue() | ServerRoles._globalPerms;
        ServerRoles.RemoteAdminMode = GlobalRemoteAdmin ? ServerRoles.AccessMode.GlobalAccess : ServerRoles.AccessMode.PasswordOverride;
        if (!ServerRoles.AdminChatPerms)
            ServerRoles.AdminChatPerms = SynapseGroup.HasVanillaPermission(PlayerPermissions.AdminChat);
        ServerRoles.TargetOpenRemoteAdmin(false);

        QueryProcessor.SyncCommandsToClient();
    }

    public void RaLogout()
    {
        Hub.serverRoles.RemoteAdmin = false;
        Hub.serverRoles.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
        Hub.serverRoles.TargetCloseRemoteAdmin();
    }
    */
    
    public void Heal(float hp) => GetStatBase<HealthStat>().ServerHeal(hp);

    public bool Hurt(float damage, DamageType type = DamageType.Unknown)
    {
        var handler = type.GetUniversalDamageHandler();
        handler.Damage = damage;
        return PlayerStats.DealDamage(handler);
    }

    public bool Hurt(DamageHandlerBase handlerbase) => PlayerStats.DealDamage(handlerbase);

    public void Kill() => Kill("Unknown Reason");

    public bool Kill(string reason) => PlayerStats.DealDamage(new CustomReasonDamageHandler(reason));

    
    public bool Kill(string reason, string cassie)
    {
        //TODO:
        var result = Kill(reason);
        //if (result)
        //    Server.Get.Map.Cassie(cassie);
        return result;
    }
}