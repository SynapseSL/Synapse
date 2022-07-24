using System;
using Hints;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using RoundRestarting;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
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
    public void Ban(int duration, string reason, string issuer = "Plugin") => Synapse.GetObject<BanPlayer>().BanUser(gameObject, duration, reason, issuer);
    
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
    /// Displays a hint on the Player's screen
    /// </summary>
    public void GiveTextHint(string message, float duration = 5f)
    {
        Hub.hints.Show(new TextHint(message, new HintParameter[]
        {
            new StringHintParameter("")
        }, HintEffectPresets.FadeInAndOut(duration), duration));
    }

    /// <summary>
    /// Displays a Broadcast on the Player's screen
    /// </summary>
    public Broadcast SendBroadcast(ushort time, string message, bool instant = false)
    {
        if (PlayerType == PlayerType.Server)
            NeuronLogger.For<Synapse>().Info($"Broadcast: {message}", ConsoleColor.White);

        Broadcast bc = new(message, time, this);
        ActiveBroadcasts.Add(bc, instant);
        return bc;
    }
    
    /// <summary>
    /// Creates the broadcast
    /// </summary>
    /// <param name="time"></param>
    /// <param name="message"></param>
    internal void Broadcast(ushort time, string message) => GetComponent<global::Broadcast>()
        .TargetAddElement(Connection, message, time, new global::Broadcast.BroadcastFlags());

    /// <summary>
    /// Removes the currently displayed broadcast
    /// </summary>
    internal void ClearBroadcasts() => GetComponent<global::Broadcast>().TargetClearElements(Connection);

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
    public void SendConsoleMessage(string message, string color = "red") => ClassManager.TargetConsolePrint(Connection, message, color);

    /// <summary>
    /// Sends a Message to the Player in his (Text)RemoteAdmin
    /// </summary>
    public void SendRaConsoleMessage(string message, bool success = true, RaCategory type = RaCategory.None) => CommandSender.RaMessage(message, success, type);

    /// <summary>
    /// Gives the Player an effect
    /// </summary>
    public void GiveEffect(Effect effect, byte intensity = 1, float duration = -1f) => PlayerEffectsController.ChangeByString(effect.ToString().ToLower(), intensity, duration);

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
    public bool Hurt(DamageHandlerBase handlerbase) => PlayerStats.DealDamage(handlerbase);

    /// <summary>
    /// Kills the Player
    /// </summary>
    public void Kill() => Kill("Unknown Reason");

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
            Synapse.Get<CassieService>().Announce(cassie);
        return result;
    }
    
    /// <summary>
    /// Opens the Window that is usually used for Reports with a Custom Message
    /// </summary>
    public void OpenReportWindow(string text) => GameConsoleTransmission.SendToClient(Connection, "[REPORTING] " + text, "white");

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
    public void ExecuteCommand(string command, bool RA = true)
    {
        if (RA) RemoteAdmin.CommandProcessor.ProcessQuery(command, CommandSender);
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
        {
            var component = RoundSummary.singleton;
            var writer = NetworkWriterPool.GetWriter();
            var msg = new RpcMessage
            {
                netId = component.netId,
                componentIndex = component.ComponentIndex,
                functionHash = typeof(RoundSummary).FullName.GetStableHashCode() * 503 + "RpcDimScreen".GetStableHashCode(),
                payload = writer.ToArraySegment()
            };
            Connection.Send(msg);
            NetworkWriterPool.Recycle(writer);
        }

    /// <summary>
    /// Shakes the Screen of the Player like the Alpha Warhead
    /// </summary>
    public void ShakeScreen(bool achieve = false)
        => AlphaWarheadController.Host.TargetRpcShake(Connection, achieve, GodMode);

    /// <summary>
    /// Places Blood locally on the Map of the Player
    /// </summary>
    public void PlaceBlood(Vector3 pos, int type = 1, float size = 2f)
    {
        var component = ClassManager;
        var writer = NetworkWriterPool.GetWriter();
        writer.WriteVector3(pos);
        writer.WriteInt32(type);
        writer.WriteSingle(size);
        var msg = new RpcMessage
        {
            netId = component.netId,
            componentIndex = component.ComponentIndex,
            functionHash = typeof(CharacterClassManager).FullName.GetStableHashCode() * 503 + "RpcPlaceBlood".GetStableHashCode(),
            payload = writer.ToArraySegment()
        };
        Connection.Send(msg);
        NetworkWriterPool.Recycle(writer);
    }

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

    public void TriggerEscape()
    {
        if (CustomRole != null)
        {
            CustomRole.TryEscape();
            return;
        }
    }
}