using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using RemoteAdmin;
using RemoteAdmin.Communication;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Permissions;
using Synapse3.SynapseModule.Permissions.RemoteAdmin;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Teams;
using Utils;
using VoiceChat;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
[Automatic]
[SynapsePatch("RAPlayerList", PatchType.RemoteAdmin)]
public static class RemoteAdminListPatch
{
    private static readonly PermissionService PermissionService;
    private static readonly RemoteAdminCategoryService CategoryService;
    private static readonly SynapseConfigService ConfigService;
    private static readonly ServerService ServerService;
    private static readonly DummyService DummyService;

    static RemoteAdminListPatch()
    {
        PermissionService = Synapse.Get<PermissionService>();
        CategoryService = Synapse.Get<RemoteAdminCategoryService>();
        ConfigService = Synapse.Get<SynapseConfigService>();
        ServerService = Synapse.Get<ServerService>();
        DummyService = Synapse.Get<DummyService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.ReceiveData), typeof(CommandSender), typeof(string))]
    public static bool OnReceiveData(RaPlayerList __instance, CommandSender sender, string data)
    {
        try
        {
            var args = data.Split(' ');
            if (args.Length != 3) return false;
            if (!int.TryParse(args[0], out var number) || !int.TryParse(args[1], out var sortingNumber)) return false;
            if (!Enum.IsDefined(typeof(RaPlayerList.PlayerSorting), sortingNumber)) return false;

            var logRequest = number != 1;
            var sortingType = (RaPlayerList.PlayerSorting)sortingNumber;
            var sortListDescending = args[2] == "1";

            var viewHiddenBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var viewGlobalBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);

            if (sender is PlayerCommandSender playerSender && playerSender.ServerRoles.Staff)
            {
                viewHiddenBadges = true;
                viewGlobalBadges = true;
            }

            var players = new List<RemoteAdminPlayer>();

            foreach (var hub in sortListDescending
                         ? __instance.SortPlayersDescending(sortingType)
                         : __instance.SortPlayers(sortingType))
            {
                var player = hub.GetSynapsePlayer();
                if (player.PlayerType == PlayerType.Dummy) continue;
                if (player.Hub.Mode != ClientInstanceMode.ReadyClient &&
                    player.Hub.Mode != ClientInstanceMode.Host) continue;

                var element = new RemoteAdminPlayer
                {
                    Player = player
                };
                players.Add(element);

                var badgeText = __instance.GetPrefix(hub, viewHiddenBadges, viewGlobalBadges);
                var overWatchText = player.OverWatch ? RaPlayerList.OverwatchBadge : string.Empty;

                element.Text = badgeText + overWatchText + "<color={RA_ClassColor}>(" +
                               player.PlayerId + ") " +
                               player.NicknameSync.CombinedName.Replace("\n", "").Replace("RA_", string.Empty) +
                               "</color>";

                if (!string.IsNullOrWhiteSpace(player.CustomRemoteAdminBadge))
                    element.Text = player.CustomRemoteAdminBadge + " " + element.Text;
            }

            sender.RaReply("$0 " + GenerateList(players, sender), true, logRequest, string.Empty);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: RemoteAdmin Receive List failed\n" + ex);
        }

        return false;
    }

    private static string GenerateList(List<RemoteAdminPlayer> players, CommandSender sender)
    {
        var remoteAdminGroups = PermissionService.Groups.Select(x => new RemoteAdminGroup
        {
            Name = x.Key,
            GroupId = x.Value.GroupId,
            Color = string.IsNullOrWhiteSpace(x.Value.Color) ||
                    string.Equals(x.Value.Color, "none", StringComparison.OrdinalIgnoreCase)
                ? "white"
                : x.Value.Color
        }).ToList();

        var config = ConfigService.PermissionConfiguration;

        var text = "\n";
        var categories = CategoryService.RemoteAdminCategories;

        var groupPlayers = players.ToList();
        if (config.BetterRemoteAdminList)
        {
            foreach (var player in groupPlayers.ToList())
            {
                if (remoteAdminGroups.Any(x => x.GroupId == player.Player.SynapseGroup.GroupId))
                {
                    var group = remoteAdminGroups.FirstOrDefault(x => x.GroupId == player.Player.SynapseGroup.GroupId);
                    if (group == null) continue;

                    group.Members.Add(player);
                    groupPlayers.Remove(player);
                }
            }
        }

        var senderPlayer = sender.GetSynapsePlayer();

        foreach (var category in categories)
        {
            if (!category.CanSeeCategory(senderPlayer) || !category.DisplayOnTop) continue;

            var color = category.Attribute.Color;

            if (string.Equals(color, "rainbow", StringComparison.OrdinalIgnoreCase))
            {
                var colors = ServerService.Colors;
                color = colors.ElementAt(Random.Range(0, colors.Count)).Value;
            }

            text +=
                $"<align=center><size=0>({category.Attribute.Id})</size> <size={category.Attribute.Size}></color><color={color}>{category.Attribute.Name}</color></size>\n</align>";

            if (!config.DisplayPlayerMultipleTimes) continue;

            foreach (var player in category.GetPlayers() ?? new List<SynapsePlayer>())
            {
                var raPlayer = players.FirstOrDefault(x => x.Player == player);
                if (raPlayer != null)
                    text += raPlayer.Text + "\n";
            }
        }

        foreach (var group in remoteAdminGroups)
        {
            if (group.Members.Count == 0) continue;

            var color = group.Color;
            if (string.Equals(color, "rainbow", StringComparison.OrdinalIgnoreCase))
            {
                var colors = ServerService.Colors;
                color = colors.ElementAt(Random.Range(0, colors.Count)).Value;
            }
            else
            {
                color = ServerService.GetColorHexCode(color);
            }

            text += "<align=center><size=0>(-" + group.GroupId + ")</size> <size=20><color=" + color + ">[" +
                    group.Name +
                    "]</color></size>\n</align>";

            foreach (var player in group.Members)
            {
                text += player.Text + "\n";
            }
        }

        if (config.BetterRemoteAdminList && groupPlayers.Any())
            text += "<align=center><size=0>(default)</size> <size=20>[Default Player]</size></align>\n";

        foreach (var player in groupPlayers)
        {
            text += player.Text + "\n";
        }

        var dummies = DummyService.Dummies //Add the dummy
            .Where(p => p.RaVisible)
            .Select(d => new RemoteAdminPlayer()
            {
                Player = d.Player,
                Text = $"<color={{RA_ClassColor}}>({d.Player.PlayerId}) {d.Player.DisplayName}</color>"
            });

        if (dummies.Any())
            text += "<align=center><size=0>(dummy)</size> <size=20>[Dummy]</size></align>\n";

        foreach (var dummy in dummies)
        {
            text += dummy.Text + "\n";
        }

        foreach (var category in categories)
        {
            if (!category.CanSeeCategory(senderPlayer) || category.DisplayOnTop) continue;

            var color = category.Attribute.Color;

            if (string.Equals(color, "rainbow", StringComparison.OrdinalIgnoreCase))
            {
                var colors = ServerService.Colors;
                color = colors.ElementAt(Random.Range(0, colors.Count)).Value;
            }

            text +=
                $"<align=center><size=0>({category.Attribute.Id})</size> <size={category.Attribute.Size}></color><color={color}>{category.Attribute.Name}</color></size></align>\n";

            if (!config.DisplayPlayerMultipleTimes) continue;

            foreach (var player in category.GetPlayers() ?? new List<SynapsePlayer>())
            {
                var raPlayer = players.FirstOrDefault(x => x.Player == player);
                if (raPlayer != null)
                    text += raPlayer.Text + "\n";
            }
        }

        return text;
    }

    private class RemoteAdminPlayer
    {
        public SynapsePlayer Player { get; set; }

        public string Text { get; set; }
    }

    private class RemoteAdminGroup
    {
        public string Name { get; set; }

        public List<RemoteAdminPlayer> Members { get; } = new();

        public uint GroupId { get; set; }

        public string Color { get; set; }
    }
}

