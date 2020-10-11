using System;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Shared;
using Paddle.Core.UserProfiles;

namespace Paddle.Core.Channels
{
    public class ChannelCommandHandlers :
        IHandle<CreateChannel>,
        IHandle<JoinChannel>,
        IHandle<LeaveChannel>
    {
        private readonly IRepository _repo;

        public ChannelCommandHandlers(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<long> Handle(CreateChannel message)
        {
            var channel = new Channel();
            channel.Create(new ChannelName(message.ChannelName),
                new ChannelId(message.ChannelId), new UserId(message.CreatedBy),
                new Instant(message.CreateTime));
            return await _repo.Save(channel);
        }

        public async Task<long> Handle(JoinChannel message)
        {
            var user = await _repo.Load<User>(message.UserId);
            return await Execute(message.ChannelId,
                c => c.Join(new ChannelId(message.ChannelId),
                    new UserId(message.UserId),
                    new DisplayName(user.DisplayName ?? message.UserId),
                    new Instant(message.Time)));
        }

        public Task<long> Handle(LeaveChannel message)
        {
            return Execute(
                message.ChannelId,
                c => c.Leave(
                    new ChannelId(message.ChannelId),
                    new UserId(message.UserId),
                    new Instant(message.Time)));
        }

        private async Task<long> Execute(string channelId,
            Action<Channel> action)
        {
            var channel = await _repo.Load<Channel>(channelId);
            action(channel);
            return await _repo.Save(channel);
        }
    }
}