using System;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Messages;

namespace Paddle.Core.Channels
{
    public class ChannelHistoryHandlers :
        IReactTo<ChannelJoined>,
        IReactTo<ChannelLeft>,
        IReactTo<MessageSubmitted>,
        IReactTo<ChannelCreated>,
        IReactTo<MessageEdited>
    {
        private readonly IDocumentStorage _repo;

        public ChannelHistoryHandlers(IDocumentStorage repo)
        {
            _repo = repo;
        }

        public Task When(ChannelJoined @event) =>
            React(@event.ChannelId, h => h.When(@event));

        public Task When(ChannelLeft @event) =>
            React(@event.ChannelId, h => h.When(@event));

        public Task When(MessageSubmitted @event) =>
            React(@event.Channel, h => h.When(@event));

        public async Task When(ChannelCreated @event)
        {
            var history = new ChannelHistory();
            history.When(@event);
            await _repo.Save(history);
        }

        public Task When(MessageEdited @event) =>
            React(@event.ChannelId, h => h.When(@event));

        private async Task React(string channel, Action<ChannelHistory> action)
        {
            var history = await _repo.Load<ChannelHistory>(channel);
            action(history);
            await _repo.Save(history);
        }
    }
}