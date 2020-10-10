using Assets._Scripts.Dissonance;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class PlayerEvents
    {
        internal PlayerEvents() {}
        
        public event EventHandler.OnSynapseEvent<PlayerJoinEventArgs> PlayerJoinEvent;

        public event EventHandler.OnSynapseEvent<PlayerLeaveEventArgs> PlayerLeaveEvent;

        public event EventHandler.OnSynapseEvent<PlayerBanEventArgs> PlayerBanEvent;

        public event EventHandler.OnSynapseEvent<PlayerSpeakEventArgs> PlayerSpeakEvent;

        public event EventHandler.OnSynapseEvent<PlayerDeathEventArgs> PlayerDeathEvent;

        public event EventHandler.OnSynapseEvent<PlayerDamageEventArgs> PlayerDamageEvent;

        public event EventHandler.OnSynapseEvent<LoadComponentEventArgs> LoadComponentsEvent;

        public event EventHandler.OnSynapseEvent<PlayerItemInteractEventArgs> PlayerItemUseEvent;

        public event EventHandler.OnSynapseEvent<PlayerThrowGrenadeEventArgs> PlayerThrowGrenadeEvent;

        public event EventHandler.OnSynapseEvent<PlayerHealEventArgs> PlayerHealEvent;

        public event EventHandler.OnSynapseEvent<PlayerEscapeEventArgs> PlayerEscapseEvent;

        public event EventHandler.OnSynapseEvent<PlayerSyncDataEventArgs> PlayerSyncDataEvent;

        public event EventHandler.OnSynapseEvent<PlayerReloadEventArgs> PlayerReloadEvent;

        public event EventHandler.OnSynapseEvent<PlayerEnterFemurEventArgs> PlayerEnterFemurEvent;

        public event EventHandler.OnSynapseEvent<PlayerGeneratorInteractEventArgs> PlayerGeneratorInteractEvent;

        public event EventHandler.OnSynapseEvent<PlayerKeyPressEventArgs> PlayerKeyPressEvent;

        public event EventHandler.OnSynapseEvent<PlayerDropItemEventArgs> PlayerDropItemEvent;
        
        #region PlayerEventsInvoke
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

        internal void InvokePlayerDeathEvent(Player victim, Player killer, PlayerStats.HitInfo info)
        {
            var ev = new PlayerDeathEventArgs {HitInfo = info, Killer = killer, Victim = victim};
            PlayerDeathEvent?.Invoke(ev);
        }

        internal void InvokePlayerDamageEvent(Player victim, Player killer, ref PlayerStats.HitInfo info)
        {
            var ev = new PlayerDamageEventArgs {HitInfo = info, Killer = killer, Victim = victim};
            PlayerDamageEvent?.Invoke(ev);

            info = ev.HitInfo;
        }

        internal void InvokeLoadComponentsEvent(GameObject gameObject)
        {
            var ev = new LoadComponentEventArgs { Player = gameObject };
            LoadComponentsEvent?.Invoke(ev);
        }

        internal void InvokePlayerItemUseEvent(Player player, ItemType type, ItemInteractState state, ref bool allow)
        {
            var ev = new PlayerItemInteractEventArgs { Player = player, Type = type, Allow = allow, CurrentItem = player.ItemInHand, State = state };
            PlayerItemUseEvent?.Invoke(ev);
        }
        
        internal void InvokePlayerThrowGrenadeEvent(Player player, Inventory.SyncItemInfo itemInfo, ref float force, ref float delay, ref bool allow)
        {
            var ev = new PlayerThrowGrenadeEventArgs() { Player = player, ForceMultiplier = force, Delay = delay, Allow = allow };
            PlayerThrowGrenadeEvent?.Invoke(ev);
        }

        internal void InvokePlayerHealEvent(Player player, ref float amount, ref bool allow)
        {
            var ev = new PlayerHealEventArgs() { Player = player, Amount = amount, Allow = allow};
            PlayerHealEvent?.Invoke(ev);
        }

        internal void InvokePlayerEscapeEvent(Player player, ref RoleType spawnRoleType, RoleType cuffedRoleType,
            ref bool allow, bool isCuffed)
        {
            var ev = new PlayerEscapeEventArgs
            {
                Allow = allow, Player = player, ChuffedRole = cuffedRoleType, IsCuffed = allow,
                SpawnRole = spawnRoleType
            };
            PlayerEscapseEvent?.Invoke(ev);
        }

        internal void InvokePlayerSyncDataEvent(Player player, out bool allow)
        {
            allow = true;

            var ev = new PlayerSyncDataEventArgs
            {
                Player = player,
            };

            PlayerSyncDataEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerReloadEvent(Player player, ref bool allow, Inventory.SyncItemInfo syncItemInfo)
        {
            var ev = new PlayerReloadEventArgs {Allow = allow, Item = syncItemInfo,Player = player};
            PlayerReloadEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerEnterFemurEvent(Player player, ref bool allow, ref bool closeFemur)
        {
            var ev = new PlayerEnterFemurEventArgs {Allow = allow, Player = player, CloseFemur = closeFemur};
            PlayerEnterFemurEvent?.Invoke(ev);

            allow = ev.Allow;
            closeFemur = ev.CloseFemur;
        }

        internal void InvokePlayerGeneratorInteractEvent(Player player,Generator generator,Enum.GeneratorInteraction interaction,ref bool allow)
        {
            if (PlayerGeneratorInteractEvent == null) return;

            var ev = new PlayerGeneratorInteractEventArgs
            {
                Player = player,
                Generator = generator,
                GeneratorInteraction = interaction,
                Allow = true
            };

            PlayerGeneratorInteractEvent.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerKeyPressEvent(Player player, KeyCode keyCode) => PlayerKeyPressEvent?.Invoke(new PlayerKeyPressEventArgs { Player = player, KeyCode = keyCode });

        internal void InvokePlayerDropItemPatch(Player player,Items.Item item,out bool allow)
        {
            allow = true;
            if (PlayerDropItemEvent == null) return;

            var ev = new PlayerDropItemEventArgs
            {
                Player = player,
                Item = item,
                Allow = true,
            };

            PlayerDropItemEvent.Invoke(ev);

            allow = ev.Allow;
        }
        #endregion
    }
}