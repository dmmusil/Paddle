using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using Newtonsoft.Json;
using SqlStreamStore.Infrastructure;
using SqlStreamStore.Streams;

namespace DomainTactics.Persistence
{
    public class SqlStreamStoreRepository : IRepository
    {
        private readonly StreamStoreBase _store;
        private readonly TypeMapper _types;

        public SqlStreamStoreRepository(StreamStoreBase store, TypeMapper types)
        {
            _store = store;
            _types = types;
        }

        public async Task<T> Load<T>(string id) where T : Aggregate, new()
        {
            if (string.IsNullOrEmpty(id))throw new ArgumentException("Id required", nameof(id));

            var events = new List<Event>();
            var streamId = new StreamId(id);
            var page = await _store.ReadStreamForwards(streamId,
                StreamVersion.Start, 100);
            for (;; page = await page.ReadNext())
            {
                events.AddRange(
                    await Task.WhenAll(page.Messages.Select(async m =>
                        (Event) JsonConvert.DeserializeObject(
                            await m.GetJsonData(),
                            _types.TypeFor(m.Type)))));
                if (page.IsEnd) break;
            }

            var aggregate = new T();
            aggregate.Load(events);
            return aggregate;
        }

        public async Task<long> Save(Aggregate a)
        {
            if (!a.HasChanges) return StreamVersion.Start;

            var events = a.UncommittedEvents.ToList();
            await _store.AppendToStream(new StreamId(a.Id),
                a.Version == -1 ? ExpectedVersion.NoStream : a.Version,
                events.Select(e => new NewStreamMessage(
                    Guid.Parse(e.UniqueId),
                    _types.NameFor(e.GetType()),
                    JsonConvert.SerializeObject(e))).ToArray());
            
            return await _store.ReadHeadPosition();
        }
    }
}