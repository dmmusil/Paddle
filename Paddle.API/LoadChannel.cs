using System.Net.Http;
using System.Threading.Tasks;
using DomainTactics.Persistence;
using Paddle.Core.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Paddle.API
{
    public class LoadChannel
    {
        private readonly IDocumentStorage _repo;

        public LoadChannel(IDocumentStorage repo)
        {
            _repo = repo;
        }

        [FunctionName("Channel")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get",
                Route = "channel/{id}")]
            HttpRequestMessage req, string id)
        {
            return new JsonResult(
                await _repo.Load<ChannelHistory>($"channels/{id}"));
        }
    }
}