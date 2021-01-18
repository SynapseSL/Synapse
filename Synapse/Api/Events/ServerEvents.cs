using LiteNetLib;
using Synapse.Api.Events.SynapseEventArguments;
using System;

namespace Synapse.Api.Events
{
    public class ServerEvents
    {
        internal ServerEvents() { }

        public event EventHandler.OnSynapseEvent<PreAuthenticationEventArgs> PreAuthenticationEvent;

        public event EventHandler.OnSynapseEvent<RemoteAdminCommandEventArgs> RemoteAdminCommandEvent;

        public event EventHandler.OnSynapseEvent<ConsoleCommandEventArgs> ConsoleCommandEvent;

        public event Action UpdateEvent;

        internal void InvokePreAuthenticationEvent(string userid, ref bool allow, ref string reason, ConnectionRequest request)
        {
            var ev = new PreAuthenticationEventArgs {Allow = allow, Request = request,UserId = userid, Reason = reason};
            PreAuthenticationEvent?.Invoke(ev);

            allow = ev.Allow;
            reason = ev.Reason;
        }

        internal void InvokeRemoteAdminCommandEvent(CommandSender sender, string command, ref bool allow)
        {
            var ev = new RemoteAdminCommandEventArgs {Allow = allow, Command = command, Sender = sender};
            RemoteAdminCommandEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeConsoleCommandEvent(Player player, string command)
        {
            var ev = new ConsoleCommandEventArgs {Command = command, Player = player};
            ConsoleCommandEvent?.Invoke(ev);
        }

        internal void InvokeUpdateEvent() => UpdateEvent?.Invoke();
    }
}
