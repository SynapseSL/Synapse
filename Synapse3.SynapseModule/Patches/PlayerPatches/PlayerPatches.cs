using System;
using System.Linq;
using GameCore;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Searching;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
internal static class PlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract),nameof(PlayerInteract.UserCode_CmdDetonateWarhead))]
    public static bool OnStartWarhead(PlayerInteract __instance)
    {
        try
        {
            DecoratedPlayerPatches.OnStartWarhead(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Start Warhead Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract),nameof(PlayerInteract.UserCode_CmdUsePanel))]
    public static bool OnPanelInteract(PlayerInteract __instance ,ref PlayerInteract.AlphaPanelOperations n)
    {
        try
        {
            if (!__instance.ChckDis(AlphaWarheadOutsitePanel.nukeside.transform.position) ||
                !__instance.CanInteract) return false;

            var ev = new WarheadPanelInteractEvent(__instance.GetSynapsePlayer(),
                !Synapse.Get<NukeService>().InsidePanel.Locked, n);
            
            Synapse.Get<PlayerEvents>().WarheadPanelInteract.Raise(ev);
            n = ev.Operation;
            
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Warhead Panel Interact Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static bool OnDoorInteract(DoorVariant __instance,ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedPlayerPatches.OnDoorInteract(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Door Interact Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]
    public static bool OnUseLocker(Locker __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedPlayerPatches.LockerUse(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Locker Use Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), typeof(GameObject), typeof(long), typeof(string),
        typeof(string), typeof(bool))]
    public static bool OnBan(GameObject user, long duration, string reason, string issuer, bool isGlobalBan)
    {
        try
        {
            var player = user.GetSynapsePlayer();
            var banIssuer = issuer.Contains("(")
                ? Synapse.Get<PlayerService>().GetPlayer(issuer.Substring(issuer.LastIndexOf('(') + 1,
                    issuer.Length - 2 - issuer.LastIndexOf('(')))
                : Synapse.Get<PlayerService>().GetPlayer(issuer);

            var ev = new BanEvent(player, true, banIssuer, reason, duration, isGlobalBan);
            Synapse.Get<PlayerEvents>().Ban.Raise(ev);

            return isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip") || ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Ban Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    public static bool OnDamage(PlayerStats __instance, DamageHandlerBase handler)
    {
        try
        {
            return DecoratedPlayerPatches.OnDealDamage(__instance, handler);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Damage Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropAmmo))]
    public static bool DropAmmo(Inventory __instance, ref byte ammoType, ref ushort amount)
    {
        try
        {
            var ev = new DropAmmoEvent(__instance.GetSynapsePlayer(), true, (AmmoType)ammoType, amount);
            Synapse.Get<PlayerEvents>().DropAmmo.Raise(ev);
            ammoType = (byte)ev.AmmoType;
            amount = ev.Amount;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Drop Ammo Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    public static bool EnterFemur(CharacterClassManager __instance)
    {
        try
        {
            DecoratedPlayerPatches.EnterFemur(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Enter Femur Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerInteract))]
    public static bool OnGeneratorInteract(Scp079Generator __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedPlayerPatches.OnGenInteract(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: GeneratorInteract Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.ServerHeal))]
    public static bool OnHeal(HealthStat __instance, ref float healAmount)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null) return false;
            
            var ev = new HealEvent(__instance.GetSynapsePlayer(), true, healAmount);
            Synapse.Get<PlayerEvents>().Heal.Raise(ev);
            healAmount = ev.Amount;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Heal Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static void OnJoin(NicknameSync __instance, ref string nick)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player.PlayerType == PlayerType.Server) return;
            var ev = new JoinEvent(__instance.GetSynapsePlayer(), nick);
            Synapse.Get<PlayerEvents>().Join.Raise(ev);
            nick = ev.NickName;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Join Event failed\n" + ex);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBulletholeDecal))]
    public static bool PlaceBulletHole(StandardHitregBase __instance, RaycastHit hit)
    {
        try
        {
            var player = __instance.Hub.GetSynapsePlayer();
            if (player == null) return false;

            var ev = new PlaceBulletHoleEvent(player, true, hit.point);
            Synapse.Get<PlayerEvents>().PlaceBulletHole.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Place Bullet Hole Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.UserCode_CmdReport))]
    public static bool OnReport(CheaterReport __instance, int playerId, ref string reason, ref bool notifyGm)
    {
        try
        {
            var reported = Synapse.Get<PlayerService>().GetPlayer(playerId);
            var ev = new ReportEvent(__instance.GetSynapsePlayer(), true, reported, reason, notifyGm);
            Synapse.Get<PlayerEvents>().Report.Raise(ev);
            reason = ev.Reason;
            notifyGm = ev.SendToNorthWood;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Report Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static bool SyncAltActive(Radio __instance, bool b)
    {
        try
        {
            return DecoratedPlayerPatches.OnSpeak(__instance, b);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Speak Secondary Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdSwitchAWButton))]
    public static bool OnOpenWarheadButton(PlayerInteract __instance)
    {
        try
        {
            DecoratedPlayerPatches.OpenWarheadButton(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Enter Warhead KeyCard Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SearchCoordinator), nameof(SearchCoordinator.ContinuePickupServer))]
    public static bool OnPickUpRequest(SearchCoordinator __instance)
    {
        try
        {
            var item = __instance.Completor.TargetPickup?.GetItem();
            if (item == null) return true;

            //Item is just used for the Event so I just redirect to the RootItem directly here
            if (item.RootParent is SynapseItem root)
                item = root;
            
            if (__instance.Completor.ValidateUpdate())
            {
                if (NetworkTime.time >= __instance.SessionPipe.Session.FinishTime)
                {
                    var ev = new PickupEvent(__instance.Hub, true, item);
                    Synapse.Get<PlayerEvents>().Pickup.Raise(ev);

                    if (ev.Allow)
                    {
                        __instance.Completor.Complete();
                    }
                    else
                    {
                        __instance.SessionPipe.Invalidate();
                    }
                    return false;
                }
            }
            else
            {
                __instance.SessionPipe.Invalidate();
            }

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Pickup Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem))]
    public static bool DropItem(Inventory __instance,ref ushort itemSerial, ref bool tryThrow)
    {
        try
        {
            if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var itembase) || !itembase.CanHolster())
                return false;

            var ev = new DropItemEvent(__instance.GetSynapsePlayer(), true, itembase.GetItem(), tryThrow);
            Synapse.Get<PlayerEvents>().DropItem.Raise(ev);
            tryThrow = ev.Throw;
            itemSerial = ev.ItemToDrop.Serial;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Drop Item Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ServerSelectItem))]
    public static bool OnSelectItem(Inventory __instance, ushort itemSerial)

    {
        try
        {
            DecoratedPlayerPatches.OnChangeItem(__instance, itemSerial);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Change Item Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SinkholeEnvironmentalHazard), nameof(SinkholeEnvironmentalHazard.OnEnter))]
    public static bool OnEnterSinkhole(SinkholeEnvironmentalHazard __instance, ReferenceHub player)
    {
        try
        {
            var sPlayer = player.GetSynapsePlayer();
            var allow = !(!Synapse3Extensions.CanHarmScp(sPlayer, false) || sPlayer.GodMode);

            var ev = new WalkOnSinkholeEvent(sPlayer, allow, __instance);
            Synapse.Get<PlayerEvents>().WalkOnSinkhole.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Enter Hazard Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TantrumEnvironmentalHazard),nameof(TantrumEnvironmentalHazard.OnEnter))]
    [HarmonyPatch(typeof(TantrumEnvironmentalHazard),nameof(TantrumEnvironmentalHazard.OnExit))]
    public static bool OnEnterTantrum(TantrumEnvironmentalHazard __instance, ReferenceHub player)
    {
        try
        {
            if (player == null || __instance.DisableEffect || __instance._correctPosition == null ||
                player.characterClassManager == null)
                return false;
            
            var sPlayer = player.GetSynapsePlayer();
            var allow = !(!Synapse3Extensions.CanHarmScp(sPlayer, false) || sPlayer.GodMode);

            var ev = new WalkOnTantrumEvent(sPlayer, allow, __instance);
            Synapse.Get<PlayerEvents>().WalkOnTantrum.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Enter Hazard Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorkstationController), nameof(WorkstationController.ServerInteract))]
    public static bool OnUseWorkstation(WorkstationController __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            if (colliderId != __instance._activateCollder.ColliderId || __instance.Status != 0 || ply == null)
                return false;

            var ev = new StartWorkStationEvent(ply, true, __instance.GetSynapseWorkStation());

            if (ev.Player == null || ev.WorkStation == null) return false;
            Synapse.Get<PlayerEvents>().StartWorkStation.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Start WorkStation Event failed\n" + ex);
            return true;
        }
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheckpointKiller), nameof(CheckpointKiller.OnTriggerEnter))]
    public static bool OnCheckpointEnter(Collider other)
    {
        try
        {
            DecoratedPlayerPatches.OnFallingIntoAbyss(other);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: FallingIntoAbyss event failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedPlayerPatches
{
    public static void OnFallingIntoAbyss(Collider other)
    {
        var player = other.GetComponentInParent<SynapsePlayer>();
        var ev = new FallingIntoAbyssEvent(player, true);
        Synapse.Get<PlayerEvents>().FallingIntoAbyss.Raise(ev);
        if (!ev.Allow) return;

        player.Hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Crushed));
    }
    
    public static void OpenWarheadButton(PlayerInteract interact)
    {
        if(!interact.CanInteract) return;
        var gameObject = GameObject.Find("OutsitePanelScript");
        if (!interact.ChckDis(gameObject.transform.position)) return;

        var player = interact.GetSynapsePlayer();
        var allow = interact._sr.BypassMode || KeycardPermissions.AlphaWarhead.CheckPermission(player);
        var component = gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>();

        var open = !Synapse.Get<SynapseConfigService>().GamePlayConfiguration.CloseWarheadButton || !component.NetworkkeycardEntered;
        
        var ev = new OpenWarheadButtonEvent(player, allow, open);
        Synapse.Get<PlayerEvents>().OpenWarheadButton.Raise(ev);
        if(!ev.Allow) return;
        
        component.NetworkkeycardEntered = ev.Open;
        interact.OnInteract();
    }
    
    public static bool OnSpeak(Radio radio, bool alternativeChat)
    {
        var player = radio.GetSynapsePlayer();
        
        var radioChat = radio._dissonanceSetup.RadioAsHuman;
        var scp939 = Synapse.Get<SynapseConfigService>().GamePlayConfiguration.SpeakingScp.Contains(player.RoleID);
        
        var ev = new SpeakSecondaryEvent(player, true, radioChat, scp939, alternativeChat);
        Synapse.Get<PlayerEvents>().SpeakSecondary.Raise(ev);
        
        radio._dissonanceSetup.MimicAs939 = ev.Scp939Chat && alternativeChat && ev.Allow;
        radio._dissonanceSetup.RadioAsHuman = ev.RadioChat && alternativeChat && ev.Allow;

        return ev.Allow;
    }

    public static void OnGenInteract(Scp079Generator gen, ReferenceHub hub, byte interaction)
    {
        if(gen._cooldownStopwatch.IsRunning && gen._cooldownStopwatch.Elapsed.TotalSeconds < gen._targetCooldown) return;

        if (interaction != 0 && !gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open))
        {
            return;
        }
        gen._cooldownStopwatch.Stop();
        
        var player = hub.GetSynapsePlayer();

        switch (interaction)
        {
            //0 - Request interaction with Generator Doors (doors)
            case 0:
                if (gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Unlocked))
                {
                    var ev = new GeneratorInteractEvent(player, true, gen.GetSynapseGenerator(),
                        gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open)
                            ? GeneratorInteract.CloseDoor
                            : GeneratorInteract.OpenDoor);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);

                    if (ev.Allow)
                    {
                        gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Open,
                            !gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open));
                        gen._targetCooldown = gen._doorToggleCooldownTime;
                    }
                }
                else
                {
                    var allow = gen._requiredPermission.CheckPermission(player) &&
                                Synapse3Extensions.CanHarmScp(player, true);
                    var ev = new GeneratorInteractEvent(player, allow, gen.GetSynapseGenerator(),
                        GeneratorInteract.UnlockDoor);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);

                    if (ev.Allow)
                    {
                        gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
                    }
                    else
                    {
                        gen.RpcDenied();
                    }

                    gen._targetCooldown = gen._unlockCooldownTime;
                }
                break;
            
            //1 - Request to swap the Activation State (lever)
            case 1:
                if ((gen.Activating || Synapse3Extensions.CanHarmScp(player,true)) && !gen.Engaged)
                {
                    var ev = new GeneratorInteractEvent(player, true, gen.GetSynapseGenerator(),
                        gen.Activating ? GeneratorInteract.Cancel : GeneratorInteract.Activate);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);
                    if(!ev.Allow) break;
                    
                    gen.Activating = !gen.Activating;
                    if (gen.Activating)
                    {
                        gen._leverStopwatch.Restart();
                    }

                    gen._targetCooldown = gen._doorToggleCooldownTime;
                }
                break;
            
            //2 - Request do Cancel The Activation (cancel button)
            case 2:
                if (gen.Activating && !gen.Engaged)
                {
                    var ev = new GeneratorInteractEvent(player, true, gen.GetSynapseGenerator(),
                        GeneratorInteract.Cancel);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);
                    if (!ev.Allow) break;

                    gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Activating, false);
                    gen._targetCooldown = gen._unlockCooldownTime;
                }
                break;
            
            default:
                gen._targetCooldown = 1f;
                break;
        }
        
        gen._cooldownStopwatch.Restart();
    }
    
    public static void EnterFemur(CharacterClassManager manager)
    {
        if(!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems) return;

        foreach (var player in Synapse.Get<PlayerService>().Players)
        {
            if(!player.Hub.Ready || Vector3.Distance(player.transform.position,manager._lureSpj.transform.position) >= 1.97f) continue;
            if (player.Team is Team.SCP or Team.RIP || player.GodMode) return;
            if (!Synapse3Extensions.CanHarmScp(player, false)) return;

            var service = Synapse.Get<RoundService>();
            var closeFemur = service.FemurSacrifices + 1 >=
                        Synapse.Get<SynapseConfigService>().GamePlayConfiguration.RequiredForFemur;

            var ev = new EnterFemurEvent(player, true, closeFemur);
            Synapse.Get<PlayerEvents>().EnterFemur.Raise(ev);

            if (ev.Allow)
            {
                service.FemurSacrifices++;
                player.PlayerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.UsedAs106Bait));

                if (ev.CloseFemur)
                    manager._lureSpj.SetState(false, true);
            }
        }
    }

    public static bool OnDealDamage(PlayerStats stats, DamageHandlerBase handler)
    {
        if (!stats._hub.characterClassManager.IsAlive || stats._hub.characterClassManager.GodMode) return false;
        var damage = (handler as StandardDamageHandler)?.Damage ?? 0;

        var player = stats.GetSynapsePlayer();
        SynapsePlayer attacker = null;
        if (handler is AttackerDamageHandler aHandler)
            attacker = aHandler.Attacker;
        var damageType = handler.GetDamageType();

        if (damageType == DamageType.PocketDecay)
        {
            attacker = Synapse.Get<PlayerService>().Players
                .FirstOrDefault(x => x.ScpController.Scp106.PlayersInPocket.Contains(player));
            if (attacker != null && !Synapse3Extensions.GetHarmPermission(attacker, player)) return false;
        }

        var ev = new DamageEvent(player, true, attacker, damageType, damage);
        Synapse.Get<PlayerEvents>().Damage.Raise(ev);
        ((StandardDamageHandler)handler).Damage = ev.Damage;
        return ev.Allow;
    }
    
    public static void OnChangeItem(Inventory inventory, ushort serial)
    {
        if(serial == inventory.CurItem.SerialNumber) return;
        ItemBase oldItem = null;
        ItemBase newItem = null;
        var flag = inventory.CurItem.SerialNumber == 0 ||
                   (inventory.UserInventory.Items.TryGetValue(inventory.CurItem.SerialNumber, out oldItem) &&
                    inventory.CurInstance != null);

        if (serial == 0 || inventory.UserInventory.Items.TryGetValue(serial, out newItem))
        {
            if (inventory.CurItem.SerialNumber > 0 && flag && !oldItem.CanHolster()) return;
            if (serial != 0 && !newItem.CanEquip()) return;
            if (serial == 0)
            {
                var ev = new ChangeItemEvent(inventory.GetSynapsePlayer(), true, SynapseItem.None);
                Synapse.Get<PlayerEvents>().ChangeItem.Raise(ev);
                if (ev.Allow)
                {
                    inventory.NetworkCurItem = ItemIdentifier.None;
                    if (!inventory.isLocalPlayer)
                        inventory.CurInstance = null;
                }
            }
            else
            {
                var ev = new ChangeItemEvent(inventory.GetSynapsePlayer(), true, newItem.GetItem());
                Synapse.Get<PlayerEvents>().ChangeItem.Raise(ev);
                if (ev.Allow)
                {
                    inventory.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, serial);
                    if (!inventory.isLocalPlayer)
                        inventory.CurInstance = newItem;
                }
            }
        }
        else if (!flag)
        {
            var ev = new ChangeItemEvent(inventory.GetSynapsePlayer(), true, SynapseItem.None);
            Synapse.Get<PlayerEvents>().ChangeItem.Raise(ev);
            if (ev.Allow)
            {
                inventory.NetworkCurItem = ItemIdentifier.None;
                if (!inventory.isLocalPlayer)
                    inventory.CurInstance = null;
            }
        }
    }
    
    public static void OnDoorInteract(DoorVariant door, ReferenceHub hub, byte colliderId)
    {
        var player = hub.GetSynapsePlayer();
        var allow = false;
        var bypassDenied = false;
        if (door.ActiveLocks > 0)
        {
            var mode = DoorLockUtils.GetMode((DoorLockReason)door.ActiveLocks);
            
            var canInteractGeneral = mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.CanOpen);
            var scpOverride = mode.HasFlagFast(DoorLockMode.ScpOverride) &&
                              hub.characterClassManager.CurRole.team == Team.SCP;
            var canChangeCurrently = mode != DoorLockMode.FullLock &&
                                     ((door.TargetState && mode.HasFlagFast(DoorLockMode.CanClose)) ||
                                      (!door.TargetState && mode.HasFlagFast(DoorLockMode.CanOpen)));

            if (!canInteractGeneral && !scpOverride && !canChangeCurrently)
            {
                bypassDenied = true;
            }
        }

        //This is most often false when the Animation is still playing
        if(!door.AllowInteracting(hub,colliderId)) return;
        
        if (!bypassDenied)
        {
            if (hub.characterClassManager.CurClass == RoleType.Scp079 ||
                door.RequiredPermissions.CheckPermission(player))
            {
                allow = true;
            }  
        }

        var ev = new DoorInteractEvent(player, allow, door.GetSynapseDoor(), bypassDenied);
        Synapse.Get<PlayerEvents>().DoorInteract.Raise(ev);
        
        if (ev.Allow)
        {
            door.NetworkTargetState = !door.TargetState;
            door._triggerPlayer = hub;
            return;
        }

        if (ev.LockBypassRejected)
        {
            door.LockBypassDenied(hub, colliderId);
            return;
        }

        if (ev.PlayDeniedSound)
            door.PermissionsDenied(hub, colliderId);
        DoorEvents.TriggerAction(door, DoorAction.AccessDenied, hub); 
    }
    
    public static void LockerUse(Locker locker, ReferenceHub hub, byte colliderId)
    {
        if(colliderId >= locker.Chambers.Length || !locker.Chambers[colliderId].CanInteract) return;

        var player = hub.GetSynapsePlayer();
        var synapseLocker = locker.GetSynapseLocker();
        var chamber = synapseLocker.Chambers[colliderId];

        if (!locker.Chambers[colliderId].RequiredPermissions.CheckPermission(player))
        {
            locker.RpcPlayDenied(colliderId);
            return;
        }

        var ev = new LockerUseEvent(player, true, synapseLocker, chamber);
        Synapse.Get<PlayerEvents>().LockerUse.Raise(ev);

        if (!ev.Allow)
        {
            locker.RpcPlayDenied(colliderId);
            return;
        }

        chamber.Open = !chamber.Open;
    }

    public static void OnStartWarhead(PlayerInteract player)
    {
        if(!player.CanInteract) return;
        
        if(!player._playerInteractRateLimit.CanExecute()) return;
        
        var panel = GameObject.Find("OutsitePanelScript");
        var sPlayer = player.GetSynapsePlayer();

        var ev = new StartWarheadEvent(sPlayer, true);

        if (!player.ChckDis(panel.transform.position) || !AlphaWarheadOutsitePanel.nukeside.enabled ||
            !panel.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
        {
            ev.Allow = false;
        }

        Synapse.Get<PlayerEvents>().StartWarhead.Raise(ev);
        
        if(!ev.Allow) return;

        AlphaWarheadController.Host.StartDetonation();
        ServerLogs.AddLog(ServerLogs.Modules.Warhead,
            sPlayer.Hub.LoggedNameFromRefHub() + " started the Alpha Warhead detonation.",
            ServerLogs.ServerLogType.GameEvent);
        player.OnInteract();
    }
}
