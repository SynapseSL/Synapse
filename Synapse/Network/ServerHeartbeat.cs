using System;
using System.Threading.Tasks;
using Synapse.Api;
using Synapse.Reactive;

namespace Synapse.Network
{
    public class ServerHeartbeat : JavaLikeThread
    {
        public NotificationSubject HeartbeatSubject { get; } = new NotificationSubject();

        public override async void Run()
        {
            while (true)
            {
                try
                {
                    HeartbeatSubject.Notify();
                }
                catch (Exception e)
                {
                    Logger.Get.Error(e);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}