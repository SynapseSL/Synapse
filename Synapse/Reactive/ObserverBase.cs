using System;
using System.Threading.Tasks;

namespace Synapse.Network
{
    public abstract class ObserverBase<T> : IObserver<T>
    {
        public void OnNext(T value)
        {
            Consume(value);
        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnCompleted()
        {
        }

        public abstract void Consume(T obj);
    }

    public class Consumer<T> : ObserverBase<T>
    {
        public Action<T> ConsumerAction;

        public Consumer(Action<T> consumerAction)
        {
            ConsumerAction = consumerAction;
        }

        public override void Consume(T obj)
        {
            ConsumerAction.Invoke(obj);
        }
    }

    public class OneShotConsumer<T>
    {
        public Consumer<T> Consumer;
        public IObservable<T> Observable;

        public OneShotConsumer(IObservable<T> observable)
        {
            Observable = observable;
        }

        public async Task<T> Consume()
        {
            var completer = new TaskCompletionSource<T>();
            Consumer = new Consumer<T>(x => { completer.TrySetResult(x); });
            var disposable = Observable.Subscribe(Consumer);
            var result = await completer.Task;
            disposable.Dispose();
            Server.Get.Logger.Info("ConsumedOnce!");
            return result;
        }
    }
}