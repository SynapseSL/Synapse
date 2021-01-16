using Synapse.Api;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Synapse;
using Synapse.Api.Enum;

public static class SynapseExtensions
{
    public static Player GetPlayer(this MonoBehaviour mono) => mono?.gameObject.GetComponent<Player>();

    public static Player GetPlayer(this GameObject gameObject) => gameObject?.GetComponent<Player>();

    public static Player GetPlayer(this PlayableScps.PlayableScp scp) => scp?.Hub?.GetPlayer();

    public static Player GetPlayer(this CommandSender sender)
    {
        return sender?.SenderId == "SERVER CONSOLE" || sender?.SenderId == "GAME CONSOLE"
        ? Server.Get.Host
        : Server.Get.GetPlayer(sender.SenderId);
    }

    public static List<Player> GetPlayers(this RoleType role) => SynapseController.Server.Players.Where(x => x.RoleType == role).ToList();

    public static List<Player> GetPlayers(this Team team) => SynapseController.Server.Players.Where(x => x.Team == team).ToList();

    public static List<Player> GetPlayers(this Fraction fraction) => SynapseController.Server.Players.Where(x => x.Fraction == fraction).ToList();

    public static List<Player> GetPlayers(this RoleType[] roles) => SynapseController.Server.Players.Where(x => roles.Any(y => x.RoleType == y)).ToList();

    public static List<Player> GetPlayers(this Team[] teams) => SynapseController.Server.Players.Where(x => teams.Any(y => x.Team == y)).ToList();

    public static List<Player> GetPlayers(this Fraction[] fractions) => SynapseController.Server.Players.Where(x => fractions.Any(y => x.Fraction == y)).ToList();

    public static void RaMessage(this CommandSender sender, string message, bool success = true,
            RaCategory type = RaCategory.None)
    {
        var category = "";
        if (type != RaCategory.None)
            category = type.ToString();


        sender.RaReply($"{Assembly.GetCallingAssembly().GetName().Name}#" + message, success, true, category);
    }

    public static Generator GetGenerator(this Generator079 generator079) => Map.Get.Generators.FirstOrDefault(x => x.GameObject == generator079.gameObject);

    public static Synapse.Api.Door GetDoor(this Interactables.Interobjects.DoorUtils.DoorVariant door) => Map.Get.Doors.FirstOrDefault(x => x.GameObject == door.gameObject);

    public static Tesla GetTesla(this TeslaGate teslaGate) => Map.Get.Teslas.FirstOrDefault(x => x.GameObject == teslaGate.gameObject);

    public static Elevator GetElevator(this Lift lift) => Map.Get.Elevators.FirstOrDefault(x => x.GameObject == lift.gameObject);

    public static Synapse.Api.WorkStation GetWorkStation(this WorkStation station) => Map.Get.WorkStations.FirstOrDefault(x => x.GameObject == station.gameObject);

    public static Synapse.Api.Items.SynapseItem GetSynapseItem(this Inventory.SyncItemInfo info) => Map.Get.Items.FirstOrDefault(x => x.itemInfo == info);
    public static Synapse.Api.Items.SynapseItem GetSynapseItem(this Pickup pickup) => Map.Get.Items.FirstOrDefault(x => x.pickup == pickup);

    public static bool CanHarmScp(Player player)
    {
        if (player.CustomRole != null && player.CustomRole.GetFriends().Any(x => x == Team.SCP))
        {
            player.GiveTextHint(Server.Get.Configs.synapseTranslation.ActiveTranslation.scpTeam);
            return false;
        }
        return true;
    }

    public static bool CanNotHurtByScp(Player player) => player.Team == Team.SCP || player.CustomRole == null ? false : player.CustomRole.GetFriends().Any(x => x == Team.SCP);
}