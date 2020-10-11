using System.Threading.Tasks;
using DomainTactics.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Paddle.API
{
    [Route("version"), ApiController]
    public class VersionController : ControllerBase
    {
        private readonly ICheckpointRepository _repo;

        public VersionController(ICheckpointRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public Task<long> Version() => _repo.GetCheckpoint();
    }
}