using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore.Infrastructure;

namespace Paddle.API
{
    public class Reactor
    {
        private readonly StreamStoreBase _store;
        private readonly IEventBus _bus;
        private readonly TypeMapper _types;
        private readonly ICheckpointRepository _repo;

        public Reactor(StreamStoreBase store, ICheckpointRepository repo,
            IEventBus bus, TypeMapper types)
        {
            _store = store;
            _repo = repo;
            _bus = bus;
            _types = types;
        }

        [FunctionName("Reactor")]
        public async Task Run(
            [QueueTrigger("new-event", Connection = "QueueConnection")]
            string version, ILogger log)
        {
            var checkpoint = await _repo.GetCheckpoint();
            var events = new List<Event>();
            var page =
                await _store.ReadAllForwards(checkpoint + 1, 100, false);
            for (; ; page = await page.ReadNext())
            {
                events.AddRange(
                    await Task.WhenAll(page.Messages.Select(async m =>
                        (Event)JsonConvert.DeserializeObject(
                            await m.GetJsonData(),
                            _types.TypeFor(m.Type)))));
                if (page.IsEnd) break;
            }


            foreach (var @event in events)
            {
                ++checkpoint;
                await _bus.Publish(@event);
            }

            await _repo.UpdateCheckpoint(checkpoint);
        }
    }
}
