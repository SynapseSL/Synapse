using HarmonyLib;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Spawnpoints;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("PlayerEscape", PatchType.PlayerEvent)]
public static class PlayerEscapePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Escape), nameof(Escape.ServerHandlePlayer))]
    public static bool OnEscape(ReferenceHub hub)
    {
        var player = hub.GetSynapsePlayer();
        if (player == null) return false;
        player.TriggerEscape(false);
        return false;
    }
}

[Automatic]
[SynapsePatch("SetClass", PatchType.PlayerEvent)]
public static class SetClassPatch
{
    private static readonly PlayerEvents PlayerEvents;
    static SetClassPatch() => PlayerEvents = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
    public static bool SetClass(PlayerRoleManager __instance, out SetClassEvent __state, ref RoleTypeId newRole,
        ref RoleChangeReason reason)

    {
        __state = new SetClassEvent(__instance.Hub.GetSynapsePlayer(), newRole, reason);
        if (PlayerRoleLoader.TryGetRoleTemplate<FpcStandardRoleBase>(newRole, out var rolePrefab) &&
            rolePrefab.SpawnpointHandler != null &&
            rolePrefab.SpawnpointHandler.TryGetSpawnpoint(out var pos, out var rot))
        {
            __state.Position = pos;
            __state.HorizontalRotation = rot;
        }

        PlayerEvents.SetClass.RaiseSafely(__state);
        newRole = __state.Role;
        reason = __state.SpawnReason;
        return __state.Allow;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
    public static void PostSetClass(SetClassEvent __state)
    {
        if (!typeof(FpcStandardRoleBase).IsAssignableFrom(FakeRoleManager.EnumToType[__state.Role])) return;
        
        __state.Player.Hub.transform.position = __state.Position;
        __state.Player.FirstPersonMovement.MouseLook.CurrentHorizontal = __state.HorizontalRotation;
    }
}