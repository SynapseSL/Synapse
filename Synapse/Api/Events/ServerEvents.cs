using LiteNetLib;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class ServerEvents
    {
        internal ServerEvents() { }

        public event EventHandler.OnSynapseEvent<PreAuthenticationEventArgs> PreAuthenticationEvent;

        public event EventHandler.OnSynapseEvent<RemoteAdminCommandEventArgs> RemoteAdminCommandEvent;

        public event EventHandler.OnSynapseEvent<ConsoleCommandEventArgs> ConsoleCommandEvent;

        public event EventHandler.OnSynapseEvent<TransmitPlayerDataEventArgs> TransmitPlayerDataEvent;

        public event Action UpdateEvent;

        internal void InvokePreAuthenticationEvent(string userid, ref bool allow, ref string reason, ConnectionRequest request)
        {
            var ev = new PreAuthenticationEventArgs { Allow = allow, Request = request, UserId = userid, Reason = reason };
            PreAuthenticationEvent?.Invoke(ev);

            allow = ev.Allow;
            reason = ev.Reason;
        }

        internal void InvokeRemoteAdminCommandEvent(CommandSender sender, string command, ref bool allow)
        {
            var ev = new RemoteAdminCommandEventArgs { Allow = allow, Command = command, Sender = sender };
            RemoteAdminCommandEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeConsoleCommandEvent(Player player, string command)
        {
            var ev = new ConsoleCommandEventArgs { Command = command, Player = player };
            ConsoleCommandEvent?.Invoke(ev);
        }

        internal void InvokeUpdateEvent() => UpdateEvent?.Invoke();

        internal void InvokeTransmitPlayerDataEvent(Player player, Player playerToShow, ref Vector3 pos, ref float rot, ref bool invisible)
        {
            var ev = new TransmitPlayerDataEventArgs
            {
                Player = player,
                PlayerToShow = playerToShow,
                Position = pos,
                Rotation = rot,
                Invisible = invisible
            };

            TransmitPlayerDataEvent?.Invoke(ev);

            pos = ev.Position;
            rot = ev.Rotation;
            invisible = ev.Invisible;
        }
    }
}
