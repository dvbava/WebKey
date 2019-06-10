using System;
using System.Threading.Tasks;

namespace EventBus
{
    public interface IEventData<T>
    {
        T Payload { get; set; }
    }

    public interface IEventBus
    {
        IDisposable Connect<T>(Action<T> onEvent) where T : class;

        IDisposable Connect<T>(Func<T, bool> eventPredicate, Action<T> onEvent) where T : class;

        IDisposable ConnectAsyncSequence<T>(Func<T, Task> onEvent) where T : class;

        IDisposable ConnectAsyncSequence<T>(Func<T, bool> eventPredicate, Func<T, Task> onEvent) where T : class;
    }
}
