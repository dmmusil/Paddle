using System.Threading.Tasks;

namespace Paddle.API
{
    public interface ICheckpointRepository
    {
        Task<long> GetCheckpoint();
        Task UpdateCheckpoint(long value);
    }
}