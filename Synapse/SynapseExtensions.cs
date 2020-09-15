using Synapse.Api;
using UnityEngine;
using CommandSystem;
using Synapse.Api.Enums;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public static class SynapseExtensions
{
    public static Player GetPlayer(this MonoBehaviour mono) => mono.gameObject.GetComponent<Player>();

    public static Player GetPlayer(this GameObject gameObject) => gameObject.GetComponent<Player>();

    public static Player GetPlayer(this PlayableScps.PlayableScp scp) => scp.Hub.GetPlayer();

    public static List<Player> GetPlayers(this RoleType role) => SynapseController.Server.Players.Where(x => x.Role == role).ToList();

    public static List<Player> GetPlayers(this Team team) => SynapseController.Server.Players.Where(x => x.Team == team).ToList();

    public static List<Player> GetPlayers(this Fraction fraction) => SynapseController.Server.Players.Where(x => x.Fraction == fraction).ToList();

    public static List<Player> GetPlayers(this RoleType[] roles) => SynapseController.Server.Players.Where(x => roles.Any(y => x.Role == y)).ToList();

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
}