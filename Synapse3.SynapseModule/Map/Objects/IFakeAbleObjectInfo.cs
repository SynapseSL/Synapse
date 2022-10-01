using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map.Objects;

public interface IFakeAbleObjectInfo<TInfo>
{
    void SendInfo(SynapsePlayer player, TInfo info);

    FakeInfoManger<TInfo> FakeInfoManger { get; }
}