[Automatic]
[SynapsePatch("RAPlayerDataRequest", PatchType.RemoteAdmin)]
public static class RemoteAdminPlayerDataRequestPatch
{
    private static readonly RemoteAdminCategoryService CategoryService;
    private static readonly TeamService TeamService;

    static RemoteAdminPlayerDataRequestPatch()
    {
        CategoryService = Synapse.Get<RemoteAdminCategoryService>();
        TeamService = Synapse.Get<TeamService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    public static bool OnRequestPlayer(CommandSender sender, string data)
    {
        try
        {
            var args = data.Split(' ');
            if (args.Length != 2) return false;
            if (!int.TryParse(args[0], out var number)) return false;

            var arg = args[1].Split('.')[0];
            if (int.TryParse(arg, out var categoryId))
            {
                var category = CategoryService.GetCategory(categoryId);
                if (category != null && category.CanSeeCategory(sender.GetSynapsePlayer()))
                {
                    sender.RaReply("$1 " + category.GetInfo(sender, number == 0), true, true, string.Empty);
                    return false;
                }
            }

            var requestSensitiveData = number == 0;
            var playerSender = sender as PlayerCommandSender;

            if (requestSensitiveData && playerSender != null &&
                !playerSender.ServerRoles.Staff &&
                !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess)) return false;

            var players =
                RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(args.Skip(1).ToArray()), 0, out _);
            if (players.Count == 0) return false;

            var allowedToSeeUserIds = PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL) ||
                                      playerSender != null &&
                                      (playerSender.ServerRoles.Staff || playerSender.ServerRoles.RaEverywhere);

