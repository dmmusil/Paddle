using System.Collections.Generic;
using System.Linq;
using DomainTactics.Persistence;
using Paddle.Core.Messages;

namespace Paddle.Core.Channels
{
    public class ChannelHistory : IHaveIdentifier
    {
        public Dictionary<string, string> Members { get; set; } =
            new Dictionary<string, string>();

        public Dictionary<string, DisplayMessage> Messages { get; set; } =
            new Dictionary<string, DisplayMessage>();

        public void When(ChannelCreated created)
        {
            Name = created.Name;
            Identifier = created.ID;
        }

        public string Name { get; private set; }

        public void When(ChannelJoined joined)
        {
            Members[joined.UserId] = joined.DisplayName;
            Messages[joined.UniqueId] = new DisplayMessage(
                $"{joined.DisplayName} joined.",
                joined.JoinTime, null, null);
        }

        public void When(ChannelLeft left)
        {
            var member = Members[left.UserId];
            if (Members.Remove(left.UserId))
            {
                Messages[left.UniqueId] = new DisplayMessage($"{member} left.",
                    left.DepartureTime, null, null);

            }
        }

        public void When(MessageSubmitted message)
        {
            Messages[message.Id] = new DisplayMessage(message.Contents,
                message.Time,
                Members[message.Sender],
                message.Id);
        }

        public DisplayMessage[] GetPage(int offset = 0, int size = 20)
        {
            return Messages
                .Select(m => m.Value)
                .OrderByDescending(m => m.MessageTime)
                .Skip(offset)
                .Take(size)
                .ToArray();
        }

        public string[] MemberNames => Members.Values.ToArray();

        public void When(MessageEdited created)
        {
            var displayMessage = Messages[created.MessageId];
            Messages[created.MessageId] =
                displayMessage.Edit(created.NewContents, created.Time);
        }

        public string Identifier { get; set; }
    }
}