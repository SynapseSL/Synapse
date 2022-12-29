using Mirror;
using System;

namespace Synapse3.SynapseModule.Dummy;
    
// code credit https://github.com/CedModV2/SCPSLAudioApi
public class FakeConnection : NetworkConnectionToClient
{
    public RecyclablePlayerId FakePlayerId { get; }

    public FakeConnection(RecyclablePlayerId id) : base(id.Value, false, 0f)
    {
        FakePlayerId = id;
    }

    public override string address => "localhost";

    public override void Send(ArraySegment<byte> segment, int channelId = 0) { }
}
