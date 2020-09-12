using Synapse.Api.Components;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class PlayerEvents
    {
        internal PlayerEvents() {}
        
        public event EventHandler.OnSynapseEvent<PlayerJoinEventArgs> PlayerJoinEvent;

        public event EventHandler.OnSynapseEvent<PlayerLeaveEventArgs> PlayerLeaveEvent;

        internal void InvokePlayerJoinEvent(Player player, ref string nickname)
        {
            var ev = new PlayerJoinEventArgs {Player = player, Nickname = nickname};
            PlayerJoinEvent?.Invoke(ev);
            nickname = ev.Nickname;
        }

        internal void InvokePlayerLeaveEvent(Player player)
        {
            var ev = new PlayerLeaveEventArgs {Player = player};
        }
    }
}