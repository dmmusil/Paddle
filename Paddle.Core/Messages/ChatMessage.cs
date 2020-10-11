using DomainTactics;
using DomainTactics.Messaging;
using Paddle.Core.Channels;
using Paddle.Core.Shared;

namespace Paddle.Core.Messages
{
    public class ChatMessage : Aggregate
    {
        private string _channelId;
        protected override void Apply(Event e)
        {
            switch (e)
            {
                case MessageSubmitted m:
                    Id = m.Id;
                    _channelId = m.Channel;
                    break;
            }
        }

        public void Submit(
            ChatMessageId id,
            Sender sender,
            MessageContents contents,
            Instant time,
            ChannelId channel
        ) =>
            Then(new MessageSubmitted(id, sender, contents, time, channel));

        public void Edit(Sender sender, MessageContents newContents,
            Instant time) =>
            Then(new MessageEdited(sender, newContents, time, _channelId, Id));
    }
}