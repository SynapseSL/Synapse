using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Reactive
{
    public abstract class SubscribableBase<T> : IObservable<T>
    {
        private List<IObserver<T>> Observers { get; } = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Observers.Add(observer);
            return new Unsubscriber<T>(Observers, observer);
        }

        public void Publish(T evt)
        {
            foreach (var observer in Observers.ToList()) observer.OnNext(evt);
        }

        public void Complete()
        {
            foreach (var observer in Observers.ToList()) observer.OnCompleted();
        }

        public void Error(Exception exception)
        {
            foreach (var observer in Observers.ToList()) observer.OnError(exception);
        }
    }

    public class PublishSubject<T> : SubscribableBase<T>
    {
    }

    public class NotificationSubject : SubscribableBase<object>
    {
        public void Notify()
        {
            Publish(null);
        }
    }
}