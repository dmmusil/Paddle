using DomainTactics;
using DomainTactics.Messaging;
using Paddle.Core.Shared;

namespace Paddle.Core.Registration
{
    public class UserRegistration : Aggregate
    {
        public RegistrationStatus Status { get; private set; }
        protected override void Apply(Event e)
        {
            switch (e)
            {
                case RegistrationSucceeded _:
                    Status = RegistrationStatus.Success;
                    break;
                case RegistrationFailed _:
                    Status = RegistrationStatus.Fail;
                    break;
                case RegistrationStarted s:
                    Status = RegistrationStatus.Pending;
                    Id = s.RegistrationId;
                    break;
            }
        }

        public void Start(Email email, Instant now, RegistrationId id) =>
            Then(new RegistrationStarted(email, now, id));
        public void When(RegistrationSucceeded @event)
        {
            if (Status == RegistrationStatus.Pending)
            {
                Then(@event);
            }
        }

        public void When(RegistrationFailed @event)
        {
            if (Status == RegistrationStatus.Pending)
            {
                Then(@event);
            }
        }
    }
}