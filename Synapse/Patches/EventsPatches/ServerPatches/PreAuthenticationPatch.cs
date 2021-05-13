using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using Synapse.Api;
using Synapse.Database;

namespace Synapse.Patches.EventsPatches.ServerPatches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    internal class PreAuthenticationPatch
    {
        private static void Postfix(CustomLiteNetLib4MirrorTransport __instance, ConnectionRequest request)
        {
            var allow = true;
            var reason = "No Reason";

            if (!request.Data.EndOfData) return;

            var userId = CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].UserId;
            SynapseController.Server.Events.Server.InvokePreAuthenticationEvent(userId, ref allow, ref reason, request);

            //Since Database is filebased, delay should be minimal, making lags unlikely
            if (allow && PunishmentRepository.Enabled)
            {
                Logger.Get.Info("=== PUNISHMENT PROXY ENABLED ===");
                var currentBan = DatabaseManager.PunishmentRepository.GetCurrentPunishments(userId)
                    .Where(x => x.Type == PunishmentType.Ban).FirstOrDefault();
                if (currentBan != null)
                {
                    allow = false;
                    reason = currentBan.ReasonString();
                }
            }

            if (allow)
            {
                request.Accept();
                return;
            }

            var data = new NetDataWriter();
            data.Put((byte) 10);
            data.Put(reason);
            request.RejectForce(data);
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