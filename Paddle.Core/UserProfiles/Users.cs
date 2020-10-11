using System.Collections.Generic;
using DomainTactics;
using DomainTactics.Messaging;
using Paddle.Core.Registration;

namespace Paddle.Core.UserProfiles
{
    public class Users : Aggregate
    {
        public Users()
        {
            Id = "user-list";
        }

        protected override void Apply(Event e)
        {
            if (e is RegistrationSucceeded r) _userNames.Add(r.Email);
        }

        private readonly HashSet<string> _userNames = new HashSet<string>();
        public void When(RegistrationStarted @event)
        {
            if (!_userNames.Contains(@event.Email))
            {
                Then(new RegistrationSucceeded(@event.Email, @event.RegistrationId));
            }
            else
            {
                Then(new RegistrationFailed(@event.Email, @event.RegistrationId));
            }
        }
    }
}