using System.Threading;

namespace Synapse.Network
{
    public abstract class JavaLikeThread
    {
        public readonly Thread Thread;

        public JavaLikeThread()
        {
            Thread = new Thread(Run);
        }

        public abstract void Run();

        public void Start(bool isBackground = false)
        {
            Thread.IsBackground = isBackground;
            Thread.Start();
        }

        public void Stop()
        {
            Thread.Abort();
        }
    }
}