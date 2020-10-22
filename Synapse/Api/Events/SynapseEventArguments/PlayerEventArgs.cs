using Assets._Scripts.Dissonance;
using Grenades;
using Synapse.Api.Items;
using UnityEngine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Synapse.Api.Events.SynapseEventArguments
{
    public class PlayerJoinEventArgs: EventHandler.ISynapseEventArgs
    {
        public Player Player { internal set; get; }
        
        public string Nickname { set; get; }
    }

    public class PlayerLeaveEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
    }
    
    public class PlayerBanEventArgs : EventHandler.ISynapseEventArgs
    {
        public bool Allow { get; set; } = true;
        
        public Player BannedPlayer { get; internal set; }
        
        public Player Issuer { get; internal set; }
        
        public int Duration { get; set; }
        
        public string Reason { get; set; }
    }

    public class PlayerSpeakEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        
        public DissonanceUserSetup DissonanceUserSetup { get; internal set; }

        public bool Scp939Talk { get; set; }
        
        public bool IntercomTalk { get; set; }
        
        public bool RadioTalk { get; set; }
        
        public bool ScpChat { get; set; }
        
        public bool SpectatorChat { get; set; }
        
        public bool Allow { get; set; }
    }

    public class PlayerDeathEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Victim { get; internal set; }
        
        public Player Killer { get; internal set; }
        
        public PlayerStats.HitInfo HitInfo { get; internal set; }
    }

    public class PlayerDamageEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Killer { get; internal set; }
        
        public Player Victim { get; internal set; }
       
        public float DamageAmount
        {
            get => HitInfo.Amount;
            set
            {
                var info = HitInfo;
                info.Amount = value;
                HitInfo = info;
            }
        }

        public PlayerStats.HitInfo HitInfo { get; set; }
    }

    public class LoadComponentEventArgs : EventHandler.ISynapseEventArgs
    {
        public GameObject Player { get; internal set; }
    }
    
    public class PlayerItemInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        
        public ItemInteractState State { get; internal set; }
        
        public Items.SynapseItem CurrentItem { get; internal set; }
        
        public bool Allow { get; set; }
    }

    public class PlayerThrowGrenadeEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Items.SynapseItem Item { get; internal set; }
        
        public GrenadeSettings Settings { get; set; }

        public float ForceMultiplier { get; set; }
        
        public float Delay { get; set; }
        
        public bool Allow { get; set; }
    }
    
    public enum ItemInteractState
    {
        Initiating,
        Finalizing,
        Stopping
    }

    public class PlayerHealEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        
        public float Amount { get; set; }
        
        public bool Allow { get; set; }
    }

    public class PlayerEscapeEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        
        public RoleType SpawnRole { get; set; }
        
        public bool Allow { get; set; }
        
        public RoleType CuffedRole { get; internal set; }
        
        public bool IsCuffed { get; internal set; }
    }

    public class PlayerSyncDataEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class PlayerReloadEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public bool Allow { get; set; } = true;
        
        public Items.SynapseItem Item { get; internal set; }
    }

    public class PlayerEnterFemurEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public bool Allow { get; set; } = true;
        
        public bool CloseFemur { get; set; }
    }

    public class PlayerGeneratorInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Generator Generator { get; internal set; }

        public Enum.GeneratorInteraction GeneratorInteraction { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PlayerKeyPressEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public KeyCode KeyCode { get; internal set; }
    }

    public class PlayerDropItemEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Items.SynapseItem Item { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PlayerPickUpItemEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Items.SynapseItem Item { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PlayerShootEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player Target { get; internal set; }

        public Vector3 TargetPosition { get; internal set; }

        public SynapseItem Weapon { get; internal set; }

        public bool Allow { get; set; }
    }
}