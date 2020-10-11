using DomainTactics.Messaging;

namespace Paddle.Core.Messages
{
    public class MessageContents : Event
    {
        private readonly string _message;

        public MessageContents(string message)
        {
            _message = message;
        }

        public static implicit operator string(MessageContents m)
        {
            return m._message;
        }
    }
}