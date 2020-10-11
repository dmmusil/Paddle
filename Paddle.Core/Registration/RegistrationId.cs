namespace Paddle.Core.Registration
{
    public class RegistrationId
    {
        private readonly string _id;

        public RegistrationId(string id)
        {
            _id = id;
        }

        public static implicit operator string(RegistrationId id) => id._id;
    }
}