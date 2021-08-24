using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Synapse.Patches.EventsPatches.ServerPatches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    internal class PreAuthenticationPatch
    {
        private static void Postfix(ConnectionRequest request)
        {
            try
            {
                var allow = true;
                var reason = "No Reason";

                if (!request.Data.EndOfData) return;

                var userId = CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].UserId;
                SynapseController.Server.Events.Server.InvokePreAuthenticationEvent(userId, ref allow, ref reason, request);

                if (allow)
                {
                    request.Accept();
                    return;
                }

                var data = new NetDataWriter();
                data.Put((byte)10);
                data.Put(reason);
                request.RejectForce(data);
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: PreAuthenticationFailed failed!!\n{e}");
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            foreach (var code in codes.Select((x, i) => new {Value = x, Index = i}))
            {
                if (code.Value.opcode != OpCodes.Callvirt) continue;
                if (codes[code.Index + 2].opcode != OpCodes.Ldstr) continue;

                var strOperand = codes[code.Index + 2].operand as string;

                if (strOperand == "Player {0} preauthenticated from endpoint {1}.") code.Value.opcode = OpCodes.Nop;
            }

            return codes.AsEnumerable();
        }
    }
}