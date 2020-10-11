namespace DomainTactics.Messaging
{
    public interface IProject<in T> where T : Event
    {
        void When(T @event);
    }
}