using Assets._Scripts.Dissonance;
using Grenades;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using System.Collections.Generic;
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

        public event EventHandler.OnSynapseEvent<PlayerEscapeEventArgs> PlayerEscapesEvent;

        public event EventHandler.OnSynapseEvent<PlayerSyncDataEventArgs> PlayerSyncDataEvent;

        public event EventHandler.OnSynapseEvent<PlayerReloadEventArgs> PlayerReloadEvent;

        public event EventHandler.OnSynapseEvent<PlayerEnterFemurEventArgs> PlayerEnterFemurEvent;

        public event EventHandler.OnSynapseEvent<PlayerGeneratorInteractEventArgs> PlayerGeneratorInteractEvent;

        public event EventHandler.OnSynapseEvent<PlayerKeyPressEventArgs> PlayerKeyPressEvent;

        public event EventHandler.OnSynapseEvent<PlayerDropItemEventArgs> PlayerDropItemEvent;

        public event EventHandler.OnSynapseEvent<PlayerPickUpItemEventArgs> PlayerPickUpItemEvent;

        public event EventHandler.OnSynapseEvent<PlayerShootEventArgs> PlayerShootEvent;

        public event EventHandler.OnSynapseEvent<PlayerSetClassEventArgs> PlayerSetClassEvent;

        public event EventHandler.OnSynapseEvent<PlayerConnectWorkstationEventArgs> PlayerConnectWorkstationEvent;

        public event EventHandler.OnSynapseEvent<PlayerUnconnectWorkstationEventArgs> PlayerUnconnectWorkstationEvent;

        public event EventHandler.OnSynapseEvent<PlayerDropAmmoEventArgs> PlayerDropAmmoEvent;

        public event EventHandler.OnSynapseEvent<PlayerCuffTargetEventArgs> PlayerCuffTargetEvent;

        public event EventHandler.OnSynapseEvent<PlayerUseMicroEventArgs> PlayerUseMicroEvent;

        public event EventHandler.OnSynapseEvent<PlayerWalkOnSinkholeEventArgs> PlayerWalkOnSinkholeEvent;

        public event EventHandler.OnSynapseEvent<PlayerReportEventArgs> PlayerReportEvent;

        public event EventHandler.OnSynapseEvent<PlayerDamagePermissionEventArgs> PlayerDamagePermissionEvent;

        public event EventHandler.OnSynapseEvent<PlayerUnCuffTargetEventArgs> PlayerUncuffTargetEvent;
        
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

        internal void InvokePlayerDamageEvent(Player victim, Player killer, ref PlayerStats.HitInfo info, out bool allow)
        {
            var ev = new PlayerDamageEventArgs
            {
                HitInfo = info,
                Killer = killer,
                Victim = victim
            };

            PlayerDamageEvent?.Invoke(ev);

            info = ev.HitInfo;
            allow = ev.Allow;
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
        
        internal void InvokePlayerThrowGrenadeEvent(Player player, SynapseItem item,ref GrenadeSettings settings, ref float force, ref float delay, ref bool allow)
        {
            var ev = new PlayerThrowGrenadeEventArgs
            {
                Player = player,
                Item = item,
                ForceMultiplier = force,
                Delay = delay,
                Allow = allow,
                Settings = settings,
            };

            PlayerThrowGrenadeEvent?.Invoke(ev);

            force = ev.ForceMultiplier;
            delay = ev.Delay;
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

        internal void InvokePlayerDropItemPatch(Player player,Items.SynapseItem item,out bool allow)
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

        internal void InvokePlayerConnectWorkstation(Player player,SynapseItem item,WorkStation station,out bool allow)
        {
            var ev = new PlayerConnectWorkstationEventArgs
            {
                Player = player,
                Item = item,
                WorkStation = station
            };

            PlayerConnectWorkstationEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerUnonnectWorkstation(Player player,WorkStation station, out bool allow)
        {
            var ev = new PlayerUnconnectWorkstationEventArgs
            {
                Player = player,
                WorkStation = station
            };

            PlayerUnconnectWorkstationEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokePlayerDropAmmoEvent(Player player, SynapseItem item, ref uint amount, ref int type, out bool allow)
        {
            var ev = new PlayerDropAmmoEventArgs
            {
                Tablet = item,
                AmmoType = (Enum.AmmoType)type,
                Amount = amount,
                Player = player
            };

            PlayerDropAmmoEvent?.Invoke(ev);

            amount = ev.Amount;
            type = (int)ev.AmmoType;
            allow = ev.Allow;
        }

        internal void InvokePlayerCuffTargetEvent(Player target,Player cuffer,SynapseItem disarmer,ref bool allow)
        {
            var ev = new PlayerCuffTargetEventArgs
            {
                Cuffer = cuffer,
                Disarmer = disarmer,
                Target = target,
                Allow = allow
            };

            PlayerCuffTargetEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeMicroUse(Player player, SynapseItem micro, ref MicroHID.MicroHidState state)
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

        internal void InvokeUncuff(Player player, Player cuffed, bool mate, out bool allow)
        {
            var ev = new PlayerUnCuffTargetEventArgs
            {
                Allow = true,
                Cuffed = cuffed,
                Player = player,
                FreeWithDisarmer = mate
            };

            PlayerUncuffTargetEvent?.Invoke(ev);

            allow = ev.Allow;
        }
        #endregion
    }
}