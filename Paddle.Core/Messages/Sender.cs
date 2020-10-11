namespace Paddle.Core.Messages
{
    public class Sender
    {
        private readonly string _id;

        public Sender(string id)
        {
            _id = id;
        }

        public static implicit operator string(Sender s)
        {
            return s._id;
        }
    }
}