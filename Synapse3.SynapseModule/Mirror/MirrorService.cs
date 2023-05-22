using System;
using System.Linq;
using System.Reflection;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles.SpawnData;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule;

public class MirrorService : Service
{
    public EntityStateMessage GetCustomVarMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour,
        Action<NetworkWriter> writeCustomData, bool writeDefaultObjectData = true)
        where TNetworkBehaviour : NetworkBehaviour
    {
        var index = behaviour.netIdentity.NetworkBehaviours.IndexOf(behaviour);//behaviour.ComponentIndex ?
        
        var writerData = new NetworkWriter();
        Compression.CompressVarUInt(writerData, 1u << index);
        int position1 = writerData.Position;
        writerData.WriteByte(0);

        int position2 = writerData.Position;
        if (!writeDefaultObjectData)
        {
            //By default, we don't update the ObjectData only when syncObjectDirtyBits is set to 0 do
            writerData.WriteULong(0);
        }
        else
        {
            behaviour.SerializeObjectsDelta(writerData);
        }

        if (writeCustomData == null)
        {
            //By default, we don't update the syncVarDirtyBits only when syncVarDirtyBits is set to 0
            writerData.WriteULong(0);
        }
        else
        {
            writeCustomData.Invoke(writerData);
        }

        int position3 = writerData.Position;
        writerData.Position = position1;
        byte num = (byte) (position3 - position2 & (int) byte.MaxValue);
        writerData.WriteByte(num);
        writerData.Position = position3;

        var msg = new EntityStateMessage()
        {
            netId = behaviour.netId,
            payload = writerData.ToArraySegment()
        };

        //writer.Reset();
        return msg;
    }

    public EntityStateMessage GetCustomVarMessage<TNetworkBehaviour, TValue>(TNetworkBehaviour behaviour, ulong id,
        TValue value) where TNetworkBehaviour : NetworkBehaviour => GetCustomVarMessage(behaviour,
        writer =>
        {
            writer.WriteULong(id);
            writer.Write(value);
        });

    public RpcMessage GetCustomRpcMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour, string methodName,
        Action<NetworkWriter> writeArguments)
        where TNetworkBehaviour : NetworkBehaviour
    {
        var writer = NetworkWriterPool.Get();
        writeArguments?.Invoke(writer);
        var msg = new RpcMessage()
        {
            netId = behaviour.netId,
            componentIndex = behaviour.ComponentIndex,
            functionHash = (ushort)(methodName.GetStableHashCode() & 65535),
            payload = new ArraySegment<byte>(writer.buffer.ToArray<byte>(), 0, writer.Position)
        };
        writer.Dispose();
        return msg;
    }

    /// <summary>
    /// Returns a SpawnMessage for an NetworkObject that can be modified
    /// </summary>
    public SpawnMessage GetSpawnMessage(NetworkIdentity identity)
    {
        var writer1 = NetworkWriterPool.Get();
        var writer2 = NetworkWriterPool.Get();
        var payload = NetworkServer.CreateSpawnMessagePayload(false, identity, writer1, writer2);
        var gameObject = identity.gameObject;
        writer1.Dispose();
        writer2.Dispose();
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
        var writer1 = NetworkWriterPool.Get();
        var writer2 = NetworkWriterPool.Get();
        var isOwner = identity.connectionToClient == playerToReceive.Connection;
        var payload = NetworkServer.CreateSpawnMessagePayload(isOwner, identity, writer1, writer2);
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
        writer1.Dispose();
        writer2.Dispose();
        return msg;
    }
}