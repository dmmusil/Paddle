using DomainTactics.Messaging;

namespace Paddle.Core.Registration
{
    public class RegistrationSucceeded : Event
    {
        public string Email { get; }
        public string RegistrationId { get; }

        public RegistrationSucceeded(string email, string registrationId)
        {
            Email = email;
            RegistrationId = registrationId;
        }
    }
}