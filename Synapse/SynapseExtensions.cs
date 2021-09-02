using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Synapse;
using Synapse.Api;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using UnityEngine;
using MapGeneration;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using MapGeneration.Distributors;

public static class SynapseExtensions
{
    public static Player GetPlayer(this NetworkConnection connection) => connection.identity.GetPlayer();

    public static Player GetPlayer(this MonoBehaviour mono) => mono?.gameObject?.GetComponent<Player>();

    public static Player GetPlayer(this GameObject gameObject) => gameObject?.GetComponent<Player>();

    public static Player GetPlayer(this PlayableScps.PlayableScp scp) => scp?.Hub?.GetPlayer();

    public static Player GetPlayer(this CommandSender sender) => Server.Get.Players.FirstOrDefault(x => x.CommandSender == sender);

    public static List<Player> GetPlayers(this RoleType role) => SynapseController.Server.Players.Where(x => x.RoleType == role).ToList();

    public static List<Player> GetPlayers(this Team team) => SynapseController.Server.Players.Where(x => x.Team == team).ToList();

    public static List<Player> GetPlayers(this Faction fraction) => SynapseController.Server.Players.Where(x => x.Faction == fraction).ToList();

    public static List<Player> GetPlayers(this RoleType[] roles) => SynapseController.Server.Players.Where(x => roles.Any(y => x.RoleType == y)).ToList();

    public static List<Player> GetPlayers(this Team[] teams) => SynapseController.Server.Players.Where(x => teams.Any(y => x.Team == y)).ToList();

    public static List<Player> GetPlayers(this Faction[] fractions) => SynapseController.Server.Players.Where(x => fractions.Any(y => x.Faction == y)).ToList();

    public static void RaMessage(this CommandSender sender, string message, bool success = true,
            RaCategory type = RaCategory.None)
    {
        var category = "";
        if (type != RaCategory.None)
            category = type.ToString();


        sender.RaReply($"{Assembly.GetCallingAssembly().GetName().Name}#" + message, success, true, category);
    }

    public static Room GetSynapseRoom(this RoomIdentifier identifier) => Map.Get.Rooms.FirstOrDefault(x => x.Identifier == identifier);

    public static Generator GetGenerator(this Scp079Generator generator079) => Map.Get.Generators.FirstOrDefault(x => x.GameObject == generator079.gameObject);

    public static Door GetDoor(this Interactables.Interobjects.DoorUtils.DoorVariant door) => Map.Get.Doors.FirstOrDefault(x => x.GameObject == door.gameObject);

    public static Tesla GetTesla(this TeslaGate teslaGate) => Map.Get.Teslas.FirstOrDefault(x => x.GameObject == teslaGate.gameObject);

    public static Elevator GetElevator(this Lift lift) => Map.Get.Elevators.FirstOrDefault(x => x.GameObject == lift.gameObject);

    public static WorkStation GetWorkStation(this WorkstationController station) => Map.Get.WorkStations.FirstOrDefault(x => x.GameObject == station.gameObject);

    public static Synapse.Api.Camera GetSynapseCamera(this Camera079 camera) => Map.Get.Cameras.FirstOrDefault(x => x.GameObject == camera.gameObject);

    public static SynapseItem GetSynapseItem(this ItemBase itembase)
    {
        //If the List doesn't even contain the Serial then it is destroyed or a item with this ID was never spawned
        if (!SynapseItem.AllItems.ContainsKey(itembase.ItemSerial)) return null;

        var item = SynapseItem.AllItems[itembase.ItemSerial];

        //This is a simple fallback if the item is not registered
        if (item == null)
        {
            Synapse.Api.Logger.Get.Warn($"Found unregistered ItemBase with Serial: {itembase.ItemSerial} - Create a new SynapseItem Instance");
            return new SynapseItem(itembase);
        }

        return item;
    }

    public static SynapseItem GetSynapseItem(this ItemPickupBase pickupbase)
    {
        //If the List doesn't even contain the Serial then it is destroyed or a item with this ID was never spawned
        if (!SynapseItem.AllItems.ContainsKey(pickupbase.Info.Serial)) return null;

        var item = SynapseItem.AllItems[pickupbase.Info.Serial];

        //This is a simple fallback if the item is not registered
        if (item == null)
        {
            Synapse.Api.Logger.Get.Warn($"Found unregistered ItemPickup with Serial: {pickupbase.Info.Serial}");
            return new SynapseItem(pickupbase);
        }

        return item;
    }

    public static ScpReconfType GetScpReconfType(this PlayerStats.HitInfo info)
    {
        if (info.Tool == DamageTypes.Tesla) return ScpReconfType.Tesla;
        else if (info.Tool == DamageTypes.Nuke) return ScpReconfType.Nuke;
        else if (info.Tool == DamageTypes.Decont) return ScpReconfType.Decontamination;
        else if (info.IsPlayer)
        {
            var team = info.RHub.GetPlayer().Team;
            if ((int)team < 5 && (int)team > 0) 
                 return (ScpReconfType)team;
            else return ScpReconfType.ByUnknown;
        }
        else return ScpReconfType.Unspecified;
    }

    public static bool CanHarmScp(Player player,bool message = true)
    {
        if (player.Team == Team.SCP || player.CustomRole?.GetFriendsID().Any(x => x == (int)Team.SCP) == true)
        {
            if (message)
                player.GiveTextHint(Server.Get.Configs.synapseTranslation.ActiveTranslation.scpTeam);
            return false;
        }
        return true;
    }

    public static bool GetHarmPermission(Player attacker, Player victim, bool ignoreConfig = false)
    {
        try
        {
            var result = true;

            if (Map.Get.Round.RoundEnded && Server.Get.Configs.synapseConfiguration.AutoFF)
                result = true;
            else if (attacker == victim)
                result = true;
            else if (attacker.Team == Team.RIP || victim.Team == Team.RIP)
                result = false;
            else if (attacker.CustomRole == null && victim.CustomRole == null)
            {
                if (attacker.Team == Team.SCP && victim.Team == Team.SCP) result = false;

                var ff = Server.Get.FF;
                if (ignoreConfig)
                    ff = true;

                else if (!ff) result = attacker.Faction != victim.Faction;
            }
            else
            {
                if (attacker.CustomRole != null)
                {
                    if (attacker.CustomRole.GetFriendsID().Any(x => x == victim.TeamID))
                    {
                        result = false;
                        attacker.GiveTextHint(Server.Get.Configs.synapseTranslation.ActiveTranslation.sameTeam);
                    }
                }
                if (victim.CustomRole != null)
                {
                    if (victim.CustomRole.GetFriendsID().Any(x => x == attacker.TeamID))
                    {
                        result = false;
                        attacker.GiveTextHint(Server.Get.Configs.synapseTranslation.ActiveTranslation.sameTeam);
                    }
                }
            }

            Server.Get.Events.Player.InvokePlayerDamagePermissions(victim, attacker, ref result);

            return result;
        }
        catch (Exception e)
        {
            Synapse.Api.Logger.Get.Error($"Synapse-API: GetShootPermission  failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            return true;
        }
    }

    [Obsolete("Use SynapseExtensions.CanHarmScp() and check if it is false")]
    public static bool CanNotHurtByScp(Player player) => !CanHarmScp(player, false);
}