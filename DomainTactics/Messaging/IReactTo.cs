using System.Threading.Tasks;

namespace DomainTactics.Messaging
{
    public interface IReactTo<in T> where T : Event
    {
        Task When(T @event);
    }
}