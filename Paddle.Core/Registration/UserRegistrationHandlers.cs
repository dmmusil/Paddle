using System;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Shared;

namespace Paddle.Core.Registration
{
    public class UserRegistrationHandlers :
        IReactTo<RegistrationSucceeded>,
        IReactTo<RegistrationFailed>,
        IHandle<Register>
    {
        private readonly IRepository _repo;

        public UserRegistrationHandlers(IRepository repo) => _repo = repo;

        public Task When(RegistrationSucceeded @event) =>
            React(@event.RegistrationId, r => r.When(@event));

        public Task When(RegistrationFailed @event) =>
            React(@event.RegistrationId, r => r.When(@event));

        private async Task React(string id, Action<UserRegistration> action)
        {
            var reg = await _repo.Load<UserRegistration>(id);
            action(reg);
            await _repo.Save(reg);
        }

        public async Task<long> Handle(Register message)
        {
            var reg = new UserRegistration();
            reg.Start(new Email(message.Email), new Instant(message.Time), new RegistrationId(message.RegistrationId));
            return await _repo.Save(reg);
        }
    }
}