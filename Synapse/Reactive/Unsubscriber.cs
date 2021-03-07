using System;
using System.Collections.Generic;

namespace Synapse.Reactive
{
    public class Unsubscriber<T> : IDisposable
    {
        private readonly IObserver<T> _observer;
        private readonly List<IObserver<T>> _observers;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer);
        }
    }
}