using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Network
{
    public abstract class SubscribableBase<T> : IObservable<T>
    {
        private List<IObserver<T>> _observers { get; } = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<T>(_observers, observer);
        }

        public void Publish(T evt)
        {
            foreach (var observer in _observers.ToList()) observer.OnNext(evt);
        }

        public void Complete()
        {
            foreach (var observer in _observers.ToList()) observer.OnCompleted();
        }

        public void Error(Exception exception)
        {
            foreach (var observer in _observers.ToList()) observer.OnError(exception);
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