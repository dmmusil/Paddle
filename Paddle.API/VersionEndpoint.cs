using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Paddle.API
{
    public class VersionEndpoint
    {
        private readonly ICheckpointRepository _repo;

        public VersionEndpoint(ICheckpointRepository repo)
        {
            _repo = repo;
        }

        [FunctionName("Version")]
        public Task<long> GetCheckpointValue(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "version")]
            HttpRequestMessage req) => _repo.GetCheckpoint();
    }
}