using Assets._Scripts.Dissonance;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class PlayerEvents
    {
        internal PlayerEvents() {}
        
        public event EventHandler.OnSynapseEvent<PlayerJoinEventArgs> PlayerJoinEvent;

        public event EventHandler.OnSynapseEvent<PlayerLeaveEventArgs> PlayerLeaveEvent;

        public event EventHandler.OnSynapseEvent<PlayerBanEventArgs> PlayerBanEvent;

        public event EventHandler.OnSynapseEvent<PlayerSpeakEventArgs> PlayerSpeakEvent; 

        internal void InvokePlayerJoinEvent(Player player, ref string nickname)
        {
            var ev = new PlayerJoinEventArgs {Player = player, Nickname = nickname};
            PlayerJoinEvent?.Invoke(ev);
            nickname = ev.Nickname;
        }

        internal void InvokePlayerLeaveEvent(Player player)
        {
            var ev = new PlayerLeaveEventArgs {Player = player};
            PlayerLeaveEvent?.Invoke(ev);
        }

        internal void InvokePlayerBanEvent(Player bannedPlayer, Player issuer, ref int duration, ref string reason,
            ref bool allow)
        {
            var ev = new PlayerBanEventArgs {Allow = allow, Duration = duration, Issuer = issuer, Reason = reason, BannedPlayer = bannedPlayer};
            PlayerBanEvent?.Invoke(ev);

            duration = ev.Duration;
            reason = ev.Reason;
            allow = ev.Allow;
        }

        internal void InvokePlayerSpeakEvent(DissonanceUserSetup userSetup, ref bool intercom, ref bool radio,
            ref bool scp939, ref bool scpChat, ref bool specChat, ref bool allow)
        {
            var ev = new PlayerSpeakEventArgs
            {
                Allow = allow, Player = userSetup.GetPlayer(), IntercomTalk = intercom, RadioTalk = radio,
                Scp939Talk = scp939, ScpChat = scpChat, SpectatorChat = specChat, DissonanceUserSetup = userSetup
            };
            PlayerSpeakEvent?.Invoke(ev);

            intercom = ev.IntercomTalk;
            radio = ev.RadioTalk;
            scp939 = ev.Scp939Talk;
            scpChat = ev.ScpChat;
            specChat = ev.SpectatorChat;
            allow = ev.Allow;
        }
    }
}