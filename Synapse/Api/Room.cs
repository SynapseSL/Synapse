using Synapse.Api.Enum;
using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Room
    {
        internal Room(GameObject gameObject) => GameObject = gameObject;

        public GameObject GameObject { get; }

        public Vector3 Position => GameObject.transform.position;

        public string RoomName => GameObject.name;

        public ZoneType Zone
        {
            get
            {
                switch (Position.y)
                {
                    case 0f:
                        return ZoneType.LCZ;

                    case 1000f:
                        return ZoneType.Surface;

                    case -1000f:
                        if (RoomName.Contains("HCZ"))
                            return ZoneType.HCZ;
                        else
                            return ZoneType.Entrance;


                    case -2000f:
                        return ZoneType.Pocket;

                    default:
                        return ZoneType.None;
                }
            }
        }

        public ImageGenerator.RoomType RoomType
        {
            get
            {
                if (RoomName.Contains("LCZ_ClassDSpawn"))
                    return ImageGenerator.RoomType.Prison;

                string[] straight = new string[]
                {
                    "HCZ_Servers",
                    "HCZ_Testroom",
                    "EZ_Smallrooms2",
                    "LCZ_Toilets",
                    "LCZ_Plants",
                    "LCZ_Straight",
                    "HCZ_Nuke",
                    "HCZ_049",
                    "LCZ_Airlock",
                    "HCZ_Tesla",
                    "HCZ_EZ_Checkpoint",
                    "EZ_Straight",
                    "HCZ_Hid",
                    "EZ_PCs",
                    "EZ_PCs_small",
                    "EZ_Cafeteria",
                    "EZ_upstairs"
                };

                string[] curve = new string[]
                {
                    "LCZ_Curve",
                    "HCZ_Curve",
                    "EZ_Curve"
                };

                string[] cross = new string[]
                {
                    "EZ_Crossing",
                    "LCZ_Crossing",
                    "Root_*&*Outside Cams",
                    "PocketWorld",
                    "EZ_Intercom"
                };

                string[] t = new string[]
                {
                    "LCZ_TCross",
                    "HCZ_Room3ar",
                    "HCZ_Room3"
                };

                string[] end = new string[]
                {
                    "EZ_Endoof",
                    "LCZ_914",
                    "HCZ_106",
                    "EZ_GateB",
                    "EZ_GateA",
                    "LCZ_Armory",
                    "HCZ_ChkpB",
                    "HCZ_ChkpA",
                    "LCZ_ChkpA",
                    "LCZ_ChkpB",
                    "EZ_Shelter",
                    "LCZ_173",
                    "LCZ_Cafe",
                    "LCZ_012",
                    "HCZ_079",
                    "LCZ_372",
                    "HCZ_457",
                    "EZ_CollapsedTunnel"
                };

                if (straight.Any(x => RoomName.Contains(x)))
                    return ImageGenerator.RoomType.Straight;

                else if (curve.Any(x => RoomName.Contains(x)))
                    return ImageGenerator.RoomType.Curve;

                else if (cross.Any(x => RoomName.Contains(x)))
                    return ImageGenerator.RoomType.Cross;

                else if (t.Any(x => RoomName.Contains(x)))
                    return ImageGenerator.RoomType.RoomT;

                else if (end.Any(x => RoomName.Contains(x)))
                    return ImageGenerator.RoomType.Endoff;

                else
                    return (ImageGenerator.RoomType)(-1);
            }
        }
        
    }
}
