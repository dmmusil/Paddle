using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Channels
{
    public class ChannelJoined : Event
    {
        public ChannelJoined(string channelId, string userId, string displayName,
            DateTime joinTime)
        {

            ChannelId = channelId;
            UserId = userId;
            DisplayName = displayName;
            JoinTime = joinTime;
        }

        public string ChannelId { get; }
        public string UserId { get; }
        public string DisplayName { get; }
        public DateTime JoinTime { get; }
    }
}