using System;

namespace Paddle.Core.Channels
{
    public class ChannelName
    {
        private readonly string _name;

        public ChannelName(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static implicit operator string(ChannelName c) => c._name;
    }
}