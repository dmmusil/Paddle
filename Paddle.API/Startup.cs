using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Storage.Blob;
using Paddle.Core.Channels;
using Paddle.Core.Messages;
using Paddle.Core.Registration;
using Paddle.Core.UserProfiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlStreamStore;
using SqlStreamStore.Infrastructure;
using BlobStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;

namespace Paddle.API
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var types = RegisterTypes();


            services.AddSingleton(c => DocumentStorage());
            services.AddSingleton<StreamStoreBase, InMemoryStreamStore>();
            services.AddSingleton(types);
            services.AddSingleton<IRepository, SqlStreamStoreRepository>();


            services.AddSingleton<IEventBus>(c=>
            {
                var bus = new EventBus();

                RegisterHandlers.RegisterEventHandlers(bus, c.GetService<IDocumentStorage>(),
                    c.GetService<IRepository>());

                return bus;
            });
            services.AddSingleton(c =>
            {
                var bus = new CommandBus();

                RegisterHandlers.RegisterCommandHandlers(bus, c.GetService<IRepository>());
                
                return bus;
            });


        }

        protected virtual IDocumentStorage DocumentStorage()
        {
            var cloudBlobClient = BlobStorageAccount.DevelopmentStorageAccount
                .CreateCloudBlobClient();
            IDocumentStorage readRepo = new BlobDocumentStorage(cloudBlobClient);
            return readRepo;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            RegisterHandlers.AllStreamSubscription(app.ApplicationServices.GetService<StreamStoreBase>(),
                app.ApplicationServices.GetService<IEventBus>(), app.ApplicationServices.GetService<TypeMapper>());
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


    }

    public class InMemoryDocumentStorage : IDocumentStorage
    {
        private readonly Dictionary<string, IHaveIdentifier> _documents = new Dictionary<string, IHaveIdentifier>();

        public Task<T> Load<T>(string identifier)
        {
            return Task.FromResult((T)_documents[identifier]);
        }

        public Task Save(IHaveIdentifier document)
        {
            _documents[document.Identifier] = document;
            return Task.CompletedTask;
        }
    }

    public static class RegisterHandlers
    {
        public static void AllStreamSubscription(StreamStoreBase store, IEventBus eventBus, TypeMapper types)
        {
            AllStreamSubscriber.Create(store, eventBus, types);
        }

        public static void RegisterCommandHandlers(CommandBus bus,
                IRepository writeRepo)
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

        public static void RegisterEventHandlers(IEventBus bus, IDocumentStorage readRepo, IRepository writeRepo)
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
    }
}
