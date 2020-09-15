using Assets._Scripts.Dissonance;
using Grenades;
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
        
        public PlayerStats.HitInfo HitInfo { get; set; }
    }

    public class LoadComponentEventArgs : EventHandler.ISynapseEventArgs
    {
        public GameObject Player { get; internal set; }
    }
    
    public class PlayerItemUseEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        
        public ItemType Type { get; internal set; }
        
        public ItemUseState State { get; internal set; }
        
        public Inventory.SyncItemInfo CurrentItem { get; internal set; }
        
        public bool Allow { get; set; }
    }

    public class PlayerThrowGrenadeEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Inventory.SyncItemInfo ItemInfo { get; set; }
        
        public GrenadeSettings Settings { get; set; }

        public float ForceMultiplier { get; set; }
        
        public float Delay { get; set; }
        
        public bool Allow { get; set; }
    }
    
    public enum ItemUseState
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
        
        public RoleType ChuffedRole { get; internal set; }
        
        public bool IsCuffed { get; internal set; }
    }
}