            if (players.Count > 1)
            {
                var text = "<color=white>";
                text += "Selecting multiple players:";
                text += "\nPlayer ID: <color=green><link=CP_ID></link></color>";
                text += "\nIP Address: " + (requestSensitiveData
                    ? "<color=green><link=CP_IP></link></color>"
                    : "[REDACTED]");
                text += "\nUser ID: " +
                        (allowedToSeeUserIds ? "<color=green><link=CP_USERID></link></color>" : "[REDACTED]");
                text += "</color>";

                var playerIds = "";
                var playerIps = "";
                var userIds = "";

                foreach (var hub in players)
                {
                    playerIds += hub.PlayerId + ".";

                    if (requestSensitiveData)
                    {
                        playerIps += (hub.networkIdentity.connectionToClient.IpOverride != null
                            ? hub.networkIdentity.connectionToClient.OriginalIpAddress
                            : hub.networkIdentity.connectionToClient.address) + ",";
                    }

                    if (allowedToSeeUserIds)
                    {
                        userIds += hub.characterClassManager.UserId + ".";
                    }
                }

                if (playerIds.Length > 0)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, playerIds);
                }

                if (playerIps.Length > 0)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, playerIps);
                }

                if (userIds.Length > 0)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, userIds);
                }

                sender.RaReply("$1 " + text, true, true, string.Empty);
                return false;
            }

            var seeGamePlayData = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
            var player = players[0];
            var connection = player.networkIdentity.connectionToClient;

            if (playerSender != null)
                playerSender.ReferenceHub.queryProcessor.GameplayData = seeGamePlayData;

            var message = "<color=white>";
            message += "Nickname: " + player.nicknameSync.CombinedName;
            message += $"\nPlayer ID: {player.PlayerId} <color=green><link=CP_ID></link></color>";
            RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, player.PlayerId.ToString());

            if (connection == null)
            {
                message += "\nIP Address: null";
            }
            else if (requestSensitiveData)
            {
                message += "\nIP Address: " + connection.address + " ";
                if (connection.IpOverride != null)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connection.OriginalIpAddress ?? "");
                    message += " [routed via " + connection.OriginalIpAddress + "]";
                }
                else
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connection.address ?? "");
                }

                message += " <color=green><link=CP_IP></link></color>";
            }
            else
            {
                message += "\nIP Address: [REDACTED]";
            }

            var id = string.IsNullOrWhiteSpace(player.characterClassManager.UserId)
                ? "(none)"
                : player.characterClassManager.UserId + " <color=green><link=CP_USERID></link></color>";


            message += "\nUser ID: " + (allowedToSeeUserIds ? id : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");

            if (allowedToSeeUserIds)
            {
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, player.characterClassManager.UserId ?? "");
                if (player.characterClassManager.SaltedUserId != null &&
                    player.characterClassManager.SaltedUserId.Contains("$"))
                {
                    message += "\nSalted User ID: " + player.characterClassManager.SaltedUserId;
                }

                if (!string.IsNullOrWhiteSpace(player.characterClassManager.UserId2))
                {
                    message += "\nUser ID 2: " + player.characterClassManager.UserId2;
                }
            }

            message += "\nServer role: " + player.serverRoles.GetColoredRoleString();
            var seeHidden = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var seeGlobal = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);

            if (playerSender != null && playerSender.ServerRoles.Staff)
            {
                seeHidden = true;
                seeGlobal = true;
            }

            var hasHiddenBadge = !string.IsNullOrWhiteSpace(player.serverRoles.HiddenBadge);
            var isAllowedToSee = !hasHiddenBadge || (player.serverRoles.GlobalHidden && seeGlobal) ||
                                 (!player.serverRoles.GlobalHidden && seeHidden);

            if (isAllowedToSee)
            {
                if (hasHiddenBadge)
                {
                    message += "\n<color=#DC143C>Hidden role: </color>" + player.serverRoles.HiddenBadge;
                    message += "\n<color=#DC143C>Hidden role type: </color>" +
                               (player.serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL");
                }

                if (player.serverRoles.RaEverywhere)
                {
                    message +=
                        "\nStudio Status: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>";
                }
                else if (player.serverRoles.Staff)
                {
                    message += "\nStudio Status: <color=#94B9CF>Studio Staff</color>";
                }
            }

            var flags = (int)VoiceChatMutes.GetFlags(players[0]);
            if (flags != 0)
            {
                message += "\nMUTE STATUS:";

                foreach (var mute in Enum.GetValues(typeof(VcMuteFlags)))
                {
                    var muteValue = (int)mute;
                    if (muteValue != 0 & (flags & muteValue) == muteValue)
                    {
                        message += " <color=#F70D1A>";
                        message += (VcMuteFlags)muteValue;
                        message += "</color>";
                    }
                }
            }

            message += "\nActive flag(s):";

            if (player.characterClassManager.GodMode)
            {
                message += " <color=#659EC7>[GOD MODE]</color>";
            }

            if (player.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip))
            {
                message += " <color=#DC143C>[NOCLIP ENABLED]</color>";
            }
            else if (FpcNoclip.IsPermitted(player))
            {
                message += " <color=#E52B50>[NOCLIP UNLOCKED]</color>";
            }

            if (player.serverRoles.DoNotTrack)
            {
                message += " <color=#BFFF00>[DO NOT TRACK]</color>";
            }

            if (player.serverRoles.BypassMode)
            {
                message += " <color=#BFFF00>[BYPASS MODE]</color>";
            }

            if (isAllowedToSee && player.serverRoles.RemoteAdmin)
            {
                message += " <color=#43C6DB>[RA AUTHENTICATED]</color>";
            }

            if (player.serverRoles.IsInOverwatch)
            {
                message += " <color=#008080>[OVERWATCH MODE]</color>";
            }
            else if (seeGamePlayData)
            {
                message += "\nClass: ";
                var sPlayer = player.GetSynapsePlayer();
                switch (sPlayer.RoleType)
                {
                    case RoleTypeId.None:
                    case RoleTypeId.Spectator:
                        message += sPlayer.RoleName;
                        break;

                    case RoleTypeId.Scp079:
                        message += sPlayer.RoleName;
                        message += " <color=blue>[AP: " + sPlayer.MainScpController.Scp079.Energy +
                            "]</color>";
                        message += "\nTeam: " + TeamService.GetTeamName(sPlayer.TeamID);
                        message += "\nCamera: " + sPlayer.MainScpController.Scp079.Camera.Name;
                        message += "\nRoom: " + sPlayer.MainScpController.Scp079.Camera.Room.Name;

                        break;
                    default:
                        message += sPlayer.RoleName;
                        message += " <color=#fcff99>[HP: " + CommandProcessor.GetRoundedStat<HealthStat>(player) +
                                   "]</color>";
                        message += " <color=green>[AHP: " + CommandProcessor.GetRoundedStat<AhpStat>(player) +
                                   "]</color>";
                        message += " <color=#977dff>[HS: " + CommandProcessor.GetRoundedStat<HumeShieldStat>(player) +
                                   "]</color>";
                        message += "\nTeam: " + TeamService.GetTeamName(sPlayer.TeamID);
                        message += "\nPosition: " + sPlayer.Position;
                        message += "\nRoom: " + sPlayer.Room.Name;
                        break;
                }
            }
            else
            {
                message += "\n<color=#D4AF37>Some fields were hidden. GameplayData permission required.</color>";
            }

            message += "</color>";
            sender.RaReply("$1 " + message, true, true, string.Empty);
            RaPlayerQR.Send(sender, false,
                string.IsNullOrWhiteSpace(player.characterClassManager.UserId)
                    ? "(no User ID)"
                    : player.characterClassManager.UserId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: RemoteAdmin Receive List failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("SelectPlayer", PatchType.RemoteAdmin)]
