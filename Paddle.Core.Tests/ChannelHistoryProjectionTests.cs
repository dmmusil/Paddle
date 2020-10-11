using System;
using System.Linq;
using Paddle.Core.Channels;
using Xunit;

namespace Paddle.Core.Tests
{
    public class ChannelHistoryProjectionTests
    {
        [Fact]
        public void When_user_joins_channel_they_appear_as_a_member()
        {
            var channel = new ChannelHistory();
            channel.When(new ChannelJoined("1", "u1", "Dylan", DateTime.Now));

            Assert.Single(channel.MemberNames);
            Assert.Equal("Dylan", channel.MemberNames.Single());
        }

        [Fact]
        public void When_user_leaves_channel_they_no_longer_appear_as_a_member()
        {
            var channel = new ChannelHistory();
            channel.When(new ChannelJoined("1", "u1", "Dylan", DateTime.Now));
            channel.When(new ChannelLeft("1", "u1", DateTime.Now));

            Assert.Empty(channel.MemberNames);
        }

        [Fact]
        public void Joining_and_leaving_generate_a_message()
        {
            var channel = new ChannelHistory();
            channel.When(new ChannelJoined("1", "u1", "Dylan", DateTime.Now));
            channel.When(new ChannelLeft("1", "u1", DateTime.Now));

            Assert.True(channel.GetPage().Length == 2);
        }
    }
}
