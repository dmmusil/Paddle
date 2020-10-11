using System;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Paddle.Core.Registration;
using Paddle.Core.UserProfiles;
using SqlStreamStore;
using SqlStreamStore.Infrastructure;
using Xunit;

namespace Paddle.Core.Tests
{
    public class AggregateReactions
    {
        private readonly IEventBus _bus = new EventBus();
        private readonly UserRegistrationHandlers _registration;
        private readonly SqlStreamStoreRepository _repo;
        private readonly InMemoryStreamStore _store;

        public AggregateReactions()
        {
            TypeMapper RegisterTypes()
            {
                var m = new TypeMapper();
                m.Register(typeof(RegistrationSucceeded),
                    nameof(RegistrationSucceeded));
                m.Register(typeof(RegistrationFailed),
                    nameof(RegistrationFailed));
                m.Register(typeof(RegistrationStarted),
                    nameof(RegistrationStarted));
                return m;
            }

            _store = new InMemoryStreamStore();
            var types = RegisterTypes();
            _repo = new SqlStreamStoreRepository(_store, types);
            var users = new UsersHandlers(_repo);
            _bus.Register<RegistrationStarted>(users.When);

            _registration = new UserRegistrationHandlers(_repo);

            _bus.Register<RegistrationSucceeded>(_registration.When);
            _bus.Register<RegistrationFailed>(_registration.When);

            var _ = new AllStreamSubscriber(_store, _bus, types, null, true);
        }

        [Fact]
        public async Task UserRegistrationSucceeds()
        {
            await _registration.Handle(new Register(DateTime.UtcNow, "email@test.com",
                "1"));
            await _store.WaitForVersion(2);
            var registration =
                await _repo.Load<UserRegistration>("1");

            Assert.Equal(RegistrationStatus.Success,
                registration.Status);
        }

        [Fact]
        public async Task DuplicateUserRegistrationFails()
        {
            await _registration.Handle(new Register(DateTime.UtcNow, "email@test.com",
                "1"));
            await _store.WaitForVersion(2);
            await _registration.Handle(new Register(DateTime.UtcNow, "email@test.com",
                   "2"));
            await _store.WaitForVersion(4);
            var registration =
                await _repo.Load<UserRegistration>("2");
            Assert.Equal(RegistrationStatus.Fail,
                registration.Status);
        }
    }

    public static class TestExtensions
    {
        public static async Task WaitForVersion(this StreamStoreBase store,
            long expected)
        {
            long actual;
            var waitTotal = 0;
            do
            {
                actual = await store.ReadHeadPosition();
                await Task.Delay(25);
                waitTotal += 25;
                if (waitTotal > 1000) throw new TimeoutException($"Stream didn't reach expected position in 1 second. Expected {expected} but was {actual} after 1 second.");
            } while (expected > actual);

        }
    }
}