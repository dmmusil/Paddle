using System;

namespace Paddle.Core.Channels
{
    public class DisplayMessage
    {
        public DisplayMessage(string messageContents, DateTime messageTime,
            string userName, string messageId)
        {
            MessageContents = messageContents;
            MessageTime = messageTime;
            UserName = userName;
            MessageId = messageId;
        }

        public string MessageContents { get; }
        public DateTime MessageTime { get; }
        public string UserName { get; }
        public string MessageId { get; }

        public DisplayMessage Edit(string newContents, DateTime editTime) =>
            new DisplayMessage(newContents, editTime, UserName, MessageId);
    }
}