using DomainTactics;
using DomainTactics.Messaging;
using Paddle.Core.Registration;
using Paddle.Core.Shared;

namespace Paddle.Core.UserProfiles
{
    public class User : Aggregate
    {
        public string UserId { get; private set; }
        public string DisplayName { get; private set; }

        protected override void Apply(Event e)
        {
            switch (e)
            {
                case DisplayNameSet d:
                    DisplayName = d.Name;
                    break;
            }
        }

        public void When(RegistrationSucceeded @event)
        {
            UserId = @event.Email;
        }

        public void SetDisplayName(DisplayName name)
            => Then(new DisplayNameSet(name));
    }
}