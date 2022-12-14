using System;
using HarmonyLib;
using LiteNetLib;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Patches;


[Patches]
[HarmonyPatch]
internal static class ServerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    public static void PreAuthentication(ConnectionRequest request)
    {
        try
        {
            if (!request.Data.EndOfData)
                return;

            var userId = "";
            if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                userId = CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].UserId;

            var ev = new PreAuthenticationEvent(request, userId);
            Synapse.Get<ServerEvents>().PreAuthentication.Raise(ev);
            
            if (ev.Rejected) return;

            if (ev.Allow)
            {
                Accept(request);
                return;
            }
            
            ev.Reject("No Reason");
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: Pre Authentication Event failed:\n" + ex);
            Accept(request);
        }
    }

    //This will block the Default Accept and you have to call the Accept below
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ConnectionRequest), nameof(ConnectionRequest.Accept))]
    public static bool OnDefaultAccept() => false;

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ConnectionRequest), nameof(ConnectionRequest.Accept))]
    public static NetPeer Accept(object instance)
    {
        //This should never be called since this is a Reverse Patch
        NeuronLogger.For<Synapse>().Warn("Accept Reverse");
        return null;
    }
}