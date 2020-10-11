using System.Threading.Tasks;
using DomainTactics.Persistence;
using Paddle.Core.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Paddle.API
{
    [ApiController, Route("channel")]
    public class ChannelController : ControllerBase
    {
        private readonly IDocumentStorage _repo;

        public ChannelController(IDocumentStorage repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChannel(string id)
        {
            return new JsonResult(
                await _repo.Load<ChannelHistory>($"channels/{id}"));
        }
    }
}