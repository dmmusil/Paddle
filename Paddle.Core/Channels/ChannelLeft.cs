using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Channels
{
    public class ChannelLeft : Event
    {
        public ChannelLeft(string channelId, string userId, DateTime departureTime)
        {
            ChannelId = channelId;
            UserId = userId;
            DepartureTime = departureTime;
        }

        public string ChannelId { get; }
        public string UserId { get; }
        public DateTime DepartureTime { get; }
    }
}