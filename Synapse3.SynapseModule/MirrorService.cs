using System;
using Mirror;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule;

public class MirrorService : Service
{
    public UpdateVarsMessage GetCustomVarMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour,
        Action<NetworkWriter> writeCustomData, bool writeDefaultObjectData = true)
        where TNetworkBehaviour : NetworkBehaviour
    {
        var writer = NetworkWriterPool.GetWriter();
        
        var pos = writer.position;
        writer.WriteByte((byte)behaviour.ComponentIndex);

        var pos1 = writer.Position;
        //This is just a placeholder and contains the length of the Data for this
        //Component since you could send multiple Component changes with just one message
        writer.WriteInt32(0);
        var pos2 = writer.Position;

        //This will write the SyncObject Data can be used the modify Synced List or similar
        if (writeDefaultObjectData)
            behaviour.SerializeObjectsDelta(writer);

        writeCustomData?.Invoke(writer);

        var pos3 = writer.Position;
        writer.Position = pos1;
        writer.WriteInt32(pos3-pos2);
        writer.Position = pos3;
                    
        if (behaviour.syncMode == SyncMode.Observers)
        {
            var segment = writer.ToArraySegment();
            var counter = writer.Position - pos;
            writer.WriteBytes(segment.Array, pos, counter);
        }

        var msg = new UpdateVarsMessage()
        {
            netId = behaviour.netId,
            //If I use the writer directly and recycle it the array will be reused afterwards and a wrong payload will be send
            payload = new ArraySegment<byte>(writer.buffer.ToArray<byte>(), 0, writer.length)
        };
        writer.Reset();
        NetworkWriterPool.Recycle(writer);
        return msg;
    }

    public UpdateVarsMessage GetCustomVarMessage<TNetworkBehaviour, TValue>(TNetworkBehaviour behaviour, ulong id,
        TValue value) where TNetworkBehaviour : NetworkBehaviour => GetCustomVarMessage(behaviour,
        writer =>
        {
            writer.WriteUInt64(id);
            writer.Write(value);
        });

    public RpcMessage GetCustomRpcMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour, string methodName,
        Action<NetworkWriter> writeArguments)
        where TNetworkBehaviour : NetworkBehaviour
    {
        var writer = NetworkWriterPool.GetWriter();
        writeArguments?.Invoke(writer);
        var msg = new RpcMessage()
        {
            netId = behaviour.netId,
            componentIndex = behaviour.ComponentIndex,
            functionHash = typeof(TNetworkBehaviour).FullName.GetStableHashCode() * 503 + methodName.GetStableHashCode(),
            payload = new ArraySegment<byte>(writer.buffer.ToArray<byte>(), 0, writer.length)
        };
        writer.Reset();
        NetworkWriterPool.Recycle(writer);
        return msg;
    }

    /// <summary>
    /// Returns a SpawnMessage for an NetworkObject that can be modified
    /// </summary>
    public SpawnMessage GetSpawnMessage(NetworkIdentity identity)
    {
        var writer = NetworkWriterPool.GetWriter();
        var writer2 = NetworkWriterPool.GetWriter();
        var payload = NetworkServer.CreateSpawnMessagePayload(false, identity, writer, writer2);
        var gameObject = identity.gameObject;
        return new SpawnMessage
        {
            netId = identity.netId,
            isLocalPlayer = false,
            isOwner = false,
            sceneId = identity.sceneId,
            assetId = identity.assetId,
            position = gameObject.transform.position,
            rotation = gameObject.transform.rotation,
            scale = gameObject.transform.localScale,
            payload = payload
        };
    }

    public SpawnMessage GetSpawnMessage(NetworkIdentity identity, SynapsePlayer playerToReceive)
    {
        var writer = NetworkWriterPool.GetWriter();
        var writer2 = NetworkWriterPool.GetWriter();
        var isOwner = identity.connectionToClient == playerToReceive.Connection;
        var payload = NetworkServer.CreateSpawnMessagePayload(isOwner, identity, writer, writer2);
        var transform = identity.transform;
        var msg = new SpawnMessage()
        {
            netId = identity.netId,
            isLocalPlayer = playerToReceive.NetworkIdentity == identity,
            isOwner = isOwner,
            sceneId = identity.sceneId,
            assetId = identity.assetId,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale,
            payload = payload
        };
        return msg;
    }
}