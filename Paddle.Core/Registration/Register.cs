using System;
using DomainTactics.Messaging;

namespace Paddle.Core.Registration
{
    public class Register : Command
    {
        public Register(DateTime time, string email, string registrationId)
        {
            Time = time;
            Email = email;
            RegistrationId = registrationId;
        }

        public string Email { get; }
        public DateTime Time { get; }
        public string RegistrationId { get; }
    }
}