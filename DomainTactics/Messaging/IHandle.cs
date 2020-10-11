using System.Threading.Tasks;

namespace DomainTactics.Messaging
{
    public interface IHandle<in T>
    {
        Task<long> Handle(T message);
    }
}