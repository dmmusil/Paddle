using System;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Channels;
using Paddle.Core.Shared;

namespace Paddle.Core.Messages
{
    public class ChatMessageHandlers : IHandle<SubmitChatMessage>, IHandle<EditChatMessage>
    {
        private readonly IRepository _repo;

        public ChatMessageHandlers(IRepository repo) => _repo = repo;

        public async Task<long> Handle(SubmitChatMessage message)
        {
            var chat = new ChatMessage();
            chat.Submit(
                new ChatMessageId(message.MessageId),
                new Sender(message.Sender),
                new MessageContents(message.Contents),
                new Instant(message.SubmitTime),
                new ChannelId(message.Channel)
            );
            return await _repo.Save(chat);
        }

        public Task<long> Handle(EditChatMessage message) =>
            Execute(message.MessageId,
                msg => msg.Edit(new Sender(message.Sender),
                    new MessageContents(message.NewContents),
                    new Instant(message.EditTime)));

        private async Task<long> Execute(string id, Action<ChatMessage> a)
        {
            var msg = await _repo.Load<ChatMessage>(id);
            a(msg);
            return await _repo.Save(msg);
        }
    }
}