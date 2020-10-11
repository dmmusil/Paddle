namespace Paddle.Core.Shared
{
    public class UserId
    {
        private readonly string _userId;

        public UserId(string userId)
        {
            _userId = userId;
        }

        public static implicit operator string(UserId id) => id._userId;
    }
}