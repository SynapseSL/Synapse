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
    /// <summary>
    /// Use to spoof information to a specific client by sending the result of this method over its connection
    /// The resulte can be send to a client using the <see cref="SynapsePlayer.SendNetworkMessage{TNetworkMessage}(TNetworkMessage, int)"/>.
    /// <exemple> 
    /// <para>To write custom varaible data and keep a default refresh of ObjectData:</para>
    /// <code>
    /// GetCustomVarMessage(behaviour, writer => MyLambdaWriter, writer => behaviour.SerializeObjectsDelta(writer));
    /// </code>
    /// To write custome object data and keep a default refresh of variable
    /// <code>
    /// GetCustomVarMessage(behaviour, writer => behaviour.SerializeSyncVars(writer), writer => MyLambdaWriter());
    /// </code>
    /// </exemple>
    /// By default the ObjectData and Variable are not update
    /// </summary>
    /// <param name="behaviour">The target NetWorkBehaviour who will see this value change on the client side of the receving player</param>
    public EntityStateMessage GetCustomVarMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour,
        Action<NetworkWriter> writeCustomVarData = null, Action<NetworkWriter> writCustomObjectData = null)
        where TNetworkBehaviour : NetworkBehaviour
    {
        var index = behaviour.netIdentity.NetworkBehaviours.IndexOf(behaviour);//behaviour.ComponentIndex ?

        var writer = new NetworkWriter();
        Compression.CompressVarUInt(writer, 1u << index);
        int position1 = writer.Position;
        writer.WriteByte(0);

        int position2 = writer.Position;
        if (writCustomObjectData == null)
        {
            //By default, we don't update the ObjectData only when syncObjectDirtyBits is set to 0 do
            writer.WriteULong(0);
        }
        else
        {
            writCustomObjectData.Invoke(writer);
        }

        if (writeCustomVarData == null)
        {
            //By default, we don't update the syncVarDirtyBits only when syncVarDirtyBits is set to 0
            writer.WriteULong(0);
        }
        else
        {
            writeCustomVarData.Invoke(writer);
        }

        int position3 = writer.Position;
        writer.Position = position1;
        byte num = (byte) (position3 - position2 & (int) byte.MaxValue);
        writer.WriteByte(num);
        writer.Position = position3;

        var msg = new EntityStateMessage()
        {
            netId = behaviour.netId,
            payload = writer.ToArraySegment()
        };

        return msg;
    }

    /// <summary>
    /// Use to spoof information on SyncVar.
    /// </summary>
    /// <typeparam name="TValue">The value type needs to be consistent with the type of the variable to change</typeparam>
    /// <param name="behaviour">The target NetWorkBehaviour who will see this value change on the client side of the receving player</param>
    /// <param name="id">The derty byte of the behaviour, can be found in the if condition of SerializeSyncVars of the target NetWorkBehaviour</param>
    /// <param name="value">The fictitious value for the client</param>
    public EntityStateMessage GetCustomVarMessage<TNetworkBehaviour, TValue>(TNetworkBehaviour behaviour, ulong id,
        TValue value) where TNetworkBehaviour : NetworkBehaviour => GetCustomVarMessage(behaviour,
        writeCustomVarData: writer =>
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