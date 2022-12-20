using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse3.SynapseModule.Dummy
{
    // code credit https://github.com/CedModV2/SCPSLAudioApi
    public class FakeConnection : NetworkConnectionToClient
    {
        RecyclablePlayerId id;
        public FakeConnection(RecyclablePlayerId id) : base(id.Value, false, 0f) { }

        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0) { }

        public override void Disconnect() 
        {
            id.Destroy();
        }
    }
}
