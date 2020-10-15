using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paddle.Core.Channels;
using Paddle.Core.Messages;
using Xunit;
using Xunit.Abstractions;

namespace Paddle.API.Tests
{
    public class InMemoryWebAppFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(null).UseStartup<TStartup>();
        }
    }
    // ReSharper disable once InconsistentNaming
    public class Create_channel_join_and_chat : IClassFixture<InMemoryWebAppFactory<StartupInMemory>>
    {
        private readonly InMemoryWebAppFactory<StartupInMemory> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public Create_channel_join_and_chat(InMemoryWebAppFactory<StartupInMemory> factory, ITestOutputHelper testOutputHelper)
        {
            _factory = factory;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test1()
        {
            var client = _factory.CreateClient();

            //await client.SubmitCommand(
            //    new Register(DateTime.UtcNow, "kate@email.com", "kate"),
            //    nameof(Register));

            //await client.SubmitCommand(
            //    new Register(DateTime.UtcNow, "dylan@email.com", "dylan"),
            //    nameof(Register));

            await client.SubmitCommand(
                new CreateChannel
                {
                    ChannelId = "channels/DK",
                    ChannelName = "Dylan and Kate",
                    CreatedBy = "dylan@email.com",
                    CreateTime = DateTime.UtcNow
                }, nameof(CreateChannel), _testOutputHelper);

            await client.SubmitCommand(
                new JoinChannel
                {
                    ChannelId = "channels/DK",
                    UserId = "dylan@email.com",
                    Time = DateTime.UtcNow
                }, nameof(JoinChannel), _testOutputHelper);

            await client.SubmitCommand(
                new JoinChannel
                {
                    ChannelId = "channels/DK",
                    UserId = "kate@email.com",
                    Time = DateTime.UtcNow
                }, nameof(JoinChannel), _testOutputHelper);

            await client.SubmitCommand(
                new SubmitChatMessage
                {
                    Channel = "channels/DK",
                    Contents = "Hi Kate!",
                    SubmitTime = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString("N"),
                    Sender = "dylan@email.com"
                }, nameof(SubmitChatMessage), _testOutputHelper);

            var json = await client.GetStringAsync("channel/DK");
            json = JObject.Parse(json).ToString(Formatting.Indented);
            _testOutputHelper.WriteLine(json);
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task SubmitCommand(this HttpClient client,
            Command c, string commandName, ITestOutputHelper console)
        {
            var command = new { Type = commandName, Command = JsonConvert.SerializeObject(c) };
            var json = JsonConvert.SerializeObject(command);
            console.WriteLine($"Submitting {json}");
            var request = await client.PostAsync("command",
                new StringContent(json, Encoding.UTF8, "application/json"));
            console.WriteLine($"Result: {request.StatusCode}");
            if (!request.IsSuccessStatusCode)
            {
                console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            var timer = Stopwatch.StartNew();
            var writeVersion = await request.Content.ReadAsStringAsync();
            var readVersion = await client.GetStringAsync("version");
            while (long.Parse(readVersion) <
                   long.Parse(writeVersion))
            {
                readVersion = await client.GetStringAsync("version");
            }

            console.WriteLine($"Read side caught up in {timer.ElapsedMilliseconds} ms");

        }

    }
}
