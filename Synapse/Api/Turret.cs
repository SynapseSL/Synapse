using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using PlayerStatsSystem;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Networking;
using Random = UnityEngine.Random;

namespace Synapse.Api
{
    public class Turret
    {
        public Turret(Vector3 position)
        {
            GameObject = new GameObject("SynapseTurret");
            GameObject.transform.position = position;
        }
        public Turret(Vector3 position, Type[] components)
        {
            GameObject = new GameObject("SynapseTurret", components);
            GameObject.transform.position = position;
        }



        public ShootSound Sound { get; set; }
        public float Damage { get; set; } = 10f;
        public string DeathReason { get; set; } = "Killed by a Turret";
        public string Cassie { get; set; } = "TERMINATED BY AUTOMATIC SHOOTING UNIT";
        public int Distance { get; set; } = 25;
        public float Inaccuracy { get; set; } = 1f;

        public GameObject GameObject { get; }
        public Vector3 Position { get => GameObject.transform.position; set => GameObject.transform.position = value; }



        public void SingleShootDirection(Vector3 direction)
        {
            var ray = GetRay(direction);

            if (Physics.Raycast(ray, out var hit, Distance, StandardHitregBase.HitregMask))
                ExecuteShoot(ray, hit);

            PlayAudio(Sound);
        }

        public void PlayAudio(ShootSound sound)
        {
            foreach(var player in Server.Get.Players)
            {
                var msg = new GunAudioMessage(player, 0, (byte)Distance, player);
                var to = Position - player.Position;

                if(player.RoleType != RoleType.Spectator && to.sqrMagnitude > 1760f)
                {
                    to.y = 0f;
                    var num = Vector3.Angle(Vector3.forward, to);
                    if (Vector3.Dot(to.normalized, Vector3.left) > 0f)
                        num = 360f - num;

                    msg.ShooterDirection = (byte)Mathf.RoundToInt(num / 1.44f);
                    msg.ShooterRealDistance = (byte)Mathf.RoundToInt(Mathf.Min(to.magnitude, 255f));
                }

                msg.Weapon = ItemType.GunE11SR;

                player.Connection.Send(msg);
            }
        }

        public bool ExecuteShoot(Ray ray, RaycastHit hit)
        {
            if (hit.collider.TryGetComponent<IDestructible>(out var destructible))
            {
                if (destructible.Damage(Damage, new SynapseTurretDamageHandler(Damage, DeathReason, Cassie), hit.point))
                {
                    if (!ReferenceHub.TryGetHubNetID(destructible.NetworkId, out var hub))
                    {
                        var player = hub.GetPlayer();
                        foreach (var hubPlayer in player.SpectatorManager.ServerCurrentSpectatingPlayers)
                            hubPlayer.GetPlayer().Connection.Send(new GunHitMessage(false, Damage, ray.origin));

                        if (player.ClassManager.IsHuman())
                            new GunHitMessage(hit.point + (ray.origin - hit.point).normalized, ray.direction, true).SendToAuthenticated();
                    }
                    return true;
                }
            }
            else new GunHitMessage(hit.point + (ray.origin - hit.point).normalized, ray.direction, false).SendToAuthenticated();
            return false;
        }



        private Ray GetRay(Vector3 direction)
        {
            var ray = new Ray(Position, direction);
            var a = (new Vector3(Random.value, Random.value, Random.value) - Vector3.one / 2f).normalized * Random.value;
            ray.direction = Quaternion.Euler(a * Inaccuracy) * ray.direction;
            return ray;
        }
    }

    public class SynapseTurretDamageHandler : StandardDamageHandler
    {
        public SynapseTurretDamageHandler(float damage, string reason, string cassie)
        {
            Damage = damage;
            DeathReason = reason;
            Cassie = cassie;
        }

        public string Cassie { get; }

        public string DeathReason { get; }

        public override float Damage { get; set; }

        public override string ServerLogsText => DeathReason;

        public override CassieAnnouncement CassieDeathAnnouncement
        {
            get
            {
                var cassie = new CassieAnnouncement();
                cassie.Announcement = Cassie;
                //TODO: Fix Subtitle
                cassie.SubtitleParts = new Subtitles.SubtitlePart[]
                {
                    new Subtitles.SubtitlePart(Subtitles.SubtitleType.Custom, new string[]
                    {
                        Cassie
                    })
                };

                return cassie;
            }
        }

        public override void WriteAdditionalData(NetworkWriter writer)
        {
            base.WriteAdditionalData(writer);
            writer.WriteString(DeathReason);
        }
    }
}
