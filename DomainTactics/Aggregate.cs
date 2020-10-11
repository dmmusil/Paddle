using System.Collections.Generic;
using System.Linq;
using DomainTactics.Messaging;

namespace DomainTactics
{
    public abstract class Aggregate
    {
        private readonly List<Event> _events = new List<Event>();
        public string Id { get; protected set; }
        public IEnumerable<Event> UncommittedEvents => _events.ToList();
        public void ClearEvents() => _events.Clear();
        public int Version { get; private set; } = -1;
        public bool HasChanges => _events.Any();

        protected void Then(Event e)
        {
            _events.Add(e);
            Apply(e);
        }

        public void Load(IEnumerable<Event> history)
        {
            foreach (var e in history)
            {
                Apply(e);
                Version++;
            }
        }

        protected virtual void Apply(Event e) { }
    }
}