using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using NorthwoodLib.Pools;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp079
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdInteract))]
    public static class Scp079BulkPatch
    {
        public static bool Prefix(Scp079PlayerScript __instance, string command, GameObject target)
        {
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
                GameCore.Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant, false);
                if (!command.Contains(":"))
                {
                    return false;
                }
                string[] array = command.Split(':');
                __instance.RefreshCurrentRoom();
                if (!__instance.CheckInteractableLegitness(__instance.currentRoom, __instance.currentZone, target, true))
                {
                    return false;
                }
                DoorVariant doorVariant = null;
                bool flag = target != null && target.TryGetComponent<DoorVariant>(out doorVariant);
                List<string> list = GameCore.ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();
                string text = array[0];

                switch (text)
                {
                    case "TESLA":
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Tesla Gate Burst", __instance.abilities);
                            GameObject gameObject = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom + "/Gate");
                            Tesla tesla = null;
                            if (gameObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                tesla = Server.Get.Map.Teslas.Find(_ => _.Gate == gameObject.GetComponent<TeslaGate>());
                            }

                            var intendedResult = manaFromLabel <= __instance.curMana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;
                            
                            SynapseController.Server.Events.Scp.Scp079.Invoke079TeslaInteract(
                                __instance.gameObject.GetPlayer(),
                                manaFromLabel,
                                tesla.Room,
                                tesla,
                                intendedResult,
                                out var actualResult
                                );

                            switch (actualResult)
                            {
                                case Scp079EventMisc.InteractionResult.Allow:
                                    {
                                        tesla.InstantTrigger();
                                        __instance.AddInteractionToHistory(gameObject, array[0], true);
                                        __instance.Mana -= manaFromLabel;
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy:
                                    {
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                        return false;
                                    }
                            }
                            break;
                        }
                    case "LOCKDOWN":
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                GameCore.Console.AddDebugLog("SCP079", "Lockdown cannot commence, Warhead in progress.", MessageImportance.LessImportant, false);
                                return false;
                            }
                            float manaFromLabel = __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);
                            GameCore.Console.AddDebugLog("SCP079", "Attempting lockdown...", MessageImportance.LeastImportant, false);
                            GameObject roomGameObject = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom);
                            if (roomGameObject != null)
                            {
                                List<Scp079Interactable> localInteractableList = ListPool<Scp079Interactable>.Shared.Rent();
                                try
                                {
                                    foreach (Scp079Interactable scp079Interactable4 in Interface079.singleton.allInteractables)
                                    {
                                        if (scp079Interactable4 != null)
                                        {
                                            foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interactable4.currentZonesAndRooms)
                                            {
                                                if (zoneAndRoom.currentRoom == __instance.currentRoom && zoneAndRoom.currentZone == __instance.currentZone && scp079Interactable4.transform.position.y - 100f < __instance.currentCamera.transform.position.y && !localInteractableList.Contains(scp079Interactable4))
                                                {
                                                    localInteractableList.Add(scp079Interactable4);
                                                }
                                            }
                                        }
                                    }
                                    GameCore.Console.AddDebugLog("SCP079", "Loaded all interactables", MessageImportance.LeastImportant, false);
                                }
                                catch
                                {
                                    GameCore.Console.AddDebugLog("SCP079", "Failed to load interactables.", MessageImportance.LeastImportant, false);
                                }
                                GameObject interactableLockdownGameObject = null;
                                foreach (Scp079Interactable interactable in localInteractableList)
                                {
                                    Scp079Interactable.InteractableType interactableType = interactable.type;
                                    if (interactableType != Scp079Interactable.InteractableType.Door)
                                    {
                                        if (interactableType == Scp079Interactable.InteractableType.Lockdown)
                                        {
                                            interactableLockdownGameObject = interactable.gameObject;
                                        }
                                    }
                                    else
                                    {
                                        if (interactable.TryGetComponent(out DoorVariant doorVariant2))
                                        {
                                            Synapse.Api.Door door = doorVariant2.GetDoor();
                                            if (door.VDoor is BreakableDoor bDoor && bDoor.IsDestroyed)
                                            {
                                                GameCore.Console.AddDebugLog("SCP079", "Lockdown can't initiate, one of the doors were destroyed.", MessageImportance.LessImportant, false);
                                                return false;
                                            }
                                        }
                                    }
                                }
                                if (localInteractableList.Count == 0 || interactableLockdownGameObject == null || __instance._scheduledUnlocks.Count > 0)
                                {
                                    GameCore.Console.AddDebugLog("SCP079", "This room can't be locked down.", MessageImportance.LessImportant, false);
                                    return false;
                                }

                                HashSet<DoorVariant> doorHashSet = new HashSet<DoorVariant>();
                                GameCore.Console.AddDebugLog("SCP079", "Looking for doors to lock...", MessageImportance.LeastImportant, false);
                                for (int i = 0; i < localInteractableList.Count; i++)
                                {
                                    if (localInteractableList[i].TryGetComponent(out DoorVariant doorVariant3))
                                    {
                                        bool flag2 = doorVariant3.ActiveLocks == 0;
                                        if (!flag2)
                                        {
                                            DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)doorVariant3.ActiveLocks);
                                            flag2 = (mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.ScpOverride));
                                        }
                                        if (flag2)
                                        {
                                            doorHashSet.Add(doorVariant3);
                                        }
                                    }
                                }

                                List<DoorVariant> affectedDoors = doorHashSet.ToList();
                                var intendedResult = manaFromLabel <= __instance.curMana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;
                                bool lightsOut = true;

                                SynapseController.Server.Events.Scp.Scp079.Invoke079RoomLockdown(
                                    __instance.gameObject.GetPlayer(),
                                    manaFromLabel,
                                    Server.Get.Map.Rooms.Find(_ => _.GameObject == roomGameObject),
                                    ref lightsOut,
                                    intendedResult,
                                    out var actualResult
                                    );

                                switch (actualResult)
                                {
                                    case Scp079EventMisc.InteractionResult.Allow:
                                        {
                                            if (lightsOut)
                                            {
                                                foreach (FlickerableLightController flickerableLightController in roomGameObject.GetComponentsInChildren<FlickerableLightController>())
                                                {
                                                    if (flickerableLightController != null)
                                                    {
                                                        flickerableLightController.ServerFlickerLights(8f);
                                                    }
                                                }
                                            }

                                            for (int i = 0; i < affectedDoors.Count; i++)
                                            {
                                                bool flag2 = affectedDoors[i].ActiveLocks == 0;
                                                if (!flag2)
                                                {
                                                    DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)affectedDoors[i].ActiveLocks);
                                                    flag2 = (mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.ScpOverride));
                                                }
                                                if (flag2)
                                                {
                                                    if (affectedDoors[i].TargetState)
                                                    {
                                                        affectedDoors[i].NetworkTargetState = false;
                                                    }
                                                    affectedDoors[i].ServerChangeLock(DoorLockReason.Lockdown079, true);
                                                }
                                            }

                                            if (doorHashSet.Count > 0)
                                            {
                                                __instance._scheduledUnlocks.Add(Time.realtimeSinceStartup + 10f, doorHashSet);
                                                GameCore.Console.AddDebugLog("SCP079", "Locking " + doorHashSet.Count + " doors", MessageImportance.LeastImportant, false);
                                            }
                                            else
                                            {
                                                GameCore.Console.AddDebugLog("SCP079", "No doors to lock found, code " + (from x in localInteractableList
                                                                                                                          where x.type == Scp079Interactable.InteractableType.Door
                                                                                                                          select x).Count(), MessageImportance.LessImportant, false);
                                            }
                                            ListPool<Scp079Interactable>.Shared.Return(localInteractableList);

                                            GameCore.Console.AddDebugLog("SCP079", "Lockdown initiated.", MessageImportance.LessImportant, false);
                                            __instance.AddInteractionToHistory(interactableLockdownGameObject, array[0], true);
                                            __instance.Mana -= __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);
                                            return false;
                                        }
                                    case Scp079EventMisc.InteractionResult.Disallow:
                                        {
                                            return false;
                                        }
                                    case Scp079EventMisc.InteractionResult.NoEnergy:
                                        {
                                            __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                            GameCore.Console.AddDebugLog("SCP079", "Lockdown cannot commence, not enough mana.", MessageImportance.LessImportant, false);
                                            return false;
                                        }
                                }

                            }
                            else
                            {
                                GameCore.Console.AddDebugLog("SCP079", "Room couldn't be specified.", MessageImportance.Normal, false);
                            }
                            break;
                        }
                    case "ELEVATORTELEPORT":
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Elevator Teleport", __instance.abilities);
                            if (manaFromLabel > __instance.curMana)
                            {
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
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
                                __instance.AddInteractionToHistory(target, array[0], true);
                                return false;
                            }
                            Color32 color;
                            if (ConsoleDebugMode.CheckImportance("SCP079", MessageImportance.LeastImportant, out color))
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
                                if (scp079Interactable2 == null)
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
                                if (scp079Interactable2.optionalObject == null)
                                {
                                    text2 = "Optional object is missing.";
                                }
                                else if (scp079Interactable2.optionalObject.GetComponent<Camera079>() == null)
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
                            break;
                        }
                    case "ELEVATORUSE":
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Elevator Use", __instance.abilities);
                            string elevatorName = string.Empty;
                            if (array.Length > 1)
                            {
                                elevatorName = array[1];
                            }
                            Elevator synElevator = Map.Get.Elevators.Find(_ => _.Name == elevatorName);
                            Scp079EventMisc.InteractionResult intendedResult;
                            if (manaFromLabel <= __instance.curMana)
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.NoEnergy;
                            }
                            else if (synElevator == null || (AlphaWarheadController.Host.timeToDetonation == 0f || !synElevator.Operative || synElevator.Locked))
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Disallow;
                            }
                            else
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Allow;
                            }

                            intendedResult = manaFromLabel <= __instance.curMana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;

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
                                        if (synElevator.Lift.UseLift())
                                        {
                                            __instance.Mana -= manaFromLabel;
                                            bool flag3 = false;
                                            foreach (Lift.Elevator elevator in synElevator.Lift.elevators)
                                            {
                                                __instance.AddInteractionToHistory(elevator.door.GetComponentInParent<Scp079Interactable>().gameObject, array[0], !flag3);
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
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                        return false;
                                    }
                            }

                            break;
                        }
                    case "DOORLOCK":
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                return false;
                            }
                            if (target == null)
                            {
                                GameCore.Console.AddDebugLog("SCP079", "The door lock command requires a target.", MessageImportance.LessImportant, false);
                                return false;
                            }
                            if (doorVariant == null)
                            {
                                return false;
                            }
                            if (doorVariant.TryGetComponent(out DoorNametagExtension doorNametagExtension))
                            {
                                if (list != null && list.Count > 0 && list.Contains(doorNametagExtension.GetName))
                                {
                                    GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                                    return false;
                                }
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
                                intendedResult = manaFromLabel <= __instance.curMana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;
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
                                        __instance.AddInteractionToHistory(doorVariant.gameObject, array[0], true);
                                        __instance.Mana -= __instance.GetManaFromLabel("Door Lock Start", __instance.abilities);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow when action == Scp079EventMisc.DoorAction.Unlocking:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.Disallow when action == Scp079EventMisc.DoorAction.Locking:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy when action == Scp079EventMisc.DoorAction.Unlocking:
                                    {
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy when action == Scp079EventMisc.DoorAction.Locking:
                                    {
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                        return false;
                                    }
                            }
                            break;
                        }
                    case "SPEAKER":
                        {
                            string text3 = __instance.currentZone + "/" + __instance.currentRoom + "/Scp079Speaker";
                            GameObject gameObject = GameObject.Find(text3);
                            float manaFromLabel = __instance.GetManaFromLabel("Speaker Start", __instance.abilities) * 1.5f;

                            Scp079EventMisc.InteractionResult intendedResult;
                            if (gameObject == null)
                            {
                                intendedResult = Scp079EventMisc.InteractionResult.Disallow;
                            }
                            else if (manaFromLabel <= __instance.curMana)
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
                                        __instance.Speaker = text3;
                                        __instance.AddInteractionToHistory(gameObject, array[0], true);
                                        return false;
                                    }
                                case Scp079EventMisc.InteractionResult.NoEnergy:
                                    {
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                        return false;
                                    }
                            }
                            break;
                        }
                    case "STOPSPEAKER":
                        {
                            __instance.Speaker = string.Empty;
                            break;
                        }
                    case "DOOR":
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                return false;
                            }
                            if (target == null)
                            {
                                GameCore.Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant, false);
                                return false;
                            }
                            if (!flag)
                            {
                                return false;
                            }
                            if (doorVariant.TryGetComponent(out DoorNametagExtension doorNametagExtension2))
                            {
                                if (list != null && list.Count > 0 && list.Contains(doorNametagExtension2.GetName))
                                {
                                    GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                                    return false;
                                }
                            }
                            string text4 = doorVariant.RequiredPermissions.RequiredPermissions.ToString();
                            float manaFromLabel = __instance.GetManaFromLabel("Door Interaction " + (text4.Contains(",") ? text4.Split(',')[0] : text4), __instance.abilities);

                            var action = doorVariant.TargetState ? Scp079EventMisc.DoorAction.Closing : Scp079EventMisc.DoorAction.Opening;
                            var intendedResult = manaFromLabel <= __instance.curMana ? Scp079EventMisc.InteractionResult.Allow : Scp079EventMisc.InteractionResult.NoEnergy;
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
                                            __instance.AddInteractionToHistory(target, array[0], true);
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
                                        __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                        return false;
                                    }
                            }
                            break;
                        }
                }

            }
            catch (System.Exception e)
            {
                Synapse.Api.Logger.Get.Warn($"{e}");
            }

            return false;
        }
    }
}
