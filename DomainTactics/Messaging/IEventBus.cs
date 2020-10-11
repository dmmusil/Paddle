using System;
using System.Threading.Tasks;

namespace DomainTactics.Messaging
{
    public interface IEventBus
    {
        void Register<T>(Func<T, Task> handler) where T : Event;
        Task Publish(Event e);
    }
}