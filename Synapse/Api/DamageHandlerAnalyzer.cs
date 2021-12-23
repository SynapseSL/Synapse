using PlayerStatsSystem;
using Synapse.Api.Enum;
using Synapse.Api.Items;

namespace Synapse.Api
{
    public static class DamageHandlerAnalyzer
    {
        public static void Analyze(this DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            switch(handler)
            {
                case WarheadDamageHandler warhead:
                    Parse(warhead, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case ScpDamageHandler scpDamageHandler:
                    Parse(scpDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case FirearmDamageHandler firearmDamageHandler:
                    Parse(firearmDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case Scp096DamageHandler scp096DamageHandler:
                    Parse(scp096DamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case ExplosionDamageHandler explosionDamageHandler:
                    Parse(explosionDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case MicroHidDamageHandler microHidDamageHandler:
                    Parse(microHidDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case RecontainmentDamageHandler recontainmentDamageHandler:
                    Parse(recontainmentDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case Scp018DamageHandler scp018DamageHandler:
                    Parse(scp018DamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case AttackerDamageHandler attackerDamageHandler:
                    Parse(attackerDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case CustomReasonDamageHandler customReasonDamageHandler:
                    Parse(customReasonDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case UniversalDamageHandler universalDamageHandler:
                    Parse(universalDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                case StandardDamageHandler standardDamageHandler:
                    Parse(standardDamageHandler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
                default:
                    Parse(handler, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);
                    break;
            }
/*            dynamic realType = handler;
            Parse(realType, out Attacker, out Weapon, out WeaponType, out DamageType, out Damage);*/
            //^^use this when mono allows it
        }

        public static void Parse(DamageHandlerBase handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage) // security Method
        {
            Attacker  = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
            Damage = -1;
        }

        // place the methods in the same order as the switch case so as not to go crazy

        public static void Parse(WarheadDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Nuck;
            Damage = -1;
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
            Logger.Get.Debug("Bug here ? 8");
            Attacker = handler.Attacker.GetPlayer();
            Weapon = null;
            WeaponType = ItemType.GrenadeHE;
            DamageType = DamageType.Explosion;
            Damage = handler.Damage;
            Logger.Get.Debug("Bug here ? 9");
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

        public static void Parse(AttackerDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            Attacker = handler.Attacker.GetPlayer();
            Weapon = Attacker.ItemInHand;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
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

        public static void Parse(StandardDamageHandler handler, out Player Attacker, out SynapseItem Weapon, out ItemType WeaponType, out DamageType DamageType, out float Damage)
        {
            
            Attacker = null;
            Weapon = null;
            WeaponType = ItemType.None;
            DamageType = DamageType.Unknown;
            Damage = handler.Damage;
        }
    }
}
