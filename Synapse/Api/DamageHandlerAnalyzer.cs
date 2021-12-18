using PlayerStatsSystem;
using Synapse.Api.Items;

namespace Synapse.Api
{
    public static class DamageHandlerAnalyzer
    {
        public static void Analyze(this DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            //get the apporiate Parse Method
            dynamic realType = handler;
            Parse(realType, out Attacker, out Weapon, out WeaponType, out Damage);
        }

        public static void Parse(DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage) // security Method
        {
            Attacker  = null;
            Weapon = null;
            WeaponType = ItemType.None;
            Damage = -1;
        }

        public static void Parse(StandardDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            Damage = handler.Damage;
        }

        public static void Parse(AttackerDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.None;
            Damage = handler.Damage;
        }

        public static void Parse(ScpDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            Damage = handler.Damage;
        }

        public static void Parse(FirearmDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = handler.WeaponType;
            Damage = handler.Damage;
        }

        public static void Parse(Scp096DamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            Damage = handler.Damage;
        }

        public static void Parse(ExplosionDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.GrenadeHE;
            Damage = handler.Damage;
        }

        public static void Parse(CustomReasonDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            Damage = handler.Damage;
        }

        public static void Parse(MicroHidDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.MicroHID;
            Damage = handler.Damage;
        }

        public static void Parse(Scp018DamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.SCP018;
            Damage = handler.Damage;
        }

        public static void Parse(UniversalDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            Damage = handler.Damage;
        }
    }
}
