using Synapse.Api;
using Synapse.Client.Packets;

namespace Synapse.Client
{
    public class EventHandlers
    {
        internal EventHandlers()
        {
            Server.Get.Events.Round.RoundStartEvent += delegate
            {
                ClientPipeline.InvokeBroadcast(PipelinePacket.From(RoundStartPacket.ID, new byte[0]));
            };

            ClientPipeline.ClientConnectionCompleteEvent += delegate (Player player, ClientConnectionComplete ev)
            {
                if (Round.Get.RoundIsActive)
                {
                    SynapseController.ClientManager.SpawnController.SpawnLate(ev.Player);
                }
            };

            SynapseController.Server.Events.Round.RoundEndEvent += delegate
            {
                SynapseController.ClientManager.SpawnController.SpawnedObjects.Clear();
            };
        }
    }
}
