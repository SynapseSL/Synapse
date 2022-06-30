using System;
using System.Reflection;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mirror;
using PlayerStatsSystem;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using UnityEngine;

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
    
    public static DamageType GetDamageType(this DamageHandlerBase handler)
    {
        if (handler == null) return DamageType.Unknown;
                
        if(Enum.TryParse<DamageType>(handler.GetType().Name.Replace("DamageHandler",""),out var type))
        {
            if(type == DamageType.Universal)
            {
                var id = ((UniversalDamageHandler)handler).TranslationId;

                if (id > 23) return DamageType.Universal;

                return (DamageType)id;
            }

            return type;
        }

        return DamageType.Unknown;
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
    
    public static SynapsePlayer GetPlayer(this NetworkConnection connection) => connection.identity.GetPlayer();

    public static SynapsePlayer GetPlayer(this MonoBehaviour mono) => mono?.gameObject?.GetComponent<SynapsePlayer>();

    public static SynapsePlayer GetPlayer(this GameObject gameObject) => gameObject?.GetComponent<SynapsePlayer>();

    public static SynapsePlayer GetPlayer(this PlayableScps.PlayableScp scp) => scp?.Hub?.GetPlayer();

    public static SynapsePlayer GetPlayer(this CommandSender sender) => Synapse.Get<PlayerService>().GetPlayer(x => x.CommandSender == sender);

    public static SynapsePlayer GetPlayer(this StatBase stat) => stat.Hub.GetPlayer();

    public static SynapsePlayer GetPlayer(this Footprinting.Footprint footprint) => footprint.Hub?.GetPlayer();

    public static SynapseItem GetSynapseItem(this ItemPickupBase pickupBase) =>
        Synapse.Get<ItemService>().GetSynapseItem(pickupBase.Info.Serial);
    
    public static SynapseItem GetSynapseItem(this ItemBase itemBase) =>
        Synapse.Get<ItemService>().GetSynapseItem(itemBase.ItemSerial);

    public static bool GetHarmPermission(SynapsePlayer attacker, SynapsePlayer victim, bool ignoreFFConfig = false)
    {
        //TODO:
        return false;
    }
}