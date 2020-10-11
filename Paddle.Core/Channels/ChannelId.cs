using System;

namespace Paddle.Core.Channels
{
    public class ChannelId
    {
        private readonly string _id;

        public ChannelId(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public static implicit operator string(ChannelId c)
        {
            return c._id;
        }
    }
}