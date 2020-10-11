using System.Threading.Tasks;

namespace DomainTactics.Persistence
{
    public interface ICheckpointRepository
    {
        Task<long> GetCheckpoint();
        Task UpdateCheckpoint(long value);
    }
}