using LiteNetLib;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class ServerEvents
    {
        internal ServerEvents() { }

        public event EventHandler.OnSynapseEvent<PreAuthenticationEventArgs> PreAuthenticationEvent;

        internal void InvokePreAuthenticationEvent(string userid, ref bool allow, ref string reason, ConnectionRequest request)
        {
            var ev = new PreAuthenticationEventArgs {Allow = allow, Request = request,UserId = userid, Reason = reason};
            PreAuthenticationEvent?.Invoke(ev);

            allow = ev.Allow;
            reason = ev.Reason;
        }
    }
}
