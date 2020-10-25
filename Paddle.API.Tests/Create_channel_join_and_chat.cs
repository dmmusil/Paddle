using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using DomainTactics.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                s.AddSingleton<IDocumentStorage, InMemoryDocumentStorage>();
                s.AddLogging(b => b.ClearProviders());
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
        /// <summary>
        /// Submits a command and waits until it's processed by the read side.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="c"></param>
        /// <param name="commandName"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        public static async Task SubmitCommand(this HttpClient client,
            Command c, string commandName, ITestOutputHelper console)
        {
            var command = new { Type = commandName, Command = JsonConvert.SerializeObject(c) };
            var json = JsonConvert.SerializeObject(command);
            console.WriteLine($"Submitting {json}");
            
            // send the command
            var request = await client.PostAsync("command",
                new StringContent(json, Encoding.UTF8, "application/json"));
            console.WriteLine($"Result: {request.StatusCode}");
            if (!request.IsSuccessStatusCode)
            {
                console.WriteLine(await request.Content.ReadAsStringAsync());
            }

            // keep track of how long it takes the read side to catch up
            var timer = Stopwatch.StartNew();
            // figure out where the write side is
            var writeVersionValue = await request.Content.ReadAsStringAsync();
            
            // query the read side
            const string versionUrl = "version?id=channels/DK";
            var readVersion = await client.GetStringAsync(versionUrl);

            var attempts = 0;
            var writeVersion = long.Parse(writeVersionValue);
            
            // query up to 50 times to see if it catches up
            while (long.Parse(readVersion) < writeVersion)
            {
                readVersion = await client.GetStringAsync(versionUrl);
                attempts++;
                if (attempts > 50) throw new Exception("Read model failed to catch up.");
            }

            console.WriteLine($"Read side caught up in {timer.ElapsedMilliseconds} ms");
        }

    }
}
