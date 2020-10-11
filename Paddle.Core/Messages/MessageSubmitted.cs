using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Messages
{
    public class MessageSubmitted : Event
    {
        public MessageSubmitted(string id, string sender, string contents,
            DateTime time,
            string channel)
        {
            Id = id;
            Sender = sender;
            Contents = contents;
            Time = time;
            Channel = channel;
        }

        public string Id { get; }
        public string Sender { get; }
        public string Contents { get; }
        public DateTime Time { get; }
        public string Channel { get; }
    }
}