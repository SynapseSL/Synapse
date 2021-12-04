using Assets._Scripts.Dissonance;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using UnityEngine;
using InventorySystem.Items.MicroHID;
using System;
using PlayerStatsSystem;
using Synapse.Api.Enum;

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

        public event EventHandler.OnSynapseEvent<PlayerEscapeEventArgs> PlayerEscapesEvent;

        [Obsolete("Use Server.Update Event instead and go through all players")]
        public event EventHandler.OnSynapseEvent<PlayerSyncDataEventArgs> PlayerSyncDataEvent;

        public event EventHandler.OnSynapseEvent<PlayerReloadEventArgs> PlayerReloadEvent;

        public event EventHandler.OnSynapseEvent<PlayerEnterFemurEventArgs> PlayerEnterFemurEvent;

        public event EventHandler.OnSynapseEvent<PlayerGeneratorInteractEventArgs> PlayerGeneratorInteractEvent;

        public event EventHandler.OnSynapseEvent<PlayerKeyPressEventArgs> PlayerKeyPressEvent;

        public event EventHandler.OnSynapseEvent<PlayerDropItemEventArgs> PlayerDropItemEvent;

        public event EventHandler.OnSynapseEvent<PlayerPickUpItemEventArgs> PlayerPickUpItemEvent;

        public event EventHandler.OnSynapseEvent<PlayerShootEventArgs> PlayerShootEvent;

        public event EventHandler.OnSynapseEvent<PlayerSetClassEventArgs> PlayerSetClassEvent;

        public event EventHandler.OnSynapseEvent<PlayerStartWorkstationEventArgs> PlayerStartWorkstationEvent;

        [Obsolete("Tablets are removed use PlayerStartWorkstationEvent")]
        public event EventHandler.OnSynapseEvent<PlayerConnectWorkstationEventArgs> PlayerConnectWorkstationEvent;

        public event EventHandler.OnSynapseEvent<PlayerDropAmmoEventArgs> PlayerDropAmmoEvent;

        public event EventHandler.OnSynapseEvent<PlayerCuffTargetEventArgs> PlayerCuffTargetEvent;

        public event EventHandler.OnSynapseEvent<PlayerUseMicroEventArgs> PlayerUseMicroEvent;

        public event EventHandler.OnSynapseEvent<PlayerWalkOnSinkholeEventArgs> PlayerWalkOnSinkholeEvent;

        public event EventHandler.OnSynapseEvent<PlayerReportEventArgs> PlayerReportEvent;

        public event EventHandler.OnSynapseEvent<PlayerDamagePermissionEventArgs> PlayerDamagePermissionEvent;

        public event EventHandler.OnSynapseEvent<PlayerUnCuffTargetEventArgs> PlayerUncuffTargetEvent;

        public event EventHandler.OnSynapseEvent<PlayerChangeItemEventArgs> PlayerChangeItemEvent;
        
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

        internal void InvokePlayerBanEvent(Player bannedPlayer, Player issuer, ref long duration, ref string reason,
            ref bool allow)
        {
            var ev = new PlayerBanEventArgs {Allow = allow, BanDuration = duration, Issuer = issuer, Reason = reason, BannedPlayer = bannedPlayer};
            PlayerBanEvent?.Invoke(ev);

            duration = ev.BanDuration;
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

        internal void InvokePlayerDeathEvent(Player victim, Player killer, ItemType Weapon, out bool Allow)
        {
            var ev = new PlayerDeathEventArgs
            {
                Killer = killer,
                Victim = victim,
                Weapon = Weapon
            };
            PlayerDeathEvent?.Invoke(ev);
            
            Allow = ev.Allow;
        }

        internal void InvokePlayerDamageEvent(Player victim, Player killer, ref float Damage, ItemType Weapon, out bool Allow)
        {
            var ev = new PlayerDamageEventArgs
            {
                Killer = killer,
                Victim = victim,
                Damage = Damage,
                Weapon = Weapon
            };
            
            PlayerDamageEvent?.Invoke(ev);

            Damage = ev.Damage;
            Allow = ev.Allow;
        }

        internal void InvokeLoadComponentsEvent(GameObject gameObject)
        {
            var ev = new LoadComponentEventArgs { Player = gameObject };
            LoadComponentsEvent?.Invoke(ev);
        }

        internal void InvokePlayerItemUseEvent(Player player, Api.Items.SynapseItem item, ItemInteractState state, ref bool allow)
        {
            var ev = new PlayerItemInteractEventArgs
            {
                Player = player,
                CurrentItem = item,
                State = state,
                Allow = allow,
            };
            PlayerItemUseEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerHealEvent(Player player, ref float amount, ref bool allow)
        {
            var ev = new PlayerHealEventArgs() { Player = player, Amount = amount, Allow = allow};
            PlayerHealEvent?.Invoke(ev);

            amount = ev.Amount;
            allow = ev.Allow;
        }

        internal void InvokePlayerEscapeEvent(Player player, ref int role, ref bool isD, ref bool change, ref bool allow)
        {
            var ev = new PlayerEscapeEventArgs
            {
                Player = player,
                SpawnRole = role,
                IsClassD = isD,
                ChangeTeam = change,
                Allow = allow,
            };
            PlayerEscapesEvent?.Invoke(ev);

            role = ev.SpawnRole;
            isD = ev.IsClassD;
            change = ev.ChangeTeam;
            allow = ev.Allow;
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

        internal void InvokePlayerReloadEvent(Player player, ref bool allow, Items.SynapseItem syncItemInfo)
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
                Allow = allow,
            };

            PlayerGeneratorInteractEvent.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerKeyPressEvent(Player player, KeyCode keyCode) => PlayerKeyPressEvent?.Invoke(new PlayerKeyPressEventArgs { Player = player, KeyCode = keyCode });

        internal void InvokePlayerDropItemPatch(Player player, Items.SynapseItem item, ref bool throwitem, out bool allow)
        {
            allow = true;
            if (PlayerDropItemEvent == null) return;

            var ev = new PlayerDropItemEventArgs
            {
                Player = player,
                Item = item,
                Throw = throwitem,
                Allow = true,
            };

            PlayerDropItemEvent.Invoke(ev);

            allow = ev.Allow;
            throwitem = ev.Throw;
        }

        internal void InvokePlayerPickUpEvent(Player player,Items.SynapseItem item,out bool allow)
        {
            allow = true;

            if (PlayerPickUpItemEvent == null) return;

            var ev = new PlayerPickUpItemEventArgs
            {
                Player = player,
                Item = item,
                Allow = true,
            };

            PlayerPickUpItemEvent.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerShootEvent(Player player,Player target,Vector3 targetpos,SynapseItem weapon,out bool allow)
        {
            var ev = new PlayerShootEventArgs
            {
                Player = player,
                Allow = true,
                Target = target,
                TargetPosition = targetpos,
                Weapon = weapon,
            };

            PlayerShootEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeSetClassEvent(PlayerSetClassEventArgs ev) => PlayerSetClassEvent?.Invoke(ev);

        internal void InvokePlayerStartWorkstation(Player player, WorkStation station, out bool allow)
        {
            var evold = new PlayerConnectWorkstationEventArgs
            {
                Player = player,
                WorkStation = station
            };
            PlayerConnectWorkstationEvent?.Invoke(evold);

            var ev = new PlayerStartWorkstationEventArgs
            {
                Player = player,
                WorkStation = station,
                Allow = evold.Allow
            };

            PlayerStartWorkstationEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerDropAmmoEvent(Player player, ref AmmoType type,ref ushort amount, out bool allow)
        {
            var ev = new PlayerDropAmmoEventArgs
            {
                AmmoType = type,
                Amount = amount,
                Player = player
            };

            PlayerDropAmmoEvent?.Invoke(ev);

            amount = (ushort)ev.Amount;
            type = ev.AmmoType;
            allow = ev.Allow;
        }

        internal void InvokePlayerCuffTargetEvent(Player target, Player cuffer, out bool allow)
        {
            var ev = new PlayerCuffTargetEventArgs
            {
                Cuffer = cuffer,
                Target = target,
                Allow = true,
            };

            PlayerCuffTargetEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeMicroUse(Player player, SynapseItem micro, ref HidState state)
        {
            var ev = new PlayerUseMicroEventArgs
            {
                Player = player,
                Micro = micro,
                State = state,
            };

            PlayerUseMicroEvent?.Invoke(ev);

            state = ev.State;
        }

        internal void InvokeSinkhole(Player player,SinkholeEnvironmentalHazard sinkhole,ref bool allow)
        {
            var ev = new PlayerWalkOnSinkholeEventArgs
            {
                Allow = allow,
                Player = player,
                Sinkhole = sinkhole
            };

            PlayerWalkOnSinkholeEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerReport(Player player, Player target, string reason, ref bool global, out bool allow)
        {
            var ev = new PlayerReportEventArgs
            {
                Reporter = player,
                Target = target,
                GlobalReport = global,
                Reason = reason,
            };

            PlayerReportEvent?.Invoke(ev);

            global = ev.GlobalReport;
            allow = ev.Allow;
        }

        internal void InvokePlayerDamagePermissions(Player victim, Player attacker, ref bool allow)
        {
            var ev = new PlayerDamagePermissionEventArgs
            {
                Victim = victim,
                Attacker = attacker,
                AllowDamage = allow
            };

            PlayerDamagePermissionEvent?.Invoke(ev);

            allow = ev.AllowDamage;
        }

        internal void InvokeUncuff(Player player, Player cuffed, out bool allow)
        {
            var ev = new PlayerUnCuffTargetEventArgs
            {
                Allow = true,
                Cuffed = cuffed,
                Player = player,
            };

            PlayerUncuffTargetEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeChangeItem(Player player, SynapseItem old, SynapseItem newitem, out bool allow)
        {
            var ev = new PlayerChangeItemEventArgs
            {
                Player = player,
                OldItem = old,
                NewItem = newitem
            };

            PlayerChangeItemEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeThrowGrenade(Player player, SynapseItem grenade, out bool allow)
        {
            var ev = new PlayerThrowGrenadeEventArgs
            {
                Allow = true,
                Item = grenade,
                Player = player
            };

            PlayerThrowGrenadeEvent?.Invoke(ev);

            allow = ev.Allow;
        }
        #endregion
    }
}