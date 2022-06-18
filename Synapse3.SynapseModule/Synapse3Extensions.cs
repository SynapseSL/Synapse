using System.Reflection;
using Mirror;
using PlayerStatsSystem;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

public static class Synapse3Extensions
{
    // FunFact: This method is the oldest method in Synapse and was originally created even before Synapse for an Exiled 1.0 Plugin
    /// <summary>
    /// Sends a message to the sender in the RemoteAdmin
    /// </summary>
    public static void RaMessage(this CommandSender sender, string message, bool success = true,
        RaCategory type = RaCategory.None)
    {
        var category = "";
        if (type != RaCategory.None)
            category = type.ToString();


        sender.RaReply($"{Assembly.GetCallingAssembly().GetName().Name}#" + message, success, true, category);
    }
    
    /// <summary>
    /// Returns a UniversalDamageHandler based upon the given DamageType
    /// </summary>
    public static UniversalDamageHandler GetUniversalDamageHandler(this DamageType type)
    {
        if((int)type < 0 || (int)type > 23) return new UniversalDamageHandler(0f,DeathTranslations.Unknown);

        return new UniversalDamageHandler(0f, DeathTranslations.TranslationsById[(byte)type]);
    }
    
    /// <summary>
    /// Updates Position Rotation and Scale of an NetworkObject for all players
    /// </summary>
    public static void UpdatePositionRotationScale(this NetworkIdentity identity)
        => NetworkServer.SendToAll(GetSpawnMessage(identity));

    /// <summary>
    /// Returns a Spawnmessage for an NetworkObject that can be modified
    /// </summary>
    public static SpawnMessage GetSpawnMessage(this NetworkIdentity identity)
    {
        var writer = NetworkWriterPool.GetWriter();
        var writer2 = NetworkWriterPool.GetWriter();
        var payload = NetworkServer.CreateSpawnMessagePayload(false, identity, writer, writer2);
        var gameObject = identity.gameObject;
        return new SpawnMessage
        {
            netId = identity.netId,
            isLocalPlayer = false,
            isOwner = false,
            sceneId = identity.sceneId,
            assetId = identity.assetId,
            position = gameObject.transform.position,
            rotation = gameObject.transform.rotation,
            scale = gameObject.transform.localScale,
            payload = payload
        };
    }

    /// <summary>
    /// Hides an NetworkObject for a single players
    /// </summary>
    public static void DespawnForOnePlayer(this NetworkIdentity identity, SynapsePlayer player)
    {
        var msg = new ObjectDestroyMessage { netId = identity.netId };
        player.Connection.Send(msg);
    }

    /// <summary>
    /// Hides an NetworkObject for all Players on the Server that are currently connected
    /// </summary>
    public static void DespawnForAllPlayers(this NetworkIdentity identity)
    {
        var msg = new ObjectDestroyMessage { netId = identity.netId };
        NetworkServer.SendToAll(msg);
    }
}