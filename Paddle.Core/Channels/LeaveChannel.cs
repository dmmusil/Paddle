using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Channels
{
    public class LeaveChannel : Command
    {
        public string ChannelId { get; set; }
        public string UserId { get; set; }
        public DateTime Time { get; set; }
    }
}