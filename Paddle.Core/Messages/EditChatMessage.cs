using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Messages
{
    public class EditChatMessage : Command
    {
        public string MessageId { get; set; }
        public string Sender { get; set; }
        public string NewContents { get; set; }
        public DateTime EditTime { get; set; }
    }
}