using PlayerStatsSystem;
using Synapse.Api.Enum;
using Synapse.Api.Items;

namespace Synapse.Api
{
    public static class DamageHandlerAnalyzer
    {
        public static void Analyze(this DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            //get the apporiate Parse Method
            dynamic realType = handler;
            Parse(realType, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
        }

        public static void Parse(DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage) // security Method
        {
            Attacker  = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
            Damage = -1;
        }

        public static void Parse(WarheadDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage) // security Method
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Nuck;
            Damage = -1;
        }
        
        public static void Parse(StandardDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
            Damage = handler.Damage;

        }

        public static void Parse(AttackerDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
            Damage = handler.Damage;
        }

        public static void Parse(ScpDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Scp;
            Damage = handler.Damage;
        }

        public static void Parse(FirearmDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = handler.WeaponType;
            DamageType = DamageType.Firearm;
            Damage = handler.Damage;
        }

        public static void Parse(Scp096DamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Scp;
            Damage = handler.Damage;
        }

        public static void Parse(ExplosionDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.GrenadeHE;
            DamageType = DamageType.Explosion;
            Damage = handler.Damage;
        }

        public static void Parse(CustomReasonDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
            Damage = handler.Damage;
        }

        public static void Parse(MicroHidDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.MicroHID;
            DamageType = DamageType.MicroHid;
            Damage = handler.Damage;
        }
        public static void Parse(RecontainmentDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Recontainment;
            Damage = handler.Damage;
        }

        public static void Parse(Scp018DamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.SCP018;
            DamageType = DamageType.Scp018;
            Damage = handler.Damage;
        }

        public static void Parse(UniversalDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = handler.TranslationId switch
            {
                0 => DamageType.Recontainment,
                5 => DamageType.Bleeding,
                6 => DamageType.Falldown,
                8 => DamageType.Decontamination,
                9 => DamageType.Poison,
                10 => DamageType.Scp207,
                11 => DamageType.Scp330,
                13 => DamageType.Tesla,
                20 => DamageType.Crushed,
                21 => DamageType.FemurBreaker,
                22 => DamageType.FriendlyFireDetector,
                _ => DamageType.Unknown
            };
            Damage = handler.Damage;
        }
    }
}
