using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Messages
{
    public class SubmitChatMessage : Command
    {
        public string Sender { get; set; }
        public string Contents { get; set; }
        public DateTime SubmitTime { get; set; }
        public string Channel { get; set; }
        public string MessageId { get; set; }
    }
}