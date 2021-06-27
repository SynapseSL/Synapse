using System;
using System.Text;
using HarmonyLib;
using LiteNetLib;
using Mirror.LiteNetLib4Mirror;
using Synapse.Api;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    internal class PreAuthenticationPatch
    {
         private static bool Prefix(ConnectionRequest request)
        {
            try
            {
                if (SynapseController.ClientManager.IsSynapseClientEnabled)
                {
                    var exists = request.Data.TryGetByte(out var packetId);
                    if (!exists)
                    {
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put(2);
                        request.RejectForce();
                        return false;
                    }
                    else
                    {
                        if (packetId == 5)
                        {
                            request.Data.GetByte();
                            if (!request.Data.TryGetBytesWithLength(out byte[] uidBytes) ||
                                !request.Data.TryGetBytesWithLength(out byte[] jwtBytes) ||
                                !request.Data.TryGetBytesWithLength(out byte[] nonceBytes))
                            {
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(2);
                                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                return false;
                            }

                            var uid = Encoding.UTF8.GetString(uidBytes);
                            var jwt = Encoding.UTF8.GetString(jwtBytes);
                            var nonce = Encoding.UTF8.GetString(nonceBytes);

                            var clientConnectionData = ClientConnectionData.DecodeJWT(jwt);

                            int num = CustomNetworkManager.slots;
                            if (LiteNetLib4MirrorCore.Host.ConnectedPeersCount < num)
                            {
                                if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                                    CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(uid);
                                else
                                    CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint,
                                        new PreauthItem(uid));

                                SynapseController.ClientManager.Clients[clientConnectionData.Uuid] = clientConnectionData;

                                request.Accept();
                                ServerConsole.AddLog(
                                    string.Format("Player {0} preauthenticated from endpoint {1}.", (object) uid,
                                        (object) request.RemoteEndPoint), ConsoleColor.Gray);
                                ServerLogs.AddLog(ServerLogs.Modules.Networking,
                                    string.Format("{0} preauthenticated from endpoint {1}.", (object) uid,
                                        (object) request.RemoteEndPoint), ServerLogs.ServerLogType.ConnectionUpdate,
                                    false);
                                CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
                            }
                            else
                            {
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 1);
                                request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                            }

                            return false;
                        }
                        else
                        {
                            request.Data._position = 0;
                            request.Data._offset = 0;
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Client: PreAuthentication Patch failed:\n{e}");
            }

            return true;
        }
    }
}