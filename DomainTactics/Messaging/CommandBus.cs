using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainTactics.Messaging
{
    public class CommandBus
    {
        private readonly Dictionary<Type, Func<Command, Task<long>>> _handlers =
            new Dictionary<Type, Func<Command, Task<long>>>();
        public void Register<T>(Func<T, Task<long>> handler) where T : Command
        {
            if (_handlers.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T).Name} already registered.");
            }

            _handlers.Add(typeof(T), c => handler((T) c));
        }

        public Task<long> Send(Command c) => _handlers[c.GetType()](c);
    }

    public class Command : Message
    {
    }
}