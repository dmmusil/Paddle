using System.Threading.Tasks;

namespace DomainTactics.Persistence
{
    public interface IRepository
    {
        Task<T> Load<T>(string id) where T : Aggregate, new();
        Task<long> Save(Aggregate a);
    }
}