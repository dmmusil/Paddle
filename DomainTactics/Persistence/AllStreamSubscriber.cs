using System;
using System.Threading;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Infrastructure;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

namespace DomainTactics.Persistence
{
    public class AllStreamSubscriber
    {
        // ReSharper disable once NotAccessedField.Local
        private static AllStreamSubscriber _instance;

        public static void Create(StreamStoreBase store, IEventBus bus,
            TypeMapper types, ICheckpointRepository repo) =>
            _instance = new AllStreamSubscriber(store, bus, types, repo);

        private readonly StreamStoreBase _store;
        private readonly IEventBus _bus;
        private readonly TypeMapper _types;
        private readonly ICheckpointRepository _repo;

        public AllStreamSubscriber(StreamStoreBase store, IEventBus bus, TypeMapper types, ICheckpointRepository repo, bool testing = false)
        {
            _store = store;
            _bus = bus;
            _types = types;
            _repo = repo;
            SubscribeToAll(testing: testing);
        }

        private void SubscribeToAll(
            long? start = null,
            HasCaughtUp hasCaughtUp = null,
            bool testing = false)
        {
            if (testing)
            {
                _store.SubscribeToAll(start, TestStreamMessageReceived,
                    SubscriptionDropped, hasCaughtUp);

            }
            else
            {
                _store.SubscribeToAll(start, StreamMessageReceived,
                    SubscriptionDropped, hasCaughtUp);
            }

        }


        private void SubscriptionDropped(
            IAllStreamSubscription subscription,
            SubscriptionDroppedReason reason, Exception exception) =>
            SubscribeToAll(subscription.LastPosition);

        private async Task StreamMessageReceived(
            IAllStreamSubscription subscription, StreamMessage streamMessage,
            CancellationToken cancellationToken)
        {
            var checkpoint = await _repo.GetCheckpoint();
            ++checkpoint;
            await _bus.Publish(await StreamMessageToEvent(streamMessage, cancellationToken));
            await _repo.UpdateCheckpoint(checkpoint);
        }


        private async Task TestStreamMessageReceived(
            IAllStreamSubscription subscription, StreamMessage streamMessage,
            CancellationToken cancellationToken) =>
            await _bus.Publish(await StreamMessageToEvent(streamMessage, cancellationToken));

        private async Task<Event> StreamMessageToEvent(
            StreamMessage streamMessage,
            CancellationToken cancellationToken) =>
            (Event)JsonConvert.DeserializeObject(
                await streamMessage.GetJsonData(cancellationToken),
                _types.TypeFor(streamMessage.Type));
    }
}