using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Neuron.Core.Logging;
using RemoteAdmin;
using RemoteAdmin.Communication;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Permissions;
using Synapse3.SynapseModule.Player;
using Utils;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class RemoteAdminPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.ReceiveData), typeof(CommandSender), typeof(string))]
    public static bool OnReceiveData(CommandSender sender, string data)
    {
        try
        {
            var args = data.Split(' ');
            if (args.Length != 1) return false;
            if (!int.TryParse(args[0], out var number)) return false;

            var logRequest = number != 1;

            var viewHiddenBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var viewGlobalBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);

            if (sender is PlayerCommandSender playerSender && playerSender.ServerRoles.Staff)
            {
                viewHiddenBadges = true;
                viewGlobalBadges = true;
            }

            var players = new List<RemoteAdminPlayer>();

            foreach (var player in Synapse.Get<PlayerService>().Players)
            {
                if(!player.Hub.Ready) continue;

                var element = new RemoteAdminPlayer
                {
                    Player = player
                };
                players.Add(element);

                var badgeText = string.Empty;
                var overWatchText = player.OverWatch
                    ? "<link=RA_OverwatchEnabled><color=white>[</color><color=#03f8fc></color><color=white>]</color></link> "
                    : string.Empty;
                try
                {
                    if (string.IsNullOrWhiteSpace(player.ServerRoles.HiddenBadge) ||
                        (player.ServerRoles.GlobalHidden && viewGlobalBadges) ||
                        (!player.ServerRoles.GlobalHidden && viewHiddenBadges))
                    {
                        //Default Badge for GlobalRemoteAdmin
                        if (player.ServerRoles.RaEverywhere)
                        {
                            badgeText =
                                "<link=RA_RaEverywhere><color=white>[<color=#EFC01A></color><color=white>]</color></link> ";
                        }
                        //Default Badge for NW Staff
                        else if (player.ServerRoles.Staff)
                        {
                            badgeText =
                                "<link=RA_StudioStaff><color=white>[<color=#005EBC></color><color=white>]</color></link> ";
                        }
                        //Default Badge for User with RemoteAdmin
                        else if (player.RemoteAdminAccess)
                        {
                            badgeText = "<link=RA_Admin><color=white>[]</color></link> ";
                        }
                    }
                }
                catch{}

                element.Text = badgeText + overWatchText + "<color={RA_ClassColor}>(" + player.PlayerId + ") " +
                               player.NicknameSync.CombinedName.Replace("\n", "").Replace("RA_", string.Empty) +
                               "</color>";
            }

            sender.RaReply("$0 " + GenerateList(players,sender), true, logRequest, string.Empty);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: RemoteAdmin Receive List failed\n" + ex);
        }
        return false;
    }

    private static string GenerateList(List<RemoteAdminPlayer> players, CommandSender sender)
    {
        var remoteAdminGroups = Synapse.Get<PermissionService>().Groups.Select(x => new RemoteAdminGroup()
        {
            Name = x.Key,
            GroupId = x.Value.GroupId,
            Color = string.IsNullOrWhiteSpace(x.Value.Color) ||
                    string.Equals(x.Value.Color, "none", StringComparison.OrdinalIgnoreCase)
                ? "white"
                : x.Value.Color
        }).ToList();
        
        var overWatch = new RemoteAdminGroup();

        var config = Synapse.Get<SynapseConfigService>().PermissionConfiguration;

        if (config.OverWatchListDown)
        {
            foreach (var player in players.Where(x => x.Player.OverWatch).ToList())
            {
                overWatch.Members.Add(player);
                players.Remove(player);
            }
        }

        var text = "\n";
        var addNoneRow = false;
        
        if (config.BetterRemoteAdminList)
        {
            foreach (var player in players.ToList())
            {
                if (remoteAdminGroups.Any(x => x.GroupId == player.Player.SynapseGroup.GroupId))
                {
                    var group = remoteAdminGroups.FirstOrDefault(x => x.GroupId == player.Player.SynapseGroup.GroupId);
                    group.Members.Add(player);
                    players.Remove(player);
                }
            }

            if (players.Count > 0)
                addNoneRow = true;
        }

        text += "<size=0>(10001)</size> <size=20></color><color=blue>[ Synapse]</color></size>\n";

        foreach (var group in remoteAdminGroups)
        {
            
            if(group.Members.Count == 0) continue;

            text += "<size=0>(" + group.GroupId + ")</size> <size=20><color=" + group.Color + ">[" + group.Name +
                    "]</color></size>\n";

            foreach (var player in group.Members)
            {
                text += player.Text + "\n";
            }
        }

        if(addNoneRow)
            text += "<size=0>(0)</size> <size=20>[None]</size>\n";
        
        foreach (var player in players)
        {
            text += player.Text + "\n";
        }

        if(overWatch.Members.Count > 0)
            text += "<size=0>(10000)</size> <size=20></color><color=#03f8fc>[ OverWatch]</color></size>\n";
        
        foreach (var player in overWatch.Members)
        {
            text += player.Text + "\n";
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

        public List<RemoteAdminPlayer> Members { get; } = new ();
        
        public int GroupId { get; set; }
        
        public string Color { get; set; }
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
            
            //The Player Request Synapse itself
            if (args[1] == "10001.")
            {
                //If needed add a second page with the number
                    
                var synapseInfo = "<color=white>Welcome to <color=blue>Synapse3<color=white>";
                synapseInfo += "\nSynapse manages Plugins as well as improving many vanilla features";
                synapseInfo += "\n\n";
                synapseInfo += "\nCurrent Version: " + Synapse.GetVersion();
                synapseInfo += "\nBased Game Version: " + Synapse.BasedGameVersion;
                synapseInfo += "\nCurrent Game Version: " + GameCore.Version.VersionString;

                synapseInfo += "\n\n";
                synapseInfo += "\nDownload: <size=10><i><link=CP_USERID>https://github.com/SynapseSL/Synapse/releases</link></i></size>";
                synapseInfo += "\nDocs: <size=10><i><link=CP_IP>https://docs.synapsesl.xyz/</link></i></size>";
                synapseInfo += "\nDiscord: <size=10><i><link=CP_ID>https://discord.gg/uVtNr9Czng</link></i></size>";
                    
                synapseInfo += "\n\n<size=15>Created by Dimenzio, Helight, Wholesome & Flo";

                if (number == 0)
                    synapseInfo += " (and maybe UselessJavaDev MineTech)";
                    
                synapseInfo += "</color>";
                    
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId,
                    "https://github.com/SynapseSL/Synapse/releases");
                RaClipboard.Send(sender,RaClipboard.RaClipBoardType.Ip,"https://docs.synapsesl.xyz/");
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, "https://discord.gg/uVtNr9Czng");
                sender.RaReply("$1 " + synapseInfo, true, true, string.Empty);
                return false;
            }
            
            var requestSensitiveData = number == 0;

            var playerSender = sender as PlayerCommandSender;

            if (requestSensitiveData && playerSender != null &&
                !playerSender.ServerRoles.Staff &&
                !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess)) return false;

            var players = RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(args.Skip(1).ToArray()), 0, out var newArgs);
            if (players.Count == 0) return false;

            var allowedToSeeUserIds = PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL);

            if (playerSender != null &&
                (playerSender.ServerRoles.Staff || playerSender.ServerRoles.RaEverywhere))
                allowedToSeeUserIds = true;
            
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
                    playerIds += hub.playerId + ".";
                    
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

                sender.RaReply($"$1 " + text, true, true, string.Empty);
                return false;
            }

            var seeGamePlayData = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
            var player = players[0];
            var connection = player.networkIdentity.connectionToClient;

            if (playerSender != null)
                playerSender.Processor.GameplayData = seeGamePlayData;

            var message = "<color=white>";
            message += "Nickname: " + player.nicknameSync.CombinedName;
            message += $"\nPlayer ID: {player.playerId} <color=green><link=CP_ID></link></color>";
            RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, player.playerId.ToString());

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
                if(player.serverRoles.RaEverywhere)
                {
                    message +=
                        "\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>";
                }
                else if (player.serverRoles.Staff)
                {
                    message += "\nActive flag: Studio Staff";
                }
            }

            if (player.dissonanceUserSetup.AdministrativelyMuted)
            {
                message += "\nActive flag: <color=#F70D1A>SERVER MUTED</color>";
            }
            else if (player.characterClassManager.IntercomMuted)
            {
                message += "\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>";
            }
            
            if (player.characterClassManager.GodMode)
            {
                message += "\nActive flag: <color=#659EC7>GOD MODE</color>";
            }

            if (player.characterClassManager.NoclipEnabled)
            {
                message += "\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>";
            }

            if (player.serverRoles.NoclipReady)
            {
                message += "\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>";
            }

            if (player.serverRoles.DoNotTrack)
            {
                message += "\nActive flag: <color=#BFFF00>DO NOT TRACK</color>";
            }

            if (player.serverRoles.BypassMode)
            {
                message += "\nActive flag: <color=#BFFF00>BYPASS MODE</color>";
            }

            if (isAllowedToSee && player.serverRoles.RemoteAdmin)
            {
                message += "\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>";
            }

            if (player.serverRoles.OverwatchEnabled)
            {
                message += "\nActive flag: <color=#008080>OVERWATCH MODE</color>";
            }

            if (seeGamePlayData)
            {
                var sPlayer = player.GetSynapsePlayer();
                message += "\nClass: " + sPlayer.RoleName;
                message += "\nTeam: " + sPlayer.TeamName;
                message += $"\nHP: {sPlayer.Health}/{sPlayer.MaxHealth}";
                message += $"\nAHP: {sPlayer.ArtificialHealth}/{sPlayer.MaxArtificialHealth}";
                message += $"\nPosition: {sPlayer.Position}";
                message += $"\nRoom: {sPlayer.Room.Name}";
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RAUtils), nameof(RAUtils.ProcessPlayerIdOrNamesList))]
    public static bool OnGettingPlayers(ArraySegment<string> args, int startindex, out string[] newargs,
        bool keepemptyentries, out List<ReferenceHub> __result)
    {
        newargs = null;
        __result = new List<ReferenceHub>();
        try
        {
            newargs = args.Count > 1
                ? RAUtils.FormatArguments(args, startindex + 1).Split(new[] { ' ' },
                    keepemptyentries ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries)
                : null;

            if (args.Count <= startindex) return false;
            
            var info = args.At(startindex);
            
            if (info.Length == 0) return false;
            
            if (Synapse.Get<PlayerService>().TryGetPlayers(info, out var players))
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
}