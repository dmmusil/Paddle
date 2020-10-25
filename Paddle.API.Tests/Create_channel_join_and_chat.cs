using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paddle.Core.Channels;
using Paddle.Core.Messages;
using Xunit;
using Xunit.Abstractions;

namespace Paddle.API.Tests
{
    // ReSharper disable once InconsistentNaming
    public class Create_channel_join_and_chat : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public Create_channel_join_and_chat(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper)
        {
            _factory = factory.WithWebHostBuilder(c => c.ConfigureTestServices(s =>
            {
                s.Remove(s.FirstOrDefault(ss =>
                    ss.ServiceType == typeof(IDocumentStorage) || ss.ServiceType == typeof(BlobDocumentStorage)));
                s.AddSingleton<IDocumentStorage, InMemoryDocumentStorage>();
            }));
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test1()
        {
            var client = _factory.CreateClient();

            const string channelId = "channels/DK";
            await client.SubmitCommand(
                new CreateChannel
                {
                    ChannelId = channelId,
                    ChannelName = "Dylan and Kate",
                    CreatedBy = "dylan@email.com",
                    CreateTime = DateTime.UtcNow
                }, nameof(CreateChannel), _testOutputHelper);

            await client.SubmitCommand(
                new JoinChannel
                {
                    ChannelId = channelId,
                    UserId = "dylan@email.com",
                    Time = DateTime.UtcNow
                }, nameof(JoinChannel), _testOutputHelper);

            await client.SubmitCommand(
                new JoinChannel
                {
                    ChannelId = channelId,
                    UserId = "kate@email.com",
                    Time = DateTime.UtcNow
                }, nameof(JoinChannel), _testOutputHelper);

            await client.SubmitCommand(
                new SubmitChatMessage
                {
                    Channel = channelId,
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
            var versionUrl = "version?id=channels/DK";
            var readVersion = await client.GetStringAsync(versionUrl);
            var attempts = 0;
            while (long.Parse(readVersion) <
                   long.Parse(writeVersion))
            {
                readVersion = await client.GetStringAsync(versionUrl);
                attempts++;
                if (attempts > 50) throw new Exception("Read model failed to catch up.");
            }

            console.WriteLine($"Read side caught up in {timer.ElapsedMilliseconds} ms");

        }

    }
}
