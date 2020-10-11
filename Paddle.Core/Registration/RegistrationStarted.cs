using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Registration
{
    public class RegistrationStarted : Event
    {
        public RegistrationStarted(string email, DateTime registrationTime, string registrationId)
        {
            Email = email;
            RegistrationTime = registrationTime;
            RegistrationId = registrationId;
        }

        public string Email { get; }
        public DateTime RegistrationTime { get; }
        public string RegistrationId { get; }
    }
}