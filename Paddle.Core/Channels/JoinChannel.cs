using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Channels
{
    public class JoinChannel : Command
    {
        public string ChannelId { get; set; }
        public DateTime Time { get; set; }
        public string UserId { get; set; }
    }
}