using DomainTactics;
using DomainTactics.Messaging;
using Paddle.Core.Shared;

namespace Paddle.Core.Channels
{
    public class Channel : Aggregate
    {
        protected override void Apply(Event e)
        {
            switch (e)
            {
                case ChannelCreated c:
                    Id = c.ID;
                    break;
            }
        }

        public void Create(ChannelName name, ChannelId id, UserId createdBy,
            Instant time) =>
            Then(new ChannelCreated(name, id, createdBy, time));

        public void Join(ChannelId channel, UserId user, DisplayName name, Instant now) =>
            Then(new ChannelJoined(channel, user, name, now));

        public void Leave(ChannelId channel, UserId user, Instant now) =>
            Then(new ChannelLeft(channel, user, now));
    }
}