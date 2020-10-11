using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Channels
{
    public class ChannelCreated : Event
    {
        public string Name { get; }
        public string ID { get; }
        public string CreatedBy { get; }
        public DateTime CreatedTime { get; }

        public ChannelCreated(string name, string id, string createdBy, DateTime createdTime)
        {
            Name = name;
            ID = id;
            CreatedBy = createdBy;
            CreatedTime = createdTime;
        }
    }
}