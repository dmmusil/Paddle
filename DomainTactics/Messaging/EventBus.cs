using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainTactics.Messaging
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, HashSet<Func<Event, Task>>>
            _handlers = new Dictionary<Type, HashSet<Func<Event, Task>>>();
        public void Register<T>(Func<T, Task> handler) where T : Event
        {
            if (!_handlers.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new HashSet<Func<Event, Task>>();
                _handlers.Add(typeof(T), handlers);
            }

            handlers.Add(e=>handler((T)e));
        }

        public async Task Publish(Event e)
        {
            var handlers = _handlers[e.GetType()];
            foreach (var handler in handlers)
            {
                await handler(e);
            }
        }
    }
}