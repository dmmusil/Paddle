using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Channels
{
    public class CreateChannel : Command
    {
        public string ChannelName { get; set; }
        public string ChannelId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateTime { get; set; }
    }
}