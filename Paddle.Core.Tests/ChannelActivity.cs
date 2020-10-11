using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Channels;
using Paddle.Core.Messages;
using Paddle.Core.Registration;
using Paddle.Core.UserProfiles;
using SqlStreamStore;
using Xunit;

namespace Paddle.Core.Tests
{
    public class ChannelActivity
    {
        private readonly InMemoryStreamStore _store;
        private readonly UserRegistrationHandlers _userService;
        private readonly ChannelCommandHandlers _channelService;
        private readonly ChatMessageHandlers _chatService;
        private readonly InMemoryReadModelRepository _readRepo;

        public ChannelActivity()
        {
            _store = new InMemoryStreamStore();
            var types = RegisterTypes();
            var repo = new SqlStreamStoreRepository(_store, types);
            var bus = new EventBus();
            _userService = new UserRegistrationHandlers(repo);
            var user = new UsersHandlers(repo);
            _readRepo = new InMemoryReadModelRepository();
            var channelHistoryService = new ChannelHistoryHandlers(_readRepo);
            _channelService = new ChannelCommandHandlers(repo);
            _chatService = new ChatMessageHandlers(repo);
            bus.Register<ChannelCreated>(channelHistoryService.When);
            bus.Register<ChannelJoined>(channelHistoryService.When);
            bus.Register<ChannelLeft>(channelHistoryService.When);
            bus.Register<MessageSubmitted>(channelHistoryService.When);
            bus.Register<MessageEdited>(channelHistoryService.When);
            bus.Register<RegistrationStarted>(user.When);
            bus.Register<RegistrationSucceeded>(_userService.When);

            var _ = new AllStreamSubscriber(_store, bus, types);


        }

        private TypeMapper RegisterTypes()
        {
            var typeMapper = new TypeMapper();
            typeMapper.Register(typeof(ChannelCreated), nameof(ChannelCreated));
            typeMapper.Register(typeof(ChannelJoined), nameof(ChannelJoined));
            typeMapper.Register(typeof(ChannelLeft), nameof(ChannelLeft));
            typeMapper.Register(typeof(MessageSubmitted), nameof(MessageSubmitted));
            typeMapper.Register(typeof(MessageEdited), nameof(MessageEdited));
            typeMapper.Register(typeof(RegistrationSucceeded), nameof(RegistrationSucceeded));
            typeMapper.Register(typeof(RegistrationStarted), nameof(RegistrationStarted));
            return typeMapper;
        }

        [Fact]
        public async Task Messages()
        {
            await CreateUsers();
            await CreateChannel();
            await JoinChannel();
            await SendMessages();
            await AssertMessages();
        }

        private async Task CreateUsers()
        {
            await _userService.Handle(new Register(DateTime.UtcNow,
                "dylan@email.com", "1"));
            await _userService.Handle(new Register(DateTime.UtcNow,
                "kate@email.com", "2"));
            await _store.WaitForVersion(4);
        }

        async Task CreateChannel()
        {
            var v = await _channelService.Handle(new CreateChannel
            {
                ChannelId = "paddle",
                ChannelName = "DKChat",
                CreatedBy = "kate@email.com",
                CreateTime = DateTime.UtcNow
            });
            await _store.WaitForVersion(v);
        }

        async Task JoinChannel()
        {
            await _channelService.Handle(new JoinChannel
            {
                ChannelId = "paddle",
                Time = DateTime.UtcNow,
                UserId = "kate@email.com"
            });
            var v = await _channelService.Handle(new JoinChannel
            {
                ChannelId = "paddle",
                Time = DateTime.UtcNow,
                UserId = "dylan@email.com"
            });
            await _store.WaitForVersion(v);
        }

        async Task SendMessages()
        {
            await _chatService.Handle(new SubmitChatMessage
            {
                Channel = "paddle",
                Contents = "Hi Dylan!",
                MessageId = "message1",
                SubmitTime = DateTime.UtcNow,
                Sender = "kate@email.com"
            });
            var v = await _chatService.Handle(new SubmitChatMessage
            {
                Channel = "paddle",
                Contents = "Hi Kate!",
                MessageId = "message2",
                SubmitTime = DateTime.UtcNow,
                Sender = "dylan@email.com"
            });
            await _store.WaitForVersion(v);
        }

        async Task AssertMessages()
        {
            var history = await _readRepo.Load<ChannelHistory>("paddle");
            var displayMessages = history.GetPage().OrderBy(m => m.MessageTime)
                .ToList();
            Assert.Equal(4, displayMessages.Count);
            Assert.Equal("kate@email.com joined.", displayMessages[0].MessageContents);
            Assert.Equal("dylan@email.com joined.", displayMessages[1].MessageContents);
            Assert.Equal("Hi Dylan!", displayMessages[2].MessageContents);
            Assert.Equal("Hi Kate!", displayMessages[3].MessageContents);
        }

    }

    public class InMemoryReadModelRepository : IDocumentStorage
    {
        Dictionary<string, IHaveIdentifier> database = new Dictionary<string, IHaveIdentifier>();
        public Task<T> Load<T>(string identifier)
        {
            return Task.FromResult((T)database[identifier]);
        }

        public Task Save(IHaveIdentifier document)
        {
            database[document.Identifier] = document;
            return Task.CompletedTask;
        }
    }
}
