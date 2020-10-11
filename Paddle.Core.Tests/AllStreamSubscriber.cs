using System;
using System.Threading;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Infrastructure;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

namespace Paddle.Core.Tests
{
    public class AllStreamSubscriber
    {
        private readonly StreamStoreBase _store;
        private readonly IEventBus _bus;
        private readonly TypeMapper _types;

        public AllStreamSubscriber(StreamStoreBase store, IEventBus bus, TypeMapper types)
        {
            _store = store;
            _bus = bus;
            _types = types;
            SubscribeToAll();
        }

        private void SubscribeToAll(
            long? start = null,
            HasCaughtUp hasCaughtUp = null) =>
            _store.SubscribeToAll(start, StreamMessageReceived,
                SubscriptionDropped, hasCaughtUp);

        private void SubscriptionDropped(
            IAllStreamSubscription subscription,
            SubscriptionDroppedReason reason, Exception exception) =>
            SubscribeToAll(subscription.LastPosition);

        private async Task StreamMessageReceived(
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