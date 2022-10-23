﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Footprinting;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using MapGeneration;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Modules.Configs.Localization;
using PlayableScps;
using PlayerStatsSystem;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic;
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
    /// Updates Position Rotation and Scale of an NetworkObject for all players
    /// </summary>
    public static void UpdatePositionRotationScale(this NetworkIdentity identity)
        => NetworkServer.SendToAll(Synapse.Get<MirrorService>().GetSpawnMessage(identity));
    
    

    /// <summary>
    /// Hides an NetworkObject for a single players
    /// </summary>
    public static void UnSpawnForOnePlayer(this NetworkIdentity identity, SynapsePlayer player)
    {
        var msg = new ObjectDestroyMessage { netId = identity.netId };
        player.Connection.Send(msg);
    }

    public static void SpawnForOnePlayer(this NetworkIdentity identity, SynapsePlayer player)
        => player.Connection.Send(Synapse.Get<MirrorService>().GetSpawnMessage(identity));

    /// <summary>
    /// Hides an NetworkObject for all Players on the Server that are currently connected
    /// </summary>
    public static void UnSpawnForAllPlayers(this NetworkIdentity identity)
    {
        var msg = new ObjectDestroyMessage { netId = identity.netId };
        NetworkServer.SendToAll(msg);
    }
    
    public static void SpawnForAllPlayers(this NetworkIdentity identity) => UpdatePositionRotationScale(identity);
    
    public static bool CheckPermission(this DoorPermissions door, SynapsePlayer player) =>
        CheckPermission(door.RequiredPermissions, player, door.RequireAll);
    
    public static bool CheckPermission(this KeycardPermissions permissions, SynapsePlayer player,
        bool needIdentical = false)
    {
        var ev2 = new CheckKeyCardPermissionEvent(player, false, permissions);
        if (player.Bypass || (ushort)permissions == 0) ev2.Allow = true;
        if (player.TeamID == (uint)Team.SCP && permissions.HasFlagFast(KeycardPermissions.ScpOverride)) ev2.Allow = true;

        if (!ev2.Allow)
        {
            var items = Synapse.Get<SynapseConfigService>().GamePlayConfiguration.RemoteKeyCard
                ? player.Inventory.Items.ToList()
                : new List<SynapseItem> { player.Inventory.ItemInHand };

            foreach (var item in items)
            {
                if (item.ItemCategory != ItemCategory.Keycard || item.Item == null) continue;

                var overlappingPerms = ((KeycardItem)item.Item).Permissions & permissions;
                var ev = new KeyCardInteractEvent(item, ItemInteractState.Finalize, player, permissions)
                {
                    Allow = needIdentical ? overlappingPerms == permissions : overlappingPerms > KeycardPermissions.None,
                };
            
                Synapse.Get<ItemEvents>().KeyCardInteract.Raise(ev);
                if (!ev.Allow) continue;
                ev2.Allow = true;
                break;
            }   
        }

        Synapse.Get<PlayerEvents>().CheckKeyCardPermission.Raise(ev2);
        return ev2.Allow;
    }
    
    
    
    public static SynapsePlayer GetSynapsePlayer(this NetworkConnection connection) => connection?.identity?.GetSynapsePlayer();
    public static SynapsePlayer GetSynapsePlayer(this MonoBehaviour mono) => mono?.gameObject?.GetComponent<SynapsePlayer>();
    public static SynapsePlayer GetSynapsePlayer(this GameObject gameObject) => gameObject?.GetComponent<SynapsePlayer>();
    public static SynapsePlayer GetSynapsePlayer(this PlayableScp scp) => scp?.Hub?.GetSynapsePlayer();
    public static SynapsePlayer GetSynapsePlayer(this CommandSender sender) => Synapse.Get<PlayerService>()
        .GetPlayer(x => x.CommandSender == sender, PlayerType.Dummy, PlayerType.Player, PlayerType.Server);
    public static SynapsePlayer GetSynapsePlayer(this StatBase stat) => stat.Hub.GetSynapsePlayer();
    public static SynapsePlayer GetSynapsePlayer(this Footprint footprint) => footprint.Hub?.GetSynapsePlayer();

    
    public static SynapseItem GetItem(this ItemPickupBase pickupBase) =>
        Synapse.Get<ItemService>().GetSynapseItem(pickupBase.Info.Serial);
    public static SynapseItem GetItem(this ItemBase itemBase) =>
        Synapse.Get<ItemService>().GetSynapseItem(itemBase.ItemSerial);

    
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
    public static IRoom GetRoom(this RoomType type) =>
        Synapse.Get<RoomService>()._rooms.FirstOrDefault(x => x.Id == (int)type);

    public static IElevator GetSynapseElevator(this ElevatorType type) => Synapse.Get<ElevatorService>().Elevators
        .FirstOrDefault(x => x is SynapseElevator elevator && elevator.ElevatorType == type);


    public static IVanillaRoom GetVanillaRoom(this RoomIdentifier identifier) => (IVanillaRoom)Synapse.Get<RoomService>()._rooms
        .FirstOrDefault(x => x.GameObject == identifier.gameObject);
    
    public static SynapseDoor GetSynapseDoor(this DoorVariant variant)
    {
        var script = variant.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseDoor door)
        {
            return door;
        }

        NeuronLogger.For<Synapse>().Debug("Found DoorVariant without SynapseObjectScript ... creating new SynapseDoor");
        return new SynapseDoor(variant);
    }

    public static SynapseGenerator GetSynapseGenerator(this Scp079Generator generator)
    {
        var script = generator.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseGenerator gen)
        {
            return gen;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found Scp079Generator without SynapseObjectScript ... creating new SynapseGenerator");
        return new SynapseGenerator(generator);
    }

    public static SynapseWorkStation GetSynapseWorkStation(this WorkstationController workstationController)
    {
        var script = workstationController.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseWorkStation workStation)
        {
            return workStation;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found WorkStationController without SynapseObjectScript ... creating new SynapseWorkStation");
        return new SynapseWorkStation(workstationController);
    }

    public static SynapseLocker GetSynapseLocker(this Locker locker)
    {
        var script = locker.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseLocker synapseLocker)
        {
            return synapseLocker;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found Locker without SynapseObjectScript ... creating new SynapseLocker");
        return new SynapseLocker(locker);
    }

    public static SynapseRagdoll GetSynapseRagdoll(this Ragdoll rag)
    {
        var script = rag.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseRagdoll ragdoll)
        {
            return ragdoll;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found Ragdoll without SynapseObjectScript ... creating new SynapseRagdoll");
        return new SynapseRagdoll(rag);
    }

    
    public static SynapseTesla GetSynapseTesla(this TeslaGate gate) =>
        Synapse.Get<MapService>()._synapseTeslas.FirstOrDefault(x => x.Gate == gate);

    public static IElevator GetSynapseElevator(this Lift lift) =>
        Synapse.Get<ElevatorService>().Elevators
            .FirstOrDefault(x => x is SynapseElevator elevator && elevator.Lift == lift);
    public static SynapseCamera GetCamera(this Camera079 cam) =>
        Synapse.Get<MapService>()._synapseCameras.FirstOrDefault(x => x.Camera == cam);

    
    public static bool CanHarmScp(SynapsePlayer player, bool message)
    {
        if (player.TeamID != (int)Team.SCP &&
            player.CustomRole?.GetFriendsID().Any(x => x == (int)Team.SCP) != true) return true;
        
        if (message)
            player.SendHint(Synapse.Get<SynapseConfigService>().Translation.Get(player).ScpTeam);
        return false;

    }
    public static bool GetHarmPermission(SynapsePlayer attacker, SynapsePlayer victim, bool ignoreFFConfig = false)
    {
        try
        {
            bool allow;

            if (Synapse.Get<RoundService>().RoundEnded &&
                Synapse.Get<SynapseConfigService>().GamePlayConfiguration.AutoFriendlyFire)
            {
                allow = true;
            }
            else if (victim.PlayerType == PlayerType.Dummy)
            {
                allow = false;
            }
            else if (attacker == victim)
            {
                allow = true;
            }
            else if (attacker.Team == Team.RIP && victim.Team == Team.RIP)
            {
                allow = false;
            }
            else if (attacker.CustomRole == null && victim.CustomRole == null)
            {
                if (attacker.Team == Team.SCP && victim.Team == Team.SCP) allow = false;

                var ff = ignoreFFConfig || Synapse.Get<ServerService>().FF;

                if (ff)
                {
                    allow = true;
                }
                else
                {
                    allow = attacker.Faction != victim.Faction;
                }
            }
            else
            {
                allow = true;
                if (attacker.CustomRole != null && attacker.CustomRole.GetFriendsID().Any(x => x == victim.TeamID))
                {
                    allow = false;
                    attacker.SendHint(Synapse.Get<SynapseConfigService>().Translation.Get(attacker).SameTeam);
                }

                if (victim.CustomRole != null && victim.CustomRole.GetFriendsID().Any(x => x == attacker.TeamID))
                {
                    allow = false;
                    attacker.SendHint(Synapse.Get<SynapseConfigService>().Translation.Get(attacker).SameTeam);
                }
            }

            var ev = new HarmPermissionEvent(attacker, victim, allow);
            Synapse.Get<PlayerEvents>().HarmPermission.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 FF: Harm Permission Event failed\n" + ex);
            return true;
        }
    }

    public static TTranslation Get<TTranslation>(this TTranslation translation)
        where TTranslation : Translations<TTranslation>, new()
        => translation.WithLocale(Synapse.Get<SynapseConfigService>().HostingConfiguration.Language);

    public static TTranslation Get<TTranslation>(this TTranslation translation, SynapsePlayer player)
        where TTranslation : Translations<TTranslation>, new()
        => player.GetTranslation(translation);

    public static string Replace(this string msg, Dictionary<string, string> values)
    {
        foreach (var value in values)
        {
            msg = msg.Replace(value.Key, value.Value);
        }

        return msg;
    }
}