public static class SelectPlayerPatch
{
    private static readonly PlayerService PlayerService;
    static SelectPlayerPatch() => PlayerService = Synapse.Get<PlayerService>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RAUtils), nameof(RAUtils.ProcessPlayerIdOrNamesList))]
    public static bool OnGettingPlayers(ArraySegment<string> args, int startindex, out string[] newargs,
        bool keepEmptyEntries, out List<ReferenceHub> __result)
    {
        try
        {
            //TODO: Update this
            newargs = null;
            __result = new List<ReferenceHub>();
            try
            {
                newargs = args.Count > 1
                    ? RAUtils.FormatArguments(args, startindex + 1).Split(new[] { ' ' },
                        keepEmptyEntries ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries)
                    : null;

                if (args.Count <= startindex) return false;

                var info = args.At(startindex);

                if (info.Length == 0) return false;
                
                if (PlayerService.TryGetPlayers(info, out var players))
                {
                    __result = players.Select(x => x.Hub).ToList();
                }
            }
            catch (Exception ex)
            {
                NeuronLogger.For<Synapse>().Error("Sy3 API: RemoteAdmin GetPlayers failed\n" + ex);
            }

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("RA Select Player Patch failed\n" + ex);
            newargs = null;
            __result = null;
            return true;
        }
    }
}
#endif