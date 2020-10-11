using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Registration;

namespace Paddle.Core.UserProfiles
{
    public class UsersHandlers : IReactTo<RegistrationStarted>
    {
        private readonly IRepository _repo;

        public UsersHandlers(IRepository repo)
        {
            _repo = repo;
        }

        public async Task When(RegistrationStarted @event)
        {
            var users = await _repo.Load<Users>("user-list") ?? new Users();
            users.When(@event);
            await _repo.Save(users);
        }
    }
}