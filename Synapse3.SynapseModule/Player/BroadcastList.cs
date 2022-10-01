using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Synapse3.SynapseModule.Player;

public class BroadcastList : 
    ICollection<Broadcast>
{
    private readonly SynapsePlayer _player;
    private readonly List<Broadcast> _broadcasts = new();
    
    
    internal BroadcastList(SynapsePlayer player) => _player = player;
    
    
    public void Add(Broadcast item) => Add(item, false);
    public void Add(Broadcast bc, bool instant)
    {
        if (bc == null)
            return;

        if (instant)
        {
            var activeBroadcast = _broadcasts.FirstOrDefault();
            
            _broadcasts.Insert(0, bc);
            activeBroadcast?.EndBc();
            bc.StartBc(_player);
        }
        else
        {
            _broadcasts.Add(bc);
            
            if(!_broadcasts.First().Active)
                _broadcasts.First().StartBc(_player);
        }
    }
    public bool Remove(Broadcast bc)
    {
        if (bc != null && _broadcasts.Any(x => x == bc))
        {
            _broadcasts.Remove(bc);

            if (bc.Active)
                bc.EndBc();

            return true;
        }

        return false;
    }
    public void Clear()
    {
        if (_broadcasts.Count < 1)
            return;

        var activeBc = _broadcasts.FirstOrDefault();
        _broadcasts.Clear();
        activeBc?.EndBc();
    }
    
    
    public void CopyTo(Broadcast[] array, int arrayIndex) => _broadcasts.CopyTo(array, arrayIndex);
    public bool Contains(Broadcast bc) => _broadcasts.Contains(bc);
    
    
    public int Count => _broadcasts.Count;
    public bool IsReadOnly => false;
    
    
    public IEnumerator<Broadcast> GetEnumerator() => _broadcasts.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}