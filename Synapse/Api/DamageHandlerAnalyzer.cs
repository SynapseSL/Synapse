using PlayerStatsSystem;
using Synapse.Api.Enum;
using Synapse.Api.Items;

namespace Synapse.Api
{
    public static class DamageHandlerAnalyzer
    {
        public static void Analyze(this DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, 
            out DamageHandlerType DamageHadnlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            switch(handler)
            {
                case WarheadDamageHandler warhead:
                    Parse(warhead, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case ScpDamageHandler scpDamageHandler:
                    Parse(scpDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case FirearmDamageHandler firearmDamageHandler:
                    Parse(firearmDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case Scp096DamageHandler scp096DamageHandler:
                    Parse(scp096DamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case ExplosionDamageHandler explosionDamageHandler:
                    Parse(explosionDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case MicroHidDamageHandler microHidDamageHandler:
                    Parse(microHidDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case RecontainmentDamageHandler recontainmentDamageHandler:
                    Parse(recontainmentDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case Scp018DamageHandler scp018DamageHandler:
                    Parse(scp018DamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case DisruptorDamageHandler disruptorDamageHandler:
                    Parse(disruptorDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage); 
                    break;
                case AttackerDamageHandler attackerDamageHandler:
                    Parse(attackerDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case CustomReasonDamageHandler customReasonDamageHandler:
                    Parse(customReasonDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case UniversalDamageHandler universalDamageHandler:
                    Parse(universalDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                case StandardDamageHandler standardDamageHandler:
                    Parse(standardDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
                default:
                    Parse(handler, out Attacker, out Weapon, out WeaponType, out DamageHadnlerType, out DamageTranslation, out Damage);
                    break;
            }
/*            dynamic realType = handler;
            Parse(realType, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);*/
            //^^use this when mono allows it
        }

        public static void Parse(DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage) // security Method
        {
            Attacker  = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Unknown;
            DamageTranslation = DamageTranslation.None;
            Damage = -1;
        }

        // place the methods in the same order as the switch case so as not to go crazy

        public static void Parse(WarheadDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Nuck;
            DamageTranslation = DamageTranslation.Nuck;
            Damage = -1;
        }

        public static void Parse(ScpDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Scp;  
            DamageTranslation = (DamageTranslation)handler._translationId;
            Damage = handler.Damage;
        }

        public static void Parse(FirearmDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = handler.WeaponType;
            DamageHandlerType = DamageHandlerType.Firearm;
            DamageTranslation = DamageTranslation.BulletWounds;
            Damage = handler.Damage;
        }

        public static void Parse(Scp096DamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Scp096;
            DamageTranslation = DamageTranslation.Scp096;
            Damage = handler.Damage;
        }

        public static void Parse(ExplosionDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.GrenadeHE;
            DamageHandlerType = DamageHandlerType.Explosion;
            DamageTranslation = DamageTranslation.Explosion;
            Damage = handler.Damage;
            
        }

        public static void Parse(MicroHidDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.MicroHID;
            DamageHandlerType = DamageHandlerType.MicroHid;
            DamageTranslation = DamageTranslation.MicroHID;
            Damage = handler.Damage;
        }

        public static void Parse(RecontainmentDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Recontainment;
            DamageTranslation = DamageTranslation.Recontained;
            Damage = handler.Damage;
        }

        public static void Parse(Scp018DamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.SCP018;
            DamageHandlerType = DamageHandlerType.Scp018;
            DamageTranslation = DamageTranslation.Crushed;
            Damage = handler.Damage;
        }

        public static void Parse(DisruptorDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.MolecularDisruptor;
            DamageHandlerType = DamageHandlerType.Disruptor;
            DamageTranslation = DamageTranslation.None; //sory but idk
            Damage = handler.Damage;
        }

        public static void Parse(AttackerDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Attacker;
            DamageTranslation = DamageTranslation.None;
            Damage = handler.Damage;
        }

        public static void Parse(CustomReasonDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.CustomReason;
            DamageTranslation = DamageTranslation.None;
            Damage = handler.Damage;
        }

        public static void Parse(UniversalDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, 
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Universal;
            DamageTranslation = (DamageTranslation)handler.TranslationId;
            Damage = handler.Damage;
        }

        public static void Parse(StandardDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType,
            out DamageHandlerType DamageHandlerType, out DamageTranslation DamageTranslation, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageHandlerType = DamageHandlerType.Standard;
            DamageTranslation = DamageTranslation.None;
            Damage = handler.Damage;
        }

        public static bool IsPartOf(this DamageHandlerType obj1, DamageHandlerType obj2)
            => (obj1 & obj2) == obj2;
    }
}
