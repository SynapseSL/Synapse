using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Patches;

//[Patches]
internal static class ServerPatches
{
    //TODO: Fix the Transpiler
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    public static void PreAuthentication(ConnectionRequest request)
    {
        try
        {
            var allow = true;
            var reason = "No Reason";

            if (!request.Data.EndOfData)
                return;

            var userId = "";

            if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                userId = CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].UserId;

            var ev = new PreAuthenticationEvent(request)
            {
                UserId = userId
            };

            Synapse.Get<ServerEvents>().PreAuthentication.Raise(ev);

            if (ev.Rejected) return;
            
            if (!ev.Allow)
            {
                ev.Reject("No Reason");
                return;
            }

            request.Accept();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: Pre Authentication Event failed:\n" + ex);
            request.Accept();
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        foreach (var code in codes.Select((x, i) => new { Value = x, Index = i }))
        {
            if (code.Value.opcode != OpCodes.Callvirt) continue;
            if (codes[code.Index + 2].opcode != OpCodes.Ldstr) continue;

            var strOperand = codes[code.Index + 2].operand as string;

            if (strOperand == "Player {0} preauthenticated from endpoint {1}.") code.Value.opcode = OpCodes.Nop;
        }

        return codes.AsEnumerable();
    }
    */
}