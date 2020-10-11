using DomainTactics.Messaging;
using DomainTactics.Persistence;
using DotNetify;
using Paddle.Core.Channels;
using Paddle.Core.Messages;
using Paddle.Core.Registration;
using Paddle.Core.UserProfiles;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using SqlStreamStore;
using SqlStreamStore.Infrastructure;

[assembly: FunctionsStartup(typeof(Paddle.API.Startup))]
namespace Paddle.API
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var types = RegisterTypes();

            var store = new InMemoryStreamStore();
            var writeRepo = new SqlStreamStoreRepository(store, types);
            var cloudBlobClient = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();
            var readRepo = new BlobDocumentStorage(cloudBlobClient);
            var commandBus = new CommandBus();
            var eventBus = new EventBus();
            RegisterCommandHandlers(commandBus, writeRepo);
            RegisterEventHandlers(eventBus, readRepo, writeRepo);

            var cloudTableClient = CloudStorageAccount
                        .DevelopmentStorageAccount.CreateCloudTableClient();
            var checkpointRepository = new TableStorageCheckpointRepository(cloudTableClient);
            checkpointRepository.ClearCheckpoint();

            builder.Services.AddSingleton<IDocumentStorage>(readRepo);
            builder.Services.AddSingleton<StreamStoreBase>(store);
            builder.Services.AddSingleton(commandBus);
            builder.Services.AddSingleton(cloudTableClient);
            builder.Services.AddSingleton(cloudBlobClient);
            builder.Services
                .AddScoped<ICheckpointRepository,
                    TableStorageCheckpointRepository>();
            builder.Services.AddSingleton<IEventBus>(eventBus);
            builder.Services.AddSingleton(types);

            builder.Services.AddDotNetify();
        }

        private void RegisterEventHandlers(EventBus bus, BlobDocumentStorage readRepo, SqlStreamStoreRepository writeRepo)
        {
            var channelHistoryService = new ChannelHistoryHandlers(readRepo);
            var user = new UsersHandlers(writeRepo);
            var userService = new UserRegistrationHandlers(writeRepo);
            bus.Register<ChannelCreated>(channelHistoryService.When);
            bus.Register<ChannelJoined>(channelHistoryService.When);
            bus.Register<ChannelLeft>(channelHistoryService.When);
            bus.Register<MessageSubmitted>(channelHistoryService.When);
            bus.Register<MessageEdited>(channelHistoryService.When);
            bus.Register<RegistrationStarted>(user.When);
            bus.Register<RegistrationSucceeded>(userService.When);
        }

        private TypeMapper RegisterTypes()
        {
            var types = new TypeMapper();
            // commands
            // need to be registered for deserializing from the generic command endpoint
            types.Register(typeof(Register), "Register");
            types.Register(typeof(EditChatMessage), "EditChatMessage");
            types.Register(typeof(SubmitChatMessage), "SubmitChatMessage");
            types.Register(typeof(CreateChannel), "CreateChannel");
            types.Register(typeof(JoinChannel), "JoinChannel");
            types.Register(typeof(LeaveChannel), "LeaveChannel");

            // events
            // need to be registered so the fully qualified name isn't used in the event store
            types.Register(typeof(RegistrationStarted), "RegistrationStarted");
            types.Register(typeof(RegistrationSucceeded), "RegistrationSucceeded");
            types.Register(typeof(RegistrationFailed), "RegistrationFailed");

            types.Register(typeof(ChannelCreated), "ChannelCreated");
            types.Register(typeof(ChannelJoined), "ChannelJoined");
            types.Register(typeof(ChannelLeft), "ChannelLeft");

            types.Register(typeof(MessageSubmitted), "MessageSubmitted");
            types.Register(typeof(MessageEdited), "MessageEdited");

            return types;
        }

        private void RegisterCommandHandlers(CommandBus bus,
            SqlStreamStoreRepository writeRepo)
        {
            var registrationHandlers = new UserRegistrationHandlers(writeRepo);
            bus.Register<Register>(registrationHandlers.Handle);

            var chatMessageHandlers = new ChatMessageHandlers(writeRepo);
            bus.Register<EditChatMessage>(chatMessageHandlers.Handle);
            bus.Register<SubmitChatMessage>(chatMessageHandlers.Handle);

            var channelHandlers = new ChannelCommandHandlers(writeRepo);
            bus.Register<CreateChannel>(channelHandlers.Handle);
            bus.Register<JoinChannel>(channelHandlers.Handle);
            bus.Register<LeaveChannel>(channelHandlers.Handle);
        }
    }
}
