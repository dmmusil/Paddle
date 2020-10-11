using DomainTactics.Messaging;

namespace Paddle.Core.Registration
{
    public class RegistrationFailed : Event
    {
        public string Email { get; }
        public string RegistrationId { get; }

        public RegistrationFailed(string email, string registrationId)
        {
            Email = email;
            RegistrationId = registrationId;
        }
    }
}