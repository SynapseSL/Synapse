using System;
using System.Collections.Generic;
using System.Net;

namespace Synapse.RCE
{
    internal class RceHandler
    {
        internal Queue<QueueAction> ActionQueue { get; } = new();

        private SynapseRceServer _rceHandler;


        internal void Init()
        {
            Api.Events.EventHandler.Get.Server.UpdateEvent += DequeueInConcurrentUnityContext;
        }

        internal void Reload()
        {
            if (Server.Get.Configs.SynapseConfiguration.UseLocalRceServer)
            {
                // only activate if it hasnt been activated yet
                if (_rceHandler is null)
                {
                    _rceHandler = new(IPAddress.Loopback, Server.Get.Configs.SynapseConfiguration.RceServerPort);
                    _rceHandler.Start();

                    Api.Logger.Get.Info("RCE Server started!");
                }
            }
            else
            {
                // only deactivate if it has been activated yet
                if (_rceHandler is not null)
                {
                    _rceHandler.Stop();
                    _rceHandler = null;

                    Api.Logger.Get.Info("RCE Server stopped!");
                }
            }
        }

        private void DequeueInConcurrentUnityContext()
        {
            if (ActionQueue.Count != 0)
            {
                var qAction = ActionQueue.Dequeue();
                try
                {
                    qAction.Action.Invoke();
                }
                catch (Exception e) // outer exception is TargetInvocationException
                {
                    qAction.Exception = e.InnerException;
                }
                finally
                {
                    qAction.Ran = true;
                }
            }
        }
    }
}