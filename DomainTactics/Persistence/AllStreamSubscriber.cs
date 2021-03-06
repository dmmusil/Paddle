﻿using System;
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
            TypeMapper types) =>
            _instance = new AllStreamSubscriber(store, bus, types);

        private readonly StreamStoreBase _store;
        private readonly IEventBus _bus;
        private readonly TypeMapper _types;

        public AllStreamSubscriber(StreamStoreBase store, IEventBus bus, TypeMapper types, bool testing = false)
        {
            _store = store;
            _bus = bus;
            _types = types;
            SubscribeToAll(testing: testing);
        }

        private void SubscribeToAll(
            long? start = null,
            HasCaughtUp hasCaughtUp = null,
            bool testing = false)
        {
            _store.SubscribeToAll(start, StreamMessageReceived,
                SubscriptionDropped, hasCaughtUp);
        }


        private void SubscriptionDropped(
            IAllStreamSubscription subscription,
            SubscriptionDroppedReason reason, Exception exception) =>
            SubscribeToAll(subscription.LastPosition);

        private async Task StreamMessageReceived(
            IAllStreamSubscription subscription, StreamMessage streamMessage,
            CancellationToken cancellationToken)
        {
            var @event = await StreamMessageToEvent(streamMessage, cancellationToken);
            @event.Position = streamMessage.Position;
            await _bus.Publish(@event);
        }

        private async Task<Event> StreamMessageToEvent(
            StreamMessage streamMessage,
            CancellationToken cancellationToken) =>
            (Event)JsonConvert.DeserializeObject(
                await streamMessage.GetJsonData(cancellationToken),
                _types.TypeFor(streamMessage.Type));
    }
}