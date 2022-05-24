using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api
{
    public class Broadcast
    {
        public Broadcast(string msg,ushort time,Player player)
        {
            Message = msg;
            Time = time;
            _player = player;
        }

        private readonly Player _player;

        public float DisplayTime { get; internal set; } = float.MinValue;

        private string msg;

        public string Message
        {
            get => msg;
            set
            {
                if(value != msg)
                {
                    msg = value;

                    if (Active)
                        Refresh();
                }
            }
        }

        public ushort Time { get; }

        public bool Active { get; private set; }

        public void StartBc(Player player)
        {
            if (player.ActiveBroadcasts.FirstOrDefault() != this)
                return;

            if (Active)
                return;

            Active = true;

            DisplayTime = UnityEngine.Time.time;
            _player.Broadcast(Time, Message);
            Timing.CallDelayed(Time, () => EndBc());
        }

        public void Refresh()
        {
            var time = Time - (UnityEngine.Time.time - DisplayTime) + 1; //The one is there because by converting a float (14,99999) to a ushort the decimal places are ignored (14)
            _player.InstantBroadcast((ushort)time, Message);
        }

        public void EndBc()
        {
            if (!Active)
                return;

            Active = false;

            _player.ActiveBroadcasts.Remove(this);
            
            _player.ClearBroadcasts();

            if (_player.ActiveBroadcasts.FirstOrDefault() is { } broadcast)
                broadcast.StartBc(_player);
        }
    }

    public class BroadcastList
    {
        public BroadcastList(Player player) => _player = player;

        private readonly Player _player;

        private List<Broadcast> bcs = new List<Broadcast>();

        public void Add(Broadcast bc,bool instant = false)
        {
            if (bc is null)
                return;

            if (instant)
            {
                var currentbc = bcs.FirstOrDefault();

                var list = new List<Broadcast>
                {
                    bc
                };
                list.AddRange(bcs);
                bcs = list;

                if (currentbc != null)
                    currentbc.EndBc();
                else
                    bcs.First().StartBc(_player);
            }
            else
            {
                bcs.Add(bc);

                if (!bcs.First().Active)
                    bcs.First().StartBc(_player);
            }
        }

        public void Remove(Broadcast bc)
        {
            if(bcs.Any(x => x == bc))
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
}
