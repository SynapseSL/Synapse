using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api
{
    public class Broadcast
    {
        public Broadcast(string msg, ushort time, Player player)
        {
            Message = msg;
            Time = time;
            _player = player;

            DisplayTime = Single.MinValue;
        }

        private readonly Player _player;

        public float DisplayTime { get; internal set; }

        private string msg;

        public string Message
        {
            get => msg;
            set
            {
                if (value != msg)
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
            _ = Timing.CallDelayed(Time, () => EndBc());
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
        public BroadcastList(Player player)
        {
            _player = player;
            broadcasts = new List<Broadcast>();
        }

        private readonly Player _player;

        private List<Broadcast> broadcasts;

        public void Add(Broadcast bc, bool instant = false)
        {
            if (bc is null)
                return;

            if (instant)
            {
                var currentbc = broadcasts.FirstOrDefault();

                var list = new List<Broadcast>
                {
                    bc
                };
                list.AddRange(broadcasts);
                broadcasts = list;

                if (currentbc != null)
                    currentbc.EndBc();
                else
                    broadcasts.First().StartBc(_player);
            }
            else
            {
                broadcasts.Add(bc);

                if (!broadcasts.First().Active)
                    broadcasts.First().StartBc(_player);
            }
        }

        public void Remove(Broadcast bc)
        {
            if (broadcasts.Any(x => x == bc))
            {
                _ = broadcasts.Remove(bc);

                if (bc.Active)
                    bc.EndBc();
            }
        }

        public void Clear()
        {
            if (broadcasts.Count < 1)
                return;
            var activebc = broadcasts.FirstOrDefault();
            broadcasts.Clear();
            activebc.EndBc();
        }

        public IEnumerator<Broadcast> GetEnumerator() 
            => broadcasts.GetEnumerator();

        public bool Contains(Broadcast bc) 
            => broadcasts.Contains(bc);

        public bool Any(Func<Broadcast, bool> func)
            => broadcasts.Any(func);

        public Broadcast FirstOrDefault() 
            => broadcasts.FirstOrDefault();

        public Broadcast FirstOrDefault(Func<Broadcast, bool> func)
            => broadcasts.FirstOrDefault(func);
    }
}
