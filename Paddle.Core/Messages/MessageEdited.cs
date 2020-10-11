using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Messages
{
    public class MessageEdited : Event
    {
        public MessageEdited(
            string sender,
            string newContents,
            DateTime time, string channelId, string messageId)
        {
            Sender = sender;
            NewContents = newContents;
            Time = time;
            ChannelId = channelId;
            MessageId = messageId;
        }

        public string Sender { get; }
        public string NewContents { get; }
        public DateTime Time { get; }
        public string ChannelId { get; }
        public string MessageId { get; }
    }
}