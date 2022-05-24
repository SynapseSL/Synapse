using System;
using System.Collections.Generic;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp079
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.UserCode_CmdInteract))]
    internal static class Scp079BulkPatch
    {
        [HarmonyPrefix]
        private static bool Scp079Interact(Scp079PlayerScript __instance, Command079 command, string args, GameObject target)
        {
            args ??= "";
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                {
                    return false;
                }
                if (!__instance.iAm079)
                {
                    return false;
                }
                string[] array = args.Split(':');
                GameCore.Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant, false);
                __instance.RefreshCurrentRoom();
                if (!__instance.CheckInteractableLegitness(__instance.CurrentRoom, target, true))
                {
                    return false;
                }

                DoorVariant doorVariant = null; // F you, compiler
                bool gotDoorVariant = target?.TryGetComponent(out doorVariant) ?? false;
                List<string> list = GameCore.ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();

                switch (command)
                {
                    case Command079.Door:
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                return false;
                            }
                            if (target is null)
                            {
                                GameCore.Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant, false);
                                return false;
                            }
                            if (!gotDoorVariant)
                            {
                                return false;
                            }
                            if (doorVariant.TryGetComponent(out DoorNametagExtension doorNametagExtension) && list.Count > 0 && list.Contains(doorNametagExtension.GetName))
                            {
                                GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                                return false;
                            }
                            string text = doorVariant.RequiredPermissions.RequiredPermissions.ToString();
                            float manaFromLabel = __instance.GetManaFromLabel("Door Interaction " + (text.Contains(",") ? text.Split(',')[0] : text), __instance.abilities);



                            var action = doorVariant.TargetState ? Scp079EventMisc.DoorAction.Closing : Scp079EventMisc.DoorAction.Opening;
                            var intendedResult = manaFromLabel <= __instance.Mana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;

                            SynapseController.Server.Events.Scp.Scp079.Invoke079DoorInteract(
                                __instance.gameObject.GetPlayer(),
                                action,
                                intendedResult,
                                manaFromLabel,
                                doorVariant.GetDoor(),
                                out var actualResult
                                );

                            switch (actualResult)
                            {
                                case Scp079EventMisc.InteractionResult.Allow:
                                    {
                                        bool targetState = doorVariant.TargetState;
                                        doorVariant.ServerInteract(ReferenceHub.GetHub(__instance.gameObject), 0);
                                        if (targetState != doorVariant.TargetState)
                                        {
                                            __instance.Mana -= manaFromLabel;
                                            __instance.AddInteractionToHistory(target, true);
                                            GameCore.Console.AddDebugLog("SCP079", "Door state changed.", MessageImportance.LeastImportant, false);
                                            return false;
                                        }
                                        GameCore.Console.AddDebugLog("SCP079", "Door state failed to change.", MessageImportance.LeastImportant, false);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow:
                                    {
                                        GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy:
                                    {
                                        GameCore.Console.AddDebugLog("SCP079", "Not enough mana.", MessageImportance.LeastImportant, false);
                                        // might wanna change __instance.Mana to 0,
                                        // the client may do another check when being told that the player has not enough mana,
                                        // resulting in realizing they do indeed have enough mana
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                        return false;
                                    }
                            }
                            return false;
                        }
                    case Command079.Doorlock:
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                return false;
                            }
                            if (target is null)
                            {
                                GameCore.Console.AddDebugLog("SCP079", "The door lock command requires a target.", MessageImportance.LessImportant, false);
                                return false;
                            }
                            if (doorVariant is null)
                            {
                                return false;
                            }
                            ;
                            if (doorVariant.TryGetComponent(out DoorNametagExtension doorNametagExtension2) && list.Count > 0 && list.Contains(doorNametagExtension2.GetName))
                            {
                                GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                                return false;
                            }

                            float manaFromLabel = __instance.GetManaFromLabel("Door Lock Minimum", __instance.abilities);
                            var action = ((DoorLockReason)doorVariant.ActiveLocks).HasFlag(DoorLockReason.Regular079) ? Scp079EventMisc.DoorAction.Unlocking : Scp079EventMisc.DoorAction.Locking;

                            Scp079EventMisc.InteractionResult intendedResult;
                            if (action == Scp079EventMisc.DoorAction.Unlocking)
                            {
                                intendedResult = __instance.lockedDoors.Contains(doorVariant.netId) ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.Disallow;
                            }
                            else
                            {
                                intendedResult = manaFromLabel <= __instance.Mana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;
                            }

                            SynapseController.Server.Events.Scp.Scp079.Invoke079DoorInteract(
                                __instance.gameObject.GetPlayer(),
                                action,
                                intendedResult,
                                manaFromLabel,
                                doorVariant.GetDoor(),
                                out var actualResult
                                );

                            switch (actualResult)
                            {
                                case Scp079EventMisc.InteractionResult.Allow when action == Scp079EventMisc.DoorAction.Unlocking:
                                    {
                                        if (!__instance.lockedDoors.Contains(doorVariant.netId))
                                        {
                                            return false;
                                        }
                                        __instance.lockedDoors.Remove(doorVariant.netId);
                                        doorVariant.ServerChangeLock(DoorLockReason.Regular079, false);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Allow when action == Scp079EventMisc.DoorAction.Locking:
                                    {
                                        if (!__instance.lockedDoors.Contains(doorVariant.netId))
                                        {
                                            __instance.lockedDoors.Add(doorVariant.netId);
                                        }
                                        doorVariant.ServerChangeLock(DoorLockReason.Regular079, true);
                                        __instance.AddInteractionToHistory(doorVariant.gameObject, true);
                                        __instance.Mana -= __instance.GetManaFromLabel("Door Lock Start", __instance.abilities);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy:
                                    {
                                        // might wanna change __instance.Mana to 0,
                                        // the client may do another check when being told that the player has not enough mana,
                                        // resulting in realizing they do indeed have enough mana
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                        return false;
                                    }
                            }
                            return false;
                        }
                    case Command079.Speaker:
                        {
                            string speakerQualifiedName = __instance.CurrentRoom.transform.parent.name + "/" + __instance.CurrentRoom.name + "/Scp079Speaker";
                            GameObject speaker = GameObject.Find(speakerQualifiedName);
                            float manaFromLabel = __instance.GetManaFromLabel("Speaker Start", __instance.abilities);

                            Scp079EventMisc.InteractionResult intendedResult;
                            if (speaker is null)
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Disallow;
                            }
                            else if (manaFromLabel * 1.5f <= __instance.Mana)
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Allow;
                            }
                            else
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.NoEnergy;
                            }

                            SynapseController.Server.Events.Scp.Scp079.Invoke079SpeakerInteract(
                                __instance.gameObject.GetPlayer(),
                                manaFromLabel,
                                intendedResult,
                                out var actualResult
                                );

                            switch (actualResult)
                            {
                                case Scp079EventMisc.InteractionResult.Allow:
                                    {
                                        __instance.Mana -= manaFromLabel;
                                        __instance.Speaker = speakerQualifiedName;
                                        __instance.AddInteractionToHistory(speaker, true);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy:
                                    {
                                        // might wanna change __instance.Mana to 0,
                                        // the client may do another check when being told that the player has not enough mana,
                                        // resulting in realizing they do indeed have enough mana
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                        return false;
                                    }
                            }

                            return false;
                        }
                    case Command079.StopSpeaker:
                        __instance.Speaker = string.Empty;
                        return false;
                    case Command079.ElevatorTeleport:
                        {
                            Synapse.Api.Logger.Get.Debug("Teleport");
                            float manaFromLabel = __instance.GetManaFromLabel("Elevator Teleport", __instance.abilities);
                            if (manaFromLabel > __instance.Mana)
                            {
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                return false;
                            }
                            Camera079 camera = null;
                            foreach (Scp079Interactable scp079Interactable in __instance.nearbyInteractables)
                            {
                                if (scp079Interactable.type == Scp079Interactable.InteractableType.ElevatorTeleport)
                                {
                                    camera = scp079Interactable.optionalObject.GetComponent<Camera079>();
                                }
                            }
                            if (camera != null)
                            {
                                __instance.RpcSwitchCamera(camera.cameraId, false);
                                __instance.Mana -= manaFromLabel;
                                __instance.AddInteractionToHistory(target, true);
                                return false;
                            }
                            if (ConsoleDebugMode.CheckImportance("SCP079", MessageImportance.LeastImportant, out Color32 color))
                            {
                                Scp079Interactable scp079Interactable2 = null;
                                Dictionary<Scp079Interactable.InteractableType, byte> dictionary = new Dictionary<Scp079Interactable.InteractableType, byte>();
                                foreach (Scp079Interactable scp079Interactable3 in __instance.nearbyInteractables)
                                {
                                    if (dictionary.ContainsKey(scp079Interactable3.type))
                                    {
                                        Dictionary<Scp079Interactable.InteractableType, byte> dictionary2 = dictionary;
                                        Scp079Interactable.InteractableType type = scp079Interactable3.type;
                                        byte b = dictionary2[type];
                                        dictionary2[type] = (byte)(b + 1);
                                    }
                                    else
                                    {
                                        dictionary[scp079Interactable3.type] = 1;
                                    }
                                    if (scp079Interactable3.type == Scp079Interactable.InteractableType.ElevatorTeleport)
                                    {
                                        scp079Interactable2 = scp079Interactable3;
                                    }
                                }
                                string text2;
                                if (scp079Interactable2 is null)
                                {
                                    text2 = "None of the " + __instance.nearbyInteractables.Count + " were an ElevatorTeleport, found: ";
                                    using (Dictionary<Scp079Interactable.InteractableType, byte>.Enumerator enumerator2 = dictionary.GetEnumerator())
                                    {
                                        while (enumerator2.MoveNext())
                                        {
                                            KeyValuePair<Scp079Interactable.InteractableType, byte> keyValuePair = enumerator2.Current;
                                            text2 = string.Concat(new object[]
                                            {
                                        text2,
                                        keyValuePair.Value,
                                        "x",
                                        keyValuePair.Key.ToString().Substring(keyValuePair.Key.ToString().Length - 4),
                                        " "
                                            });
                                        }
                                        goto IL_755;
                                    }
                                }
                                if (scp079Interactable2.optionalObject is null)
                                {
                                    text2 = "Optional object is missing.";
                                }
                                else if (scp079Interactable2.optionalObject.GetComponent<Camera079>() is null)
                                {
                                    string str = "";
                                    Transform transform = scp079Interactable2.optionalObject.transform;
                                    for (int i = 0; i < 5; i++)
                                    {
                                        str = transform.name + str;
                                        if (!(transform.parent != null))
                                        {
                                            break;
                                        }
                                        transform = transform.parent;
                                    }
                                    text2 = "Camera is missing at " + str;
                                }
                                else
                                {
                                    text2 = "Unknown error";
                                }
                            IL_755:
                                GameCore.Console.AddDebugLog("SCP079", "Could not find the second elevator: " + text2, MessageImportance.LeastImportant, false);
                                return false;
                            }
                            return false;
                        }
                    case Command079.ElevatorUse:
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Elevator Use", __instance.abilities);
                            string elevatorName = string.Empty;
                            if (array.Length > 0)
                            {
                                elevatorName = array[0];
                            }
                           
                            Elevator synElevator = Map.Get.Elevators.Find(_ => _.Name == elevatorName);
                            Scp079EventMisc.InteractionResult intendedResult;
                            if (manaFromLabel <= __instance.Mana)
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.NoEnergy;
                            }
                            else if (synElevator is null || (AlphaWarheadController.Host.timeToDetonation == 0f || !synElevator.Operative || synElevator.Locked))
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Disallow;
                            }
                            else
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Allow;
                            }

                            intendedResult = manaFromLabel <= __instance.Mana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;

                            SynapseController.Server.Events.Scp.Scp079.Invoke079ElevatorUse(
                                __instance.gameObject.GetPlayer(),
                                manaFromLabel,
                                synElevator,
                                intendedResult,
                                out var actualResult
                                );

                            switch (actualResult)
                            {
                                case Scp079EventMisc.InteractionResult.Allow:
                                    {
                                        if (synElevator.Use())
                                        {
                                            __instance.Mana -= manaFromLabel;
                                            bool flag3 = false;
                                            foreach (Lift.Elevator elevator in synElevator.Lift.elevators)
                                            {
                                                __instance.AddInteractionToHistory(elevator.door.GetComponentInParent<Scp079Interactable>().gameObject, !flag3);
                                                flag3 = true;
                                            }
                                        }

                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy:
                                    {
                                        // might wanna change __instance.Mana to 0,
                                        // the client may do another check when being told that the player has not enough mana,
                                        // resulting in realizing they do indeed have enough mana
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                        return false;
                                    }
                            }

                            return false;
                        }
                    case Command079.Tesla:
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Tesla Gate Burst", __instance.abilities);
                            if (__instance.CurrentRoom != null)
                            {
                                TeslaGate vanillaTesla = __instance.CurrentRoom.GetComponentInChildren<TeslaGate>();
                                Tesla synapseTesla = vanillaTesla != null ? Server.Get.Map.Teslas.Find(_ => _.Gate == vanillaTesla) : null;

                                var intendedResult = manaFromLabel <= __instance.Mana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;

                                SynapseController.Server.Events.Scp.Scp079.Invoke079TeslaInteract(
                                    __instance.gameObject.GetPlayer(),
                                    manaFromLabel,
                                    synapseTesla?.Room,
                                    synapseTesla,
                                    intendedResult,
                                    out var actualResult
                                    );

                                switch (actualResult)
                                {
                                    case Scp079EventMisc.InteractionResult.Allow:
                                        {
                                            if (vanillaTesla != null)
                                            {
                                                vanillaTesla.RpcInstantBurst();
                                            }
                                            __instance.AddInteractionToHistory(vanillaTesla.gameObject, true);
                                            __instance.Mana -= manaFromLabel;
                                            return false;
                                        }
                                    case Scp079EventMisc.InteractionResult.Disallow:
                                        {
                                            return false;
                                        }
                                    case Scp079EventMisc.InteractionResult.NoEnergy:
                                        {
                                            // might wanna change __instance.Mana to 0,
                                            // the client may do another check when being told that the player has not enough mana,
                                            // resulting in realizing they do indeed have enough mana
                                            __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                            return false;
                                        }
                                }
                                return false;
                            }
                            throw new Exception("Unable to find Tesla Gate in a null room");
                        }
                    case Command079.Lockdown:
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                GameCore.Console.AddDebugLog("SCP079", "Lockdown cannot commence, Warhead in progress.", MessageImportance.LessImportant, false);
                                return false;
                            }
                            float manaFromLabel = __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);
                            GameCore.Console.AddDebugLog("SCP079", "Attempting lockdown...", MessageImportance.LeastImportant, false);

                            if (__instance.CurrentRoom != null)
                            {
                                HashSet<Scp079Interactable> roomInteractablesHashSet = Scp079Interactable.InteractablesByRoomId[__instance.CurrentRoom.UniqueId];
                                HashSet<DoorVariant> doorHashSet = new HashSet<DoorVariant>();

                                GameCore.Console.AddDebugLog("SCP079", "Loaded all interactables", MessageImportance.LeastImportant, false);
                                GameObject lockdownInteractable = null;
                                foreach (Scp079Interactable interactable in roomInteractablesHashSet)
                                {
                                    if (interactable != null)
                                    {
                                        if (interactable.type != Scp079Interactable.InteractableType.Door)
                                        {
                                            if (interactable.type == Scp079Interactable.InteractableType.Lockdown)
                                            {
                                                lockdownInteractable = interactable.gameObject;
                                            }
                                        }
                                        else if (interactable.TryGetComponent(out DoorVariant doorVariant2) && (object)doorVariant2 is IDamageableDoor damageableDoor && damageableDoor.IsDestroyed)
                                        {
                                            GameCore.Console.AddDebugLog("SCP079", "Lockdown can't initiate, one of the doors were destroyed.", MessageImportance.LessImportant, false);
                                            return false;
                                        }
                                    }
                                }

                                if (__instance.CurrentLDCooldown > 0f)
                                {
                                    GameCore.Console.AddDebugLog("SCP079", "Lockdown still on cooldown.", MessageImportance.LessImportant, false);
                                    return false;
                                }

                                GameCore.Console.AddDebugLog("SCP079", "Looking for doors to lock...", MessageImportance.LeastImportant, false);
                                foreach (Scp079Interactable scp079Interactable5 in roomInteractablesHashSet)
                                {
                                    if (!(scp079Interactable5 is null) && scp079Interactable5.TryGetComponent(out DoorVariant doorVariant3))
                                    {
                                        bool doorLocked = doorVariant3.ActiveLocks == (ushort)DoorLockReason.None;
                                        if (!doorLocked)
                                        {
                                            DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)doorVariant3.ActiveLocks);
                                            doorLocked = (mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.ScpOverride));
                                        }
                                        if (doorLocked)
                                        {
                                            if (doorVariant3.TargetState)
                                            {
                                                doorVariant3.NetworkTargetState = false;
                                            }
                                            doorVariant3.ServerChangeLock(DoorLockReason.Lockdown079, true);
                                            doorVariant3.UnlockLater(__instance.LockdownDuration, DoorLockReason.Lockdown079);
                                            doorHashSet.Add(doorVariant3);
                                        }
                                    }
                                }

                                var intendedResult = manaFromLabel <= __instance.Mana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;
                                bool lightsOut = true;

                                SynapseController.Server.Events.Scp.Scp079.Invoke079RoomLockdown(
                                    __instance.gameObject.GetPlayer(),
                                    manaFromLabel,
                                    Server.Get.Map.Rooms.Find(_ => _.GameObject == __instance.CurrentRoom), // FIX
                                    ref lightsOut,
                                    intendedResult,
                                    out var actualResult
                                    );

                                switch (actualResult)
                                {
                                    case Scp079EventMisc.InteractionResult.Allow:
                                        {
                                            foreach (FlickerableLightController flickerableLightController in __instance.CurrentRoom.GetComponentsInChildren<FlickerableLightController>())
                                            {
                                                if (flickerableLightController != null)
                                                {
                                                    flickerableLightController.ServerFlickerLights(8f);
                                                }
                                            }
                                            __instance.CurrentLDCooldown = __instance.LockdownCooldown + __instance.LockdownDuration;
                                            __instance.TargetSetLockdownCooldown(__instance.connectionToClient, __instance.CurrentLDCooldown);
                                            GameCore.Console.AddDebugLog("SCP079", "Lockdown initiated.", MessageImportance.LessImportant, false);
                                            __instance.AddInteractionToHistory(lockdownInteractable, true);
                                            __instance.Mana -= __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);
                                            return false;
                                        }
                                    case Scp079EventMisc.InteractionResult.Disallow:
                                        {
                                            return false;
                                        }
                                    case Scp079EventMisc.InteractionResult.NoEnergy:
                                        {
                                            // might wanna change __instance.Mana to 0,
                                            // the client may do another check when being told that the player has not enough mana,
                                            // resulting in realizing they do indeed have enough mana
                                            __instance.RpcNotEnoughMana(manaFromLabel, __instance.Mana);
                                            GameCore.Console.AddDebugLog("SCP079", "Lockdown cannot commence, not enough mana.", MessageImportance.LessImportant, false);
                                            return false;
                                        }
                                }
                            }
                            else
                            {
                                GameCore.Console.AddDebugLog("SCP079", "Room couldn't be specified.", MessageImportance.Normal, false);
                            }
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079BulkPatch failed!!\n{e}");
                return false;
            }
        }
    }
}