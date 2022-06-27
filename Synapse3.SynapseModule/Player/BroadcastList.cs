using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse3.SynapseModule.Player;

public class BroadcastList
{
    private readonly SynapsePlayer _player;
    private List<Broadcast> bcs = new();

    public BroadcastList(SynapsePlayer player) => _player = player;

    public void Add(Broadcast bc, bool instant = false)
    {
        if (bc == null)
            return;

        if (instant)
        {
            var currentbc = bcs.FirstOrDefault();
            
            bcs.Insert(0, bc);

            if (currentbc != null)
                currentbc.EndBc();
            else
                bcs.First().StartBc(_player);
        }
        else
        {
            bcs.Add(bc);
            
            if(!bcs.First().Active)
                bcs.First().StartBc(_player);
        }
    }

    public void Remove(Broadcast bc)
    {
        if (bcs.Any(x => x == bc))
        {
            bcs.Remove(bc);

            if (bc.Active)
                bc.EndBc();
        }
    }

    public void Clear()
    {
        if (bcs.Count < 1)
            return;

        var activebc = bcs.FirstOrDefault();
        bcs.Clear();
        activebc.EndBc();
    }

    public IEnumerator<Broadcast> GetEnumerator() => bcs.GetEnumerator();
    public bool Contains(Broadcast bc) => bcs.Contains(bc);
    public bool Any(Func<Broadcast, bool> func) => bcs.Any(func);
    public Broadcast FirstOrDefault() => bcs.FirstOrDefault();
    public Broadcast FirstOrDefault(Func<Broadcast, bool> func) => bcs.FirstOrDefault(func);
}