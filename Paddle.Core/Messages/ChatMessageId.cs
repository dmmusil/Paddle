namespace Paddle.Core.Messages
{
    public class ChatMessageId
    {
        private readonly string _id;

        public ChatMessageId(string id)
        {
            _id = id;
        }

        public static implicit operator string(ChatMessageId id) => id._id;
    }
}