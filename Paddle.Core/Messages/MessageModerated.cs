using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Messages
{
    public class MessageModerated : Event
    {
        public MessageModerated(
            string sender,
            string newContents,
            DateTime time)
        {
            Sender = sender;
            NewContents = newContents;
            Time = time;
        }

        public string Sender { get; }
        public string NewContents { get; }
        public DateTime Time { get; }
    }
}