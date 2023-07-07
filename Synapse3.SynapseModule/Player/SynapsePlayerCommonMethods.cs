using System;
using Hints;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Enums;
using PluginAPI.Events;
using RemoteAdmin;
using Respawning;
using RoundRestarting;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Role;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// Kicks the Player from the Server with a specified Reason
    /// </summary>
    public void Kick(string message) => ServerConsole.Disconnect(gameObject, message);

    /// <summary>
    /// Bans the Player
    /// </summary>
    public void Ban(long duration, string reason, SynapsePlayer issuer = null) =>
        BanPlayer.BanUser(Hub, issuer ?? _player.Host, reason, duration);
    
    /// <summary>
    /// Returns a uint Value that corresponds to the players favorite Attachments for a specific Weapon
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public uint GetPreference(ItemType item)
    {
        if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(Hub, out var dict) && dict.TryGetValue(item, out var result))
            return result;

        return 0;
    }
    
    /// <summary>
    /// Opens the Window that is usually used for Reports with a Custom Message
    /// </summary>
    public virtual void SendWindowMessage(string text) => GameConsoleTransmission.SendToClient(Connection, "[REPORTING] " + text, "white");
    
    /// <summary>
    /// Displays a hint on the Player's screen, that override the hint list!
    /// </summary>
    public void SendHint(string message, float duration = 5f)
    {
        Hub.hints.Show(new TextHint(message, new HintParameter[]
        {
            new StringHintParameter("")
        }, HintEffectPresets.FadeInAndOut(duration), duration));
    }

    /// <summary>
    /// Displays a Broadcast on the Player's screen
    /// </summary>
    public Broadcast SendBroadcast(string message, ushort time, bool instant = false)
    {
        if (PlayerType == PlayerType.Server)
        {
            NeuronLogger.For<Synapse>().Info($"Broadcast: {message}", ConsoleColor.White);
            return null;
        }

        Broadcast bc = new(message, time, this);
        ActiveBroadcasts.Add(bc, instant);
        return bc;
    }

    /// <summary>
    /// Creates the broadcast
    /// </summary>
    /// <param name="time"></param>
    /// <param name="message"></param>
    internal void Broadcast(ushort time, string message) =>
        BroadcastController.TargetAddElement(Connection, message, time, new global::Broadcast.BroadcastFlags());

    /// <summary>
    /// Removes the currently displayed broadcast
    /// </summary>
    internal void ClearBroadcasts() => BroadcastController.TargetClearElements(Connection);

    /// <summary>
    /// Shows instantly the new broadcast
    /// </summary>
    /// <param name="time"></param>
    /// <param name="message"></param>
    internal void InstantBroadcast(ushort time, string message)
    {
        ClearBroadcasts();
        Broadcast(time, message);
    }

    /// <summary>
    /// Sends a Message to the Player in his Console
    /// </summary>
    public void SendConsoleMessage(string message, string color = "red") => ClassManager.ConsolePrint(message, color);

    /// <summary>
    /// Sends a Message to the Player in his (Text)RemoteAdmin
    /// </summary>
    public void SendRaConsoleMessage(string message, bool success = true, RaCategory type = RaCategory.None,
        string sender = "") =>
        CommandSender.RaMessage(message, success, type, string.IsNullOrWhiteSpace(sender) ? "Synapse" : sender);

    /// <summary>
    /// Gives the Player an effect
    /// </summary>
    public void GiveEffect(Effect effect, byte intensity = 1, float duration = -1f, bool addDuration = false) =>
        PlayerEffectsController.AllEffects[(int)effect].ServerSetState(intensity, duration, addDuration);

    /// <summary>
    /// Heals the Player the specific amount without over healing him
    /// </summary>
    public void Heal(float hp) => GetStatBase<HealthStat>().ServerHeal(hp);

    /// <summary>
    /// Hurts and if enough kills the Player
    /// </summary>
    public bool Hurt(float damage, DamageType type = DamageType.Unknown)
    {
        var handler = type.GetUniversalDamageHandler();
        handler.Damage = damage;
        return PlayerStats.DealDamage(handler);
    }

    /// <summary>
    /// Hurts and if enough kills the Player
    /// </summary>
    public bool Hurt(DamageHandlerBase handlerBase) => PlayerStats.DealDamage(handlerBase);

    /// <summary>
    /// Kills the Player
    /// </summary>
    public bool Kill() => Kill("Unknown Reason");

    /// <summary>
    /// Kills the Player
    /// </summary>
    public bool Kill(string reason) => PlayerStats.DealDamage(new CustomReasonDamageHandler(reason));

    /// <summary>
    /// Kills the Player
    /// </summary>
    public bool Kill(string reason, string cassie)
    {
        var result = Kill(reason);
        if (result)
            _cassie.Announce(cassie);
        return result;
    }

    /// <summary>
    /// Removes one of the Information of the Player that can be seen when someone else looks at him
    /// </summary>
    /// <param name="playerInfo"></param>
    public void RemoveDisplayInfo(PlayerInfoArea playerInfo) => NicknameSync.Network_playerInfoToShow &= ~playerInfo;

    /// <summary>
    /// Add one of the Information of the Player that can be seen when someone else looks at him
    /// </summary>
    public void AddDisplayInfo(PlayerInfoArea playerInfo) => NicknameSync.Network_playerInfoToShow |= playerInfo;

    /// <summary>
    /// Executes in the name of the Player a Command
    /// </summary>
    public void ExecuteCommand(string command, bool remoteAdmin = true)
    {
        if (remoteAdmin) CommandProcessor.ProcessQuery(command, CommandSender);
        else QueryProcessor.ProcessGameConsoleQuery(command);
    }

    /// <summary>
    /// Redirects the Player to another Server on the same IP with a different port
    /// </summary>
    public void SendToServer(ushort port)
        => Connection.Send(new RoundRestartMessage(RoundRestartType.RedirectRestart, 1f, port, true, false));

    /// <summary>
    /// Turns the Screen of the Player for the entire Round black
    /// </summary>
    public void DimScreen()
        => Connection.Send(_mirror.GetCustomRpcMessage(RoundSummary.singleton, nameof(RoundSummary.RpcDimScreen),
            null));

    /// <summary>
    /// Shakes the Screen of the Player like the Alpha Warhead
    /// </summary>
    public void ShakeScreen(bool achieve = false) => SendNetworkMessage(_mirror.GetCustomRpcMessage(
        AlphaWarheadController.Singleton, nameof(AlphaWarheadController.RpcShake),
        writer =>
        {
            writer.WriteBool(achieve);
        }));

    /// <summary>
    /// Opens the Menu of the Player
    /// </summary>
    /// <param name="menu"></param>
    public void OpenMenu(MenuType menu)
    {
        var menutype = "";

        switch (menu)
        {
            case MenuType.Menu:
                menutype = "NewMainMenu";
                break;

            case MenuType.OldFastMenu:
                menutype = "FastMenu";
                break;

            case MenuType.OldMenu:
                menutype = "MainMenuRemastered";
                break;
        }

        Connection.Send(new SceneMessage
        {
            sceneName = menutype,
            sceneOperation = SceneOperation.Normal,
            customHandling = false
        });
    }

    private EscapeType GetEscapeType(bool ignoreEscapeDistance)
    {
        var disarmed = IsDisarmed;
        var fpcRole = CurrentRole as FpcStandardRoleBase;
        if (fpcRole == null && !ignoreEscapeDistance) return EscapeType.TooFarAway;
        if (!ignoreEscapeDistance && (fpcRole.FpcModule.Position - Escape.WorldPos).sqrMagnitude > Escape.RadiusSqr) return EscapeType.TooFarAway;
        
        if (CurrentRole.ActiveTime < 10f) return EscapeType.TooEarly;
        if (disarmed && Disarmer?.CustomTeam?.Attribute.EvacuatePlayers == true)
            return EscapeType.CustomTeamEvacuate;
        if (HasCustomRole) return EscapeType.CustomRole;
        if (CurrentRole is not HumanRole) return EscapeType.NotAssigned;

        if (IsDisarmed && !CharacterClassManager.CuffedChangeTeam) return EscapeType.NotAssigned;
        switch (RoleType)
        {
            case RoleTypeId.ClassD when disarmed:
                return EscapeType.CuffedClassD;
            
            case RoleTypeId.ClassD:
                return EscapeType.ClassD;
            
            case RoleTypeId.Scientist when disarmed:
                return EscapeType.CuffedScientist;
            
            case RoleTypeId.Scientist:
                return EscapeType.Scientist;
            
            default: return EscapeType.NotAssigned;
        }
    }

    public void TriggerEscape(bool ignoreEscapeDistance = true)
    {
        var state = GetEscapeType(ignoreEscapeDistance);
        var ev = new EscapeEvent(this, true, state);
        _playerEvents.Escape.RaiseSafely(ev);

        RoleTypeId vanillaRole;
        switch (ev.EscapeType)
        {
            case EscapeType.CustomRole:
                CustomRole?.TryEscape();
                return;
            
            case EscapeType.CustomTeamEvacuate:
                Disarmer?.CustomTeam?.EvacuatePlayer(this);
                return;
            
            case EscapeType.PluginOverride:
                SendNetworkMessage(new Escape.EscapeMessage()
                {
                    ScenarioId = (byte)Escape.EscapeScenarioType.ClassD,
                    EscapeTime = (ushort)Mathf.CeilToInt(CurrentRole.ActiveTime)
                });
                if (_role.IsIdVanila(ev.OverrideRole))
                    RoleManager.ServerSetRole((RoleTypeId)ev.OverrideRole, RoleChangeReason.Escaped);
                else RoleID = ev.OverrideRole;
                return;
            
            case EscapeType.ClassD:
            case EscapeType.CuffedScientist:
                vanillaRole = RoleTypeId.ChaosConscript;
                break;
            
            case EscapeType.CuffedClassD:
                vanillaRole = RoleTypeId.NtfPrivate;
                break;
            
            case EscapeType.Scientist:
                vanillaRole = RoleTypeId.NtfSpecialist;
                break;
            
            case EscapeType.NotAssigned:
            case EscapeType.TooFarAway:
            case EscapeType.TooEarly:
            case EscapeType.None:
            default:
                return;
        }

        if (!EventManager.ExecuteEvent(ServerEventType.PlayerEscape, Hub, vanillaRole)) return;

        switch (ev.EscapeType)
        {
            case EscapeType.ClassD:
            case EscapeType.CuffedScientist:
                RespawnTokensManager.GrantTokens(SpawnableTeamType.ChaosInsurgency, Escape.InsurgencyEscapeReward);
                break;
            
            case EscapeType.CuffedClassD:
                RespawnTokensManager.GrantTokens(SpawnableTeamType.NineTailedFox, Escape.FoundationEscapeReward);
                break;
            
            case EscapeType.Scientist:
                RespawnTokensManager.GrantTokens(SpawnableTeamType.NineTailedFox, Escape.FoundationEscapeReward);
                break;
        }
        
        SendNetworkMessage(new Escape.EscapeMessage()
        {
            ScenarioId = (byte)ev.EscapeType,
            EscapeTime = (ushort)Mathf.CeilToInt(CurrentRole.ActiveTime)
        });
        RoleManager.ServerSetRole(vanillaRole, RoleChangeReason.Escaped);
    }
}