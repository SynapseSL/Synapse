using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse3.SynapseModule.Map.Objects;

public class FakeInfoManger<TInfo> : IJoinUpdate
{
    private readonly IFakeAbleObjectInfo<TInfo> _fakeAbleObject;
    private readonly PlayerService _playerService;
    private readonly TInfo _defaultInfo;
    private readonly Dictionary<SynapsePlayer, TInfo> _sendInfo = new();

    public FakeInfoManger(IFakeAbleObjectInfo<TInfo> fakeAbleObject, PlayerService playerService, TInfo defaultInfo)
    {
        _fakeAbleObject = fakeAbleObject;
        _playerService = playerService;
        _defaultInfo = defaultInfo;
        _playerService.JoinUpdates.Add(this);
    }

    public void UpdateAll()
    {
        foreach (var player in _playerService.Players)
        {
            UpdatePlayer(player);
        }
    }

    public bool NeedsJoinUpdate => true;
    public void UpdatePlayer(SynapsePlayer player)
    {
        var info = _defaultInfo;
        if (ToPlayerVisibleInfo.ContainsKey(player))
        {
            info = ToPlayerVisibleInfo[player];
        }
        else
        {
            foreach (var condition in VisibleInfoCondition)
            {
                if (condition.Key.Invoke(player))
                {
                    info = condition.Value;
                    break;
                }
            }
        }

        //This will prevent to send unnecessary packages from being send
        if (_sendInfo.ContainsKey(player) && _sendInfo[player].Equals(info))
            return;
        _sendInfo[player] = info;

        _fakeAbleObject.SendInfo(player, info);
    }

    public Dictionary<Func<SynapsePlayer, bool>, TInfo> VisibleInfoCondition { get; set; } = new();
    public Dictionary<SynapsePlayer, TInfo> ToPlayerVisibleInfo { get; set; } = new();
}

