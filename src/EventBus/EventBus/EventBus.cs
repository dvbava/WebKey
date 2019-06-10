using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace EventBus
{
    public sealed class EventBus : IEventBus, IEventBusPublisher
    {
        private readonly IList<IObserver<object>> _eventObservers;

        public EventBus()
        {
            this._eventObservers = new List<IObserver<object>>();
        }

        public IDisposable Connect<T>(Action<T> onEvent) where T : class
        {
            IDisposable disposable = Disposable.Empty;
            if (onEvent != null)
                disposable = ObservableExtensions.Subscribe(Observable.ObserveOn(this.CreateEventBusObserver<T>(), Scheduler.Default), be => onEvent(be as T));
            return disposable;
        }

        public IDisposable Connect<T>(Func<T, bool> eventPredicate, Action<T> onEvent) where T : class
        {
            IDisposable disposable = Disposable.Empty;
            if (eventPredicate != null && onEvent != null)
                disposable = ObservableExtensions.Subscribe(Observable.ObserveOn(Observable.Where(this.CreateEventBusObserver<T>(), be => eventPredicate(be as T)), Scheduler.Default), be => onEvent(be as T));
            return disposable;
        }

        public IDisposable ConnectAsyncSequence<T>(Func<T, Task> onEvent) where T : class
        {
            IDisposable disposable = Disposable.Empty;
            if (onEvent != null)
                disposable = ObservableExtensions.Subscribe(Observable.Merge(Observable.Select(Observable.ObserveOn(this.CreateEventBusObserver<T>(), Scheduler.Default), o => Observable.FromAsync(() => wrappedEvent(o))), 1));
            return disposable;

            async Task<Unit> wrappedEvent(object observedObject)
            {
                if (observedObject is T observedType)
                    await onEvent(observedType);
                return Unit.Default;
            }
        }

        public IDisposable ConnectAsyncSequence<T>(
          Func<T, bool> eventPredicate,
          Func<T, Task> onEvent)
          where T : class
        {
            IDisposable disposable = Disposable.Empty;
            if (eventPredicate != null && onEvent != null)
                disposable = ObservableExtensions.Subscribe(Observable.Merge(Observable.Select(Observable.ObserveOn(Observable.Where(this.CreateEventBusObserver<T>(), be => eventPredicate(be as T)), Scheduler.Default), o => Observable.FromAsync(() => wrappedEvent(o))), 1));
            return disposable;

            async Task<Unit> wrappedEvent(object observedObject)
            {
                if (observedObject is T observedType)
                    await onEvent(observedType);
                return Unit.Default;
            }
        }

        public void PublishEvent<T>(T toPublish)
        {
            if (toPublish == null) return;

            lock (this._eventObservers)
            {
                foreach (IObserver<object> eventObserver in this._eventObservers)
                    eventObserver.OnNext(toPublish);
            }
        }

        private IObservable<object> CreateEventBusObserver<T>()
        {
            return Observable.Where(Observable.Create<object>(obvs =>
            {
                lock (this._eventObservers)
                    this._eventObservers.Add(obvs);
                return Disposable.Create(() => this.Disconnect(obvs));
            }), be => be is T);
        }

        private void Disconnect(IObserver<object> eventObserver)
        {
            lock (this._eventObservers)
                this._eventObservers.Remove(eventObserver);
        }
    }
}
