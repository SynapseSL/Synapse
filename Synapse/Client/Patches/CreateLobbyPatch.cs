using System;
using System.Collections.Generic;
using HarmonyLib;
using MEC;
using Mirror.LiteNetLib4Mirror;
using UnityEngine.SceneManagement;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager._CreateLobby))]
    internal static class CreateLobbyPatch
    {
        private static bool Prefix(CustomNetworkManager __instance, ref IEnumerator<float> __result)
        {
            if (!SynapseController.ClientManager.IsSynapseClientEnabled) return true;

            __result = CreateLobby(__instance);
            return false;
        }

        private static IEnumerator<float> CreateLobby(CustomNetworkManager manager)
        {
            //Game Version Check
            if (manager.GameFilesVersion != CustomNetworkManager._expectedGameFilesVersion)
            {
                ServerConsole.AddLog("This source code file is made for different version of the game!", ConsoleColor.Gray);
                ServerConsole.AddLog("Please validate game files integrity using steam!", ConsoleColor.Gray);
                ServerConsole.AddLog("Aborting server startup.", ConsoleColor.Gray);
                yield break;
            }
            ServerConsole.AddLog("Game version: " + GameCore.Version.VersionString, ConsoleColor.Gray);
            ServerConsole.AddLog("Synapse version: " + SynapseController.SynapseVersion, ConsoleColor.Gray);
            if (GameCore.Version.PrivateBeta)
                ServerConsole.AddLog("PRIVATE BETA VERSION - DO NOT SHARE", ConsoleColor.Gray);

            yield return Timing.WaitForOneFrame;

            ServerConsole.AddLog("Synapse Verification is ENABLED");

            //Query
            if (manager._queryEnabled)
            {
                manager._queryPort = (int)LiteNetLib4MirrorTransport.Singleton.port +
                    GameCore.ConfigFile.ServerConfig.GetInt("query_port_shift", 0);
                ServerConsole.AddLog("Query port will be enabled on port " + manager._queryPort + " TCP.", ConsoleColor.Gray);
                CustomNetworkManager._queryserver = new QueryServer(manager._queryPort,
                    GameCore.ConfigFile.ServerConfig.GetBool("query_use_IPv6", true));
                CustomNetworkManager._queryserver.StartServer();
            }
            else ServerConsole.AddLog("Query port disabled in config!", ConsoleColor.Gray);

            ServerConsole.AddLog("Starting Synapse server...", ConsoleColor.Gray);

            //Server IP
            if (GameCore.ConfigFile.HosterPolicy.GetString("server_ip", "none") != "none")
            {
                CustomNetworkManager.Ip = GameCore.ConfigFile.HosterPolicy.GetString("server_ip", "none");
                ServerConsole.AddLog("Server IP set to " + CustomNetworkManager.Ip + " by your hosting provider.", ConsoleColor.Gray);
            }
            else if (ServerStatic.IsDedicated)
            {
                if (GameCore.ConfigFile.ServerConfig.GetString("server_ip", "auto") != "auto")
                {
                    CustomNetworkManager.Ip = GameCore.ConfigFile.ServerConfig.GetString("server_ip", "auto");
                    ServerConsole.AddLog("Custom config detected. Your game-server IP will be " + CustomNetworkManager.Ip, ConsoleColor.Gray);
                }
                else CustomNetworkManager.Ip = "127.0.0.1";
                //I don't think we need to get the IP with our current system and I would have to add a new reference, but normally would be here a request to the sl api to get the own ip
            }
            else CustomNetworkManager.Ip = "127.0.0.1";
            ServerConsole.Ip = CustomNetworkManager.Ip;

            ServerConsole.AddLog("Initializing Synapse game server...", ConsoleColor.Gray);

            if (!ServerStatic.IsDedicated) yield break;

            //IPv4 Bind
            if (GameCore.ConfigFile.HosterPolicy.GetString("ipv4_bind_ip", "none") != "none")
            {
                LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress =
                    GameCore.ConfigFile.HosterPolicy.GetString("ipv4_bind_ip", "0.0.0.0");

                if (LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress == "0.0.0.0")
                    ServerConsole.AddLog("Server starting at all IPv4 addresses and port " +
                        LiteNetLib4MirrorTransport.Singleton.port + " - set by your hosting provider.", ConsoleColor.Gray);

                else ServerConsole.AddLog(string.Concat(new object[]
                {
                    "Server starting at IPv4 ",
                    LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress,
                    " and port ",
                    LiteNetLib4MirrorTransport.Singleton.port,
                    " - set by your hosting provider."
                }), ConsoleColor.Gray);

            }
            else
            {
                LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress =
                    GameCore.ConfigFile.ServerConfig.GetString("ipv4_bind_ip", "0.0.0.0");

                if (LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress == "0.0.0.0")
                    ServerConsole.AddLog("Server starting at all IPv4 addresses and port " +
                        LiteNetLib4MirrorTransport.Singleton.port, ConsoleColor.Gray);

                else ServerConsole.AddLog(string.Concat(new object[]
                    {
                    "Server starting at IPv4 ",
                    LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress,
                    " and port ",
                    LiteNetLib4MirrorTransport.Singleton.port
                    }), ConsoleColor.Gray);
            }

            //IPv6
            if (GameCore.ConfigFile.HosterPolicy.GetString("ipv6_bind_ip", "none") != "none")
            {
                LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress = GameCore.ConfigFile.HosterPolicy.GetString("ipv6_bind_ip", "::");

                if (LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress == "::")
                    ServerConsole.AddLog("Server starting at all IPv6 addresses and port " +
                        LiteNetLib4MirrorTransport.Singleton.port + " - set by your hosting provider.", ConsoleColor.Gray);

                else ServerConsole.AddLog(string.Concat(new object[]
                    {
                    "Server starting at IPv6 ",
                    LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress,
                    " and port ",
                    LiteNetLib4MirrorTransport.Singleton.port,
                    " - set by your hosting provider."
                    }), ConsoleColor.Gray);
            }
            else
            {
                LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress = GameCore.ConfigFile.ServerConfig.GetString("ipv6_bind_ip", "::");

                if (LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress == "::")
                    ServerConsole.AddLog("Server starting at all IPv6 addresses and port " +
                        LiteNetLib4MirrorTransport.Singleton.port, ConsoleColor.Gray);

                else ServerConsole.AddLog(string.Concat(new object[]
                    {
                    "Server starting at IPv6 ",
                    LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress,
                    " and port ",
                    LiteNetLib4MirrorTransport.Singleton.port
                    }), ConsoleColor.Gray); 
            }

            //Start
            manager.StartHost();

            while (SceneManager.GetActiveScene().name != "Facility")
                yield return Timing.WaitForOneFrame;

            ServerConsole.AddLog("Level loaded.", ConsoleColor.Gray);

            ServerConsole.AddLog("ONLY players with the Synapse Client can join your Server", ConsoleColor.Red);
            yield break;
        }
    }
}
