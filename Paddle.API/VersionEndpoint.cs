using System.Threading.Tasks;
using DomainTactics.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Paddle.API
{
    [Route("version"), ApiController]
    public class VersionController : ControllerBase
    {
        private readonly IDocumentStorage _repo;

        public VersionController(IDocumentStorage repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<long> Version(string id)
        {
            var document = await _repo.Load<IHaveIdentifier>(id);
            return document.Position;
        }
    }
}