using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DomainTactics.Messaging;
using Paddle.Core.Channels;
using Paddle.Core.Messages;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paddle.Console
{
    class Program
    {
        static async Task Main()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001")
            };

            var blob = CloudStorageAccount.DevelopmentStorageAccount
                .CreateCloudBlobClient();

            await blob.GetContainerReference("channels").DeleteIfExistsAsync();

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
                }, nameof(CreateChannel));

            await client.SubmitCommand(
                new JoinChannel
                {
                    ChannelId = "channels/DK",
                    UserId = "dylan@email.com",
                    Time = DateTime.UtcNow
                }, nameof(JoinChannel));

            await client.SubmitCommand(
                new JoinChannel
                {
                    ChannelId = "channels/DK",
                    UserId = "kate@email.com",
                    Time = DateTime.UtcNow
                }, nameof(JoinChannel));

            await client.SubmitCommand(
                new SubmitChatMessage
                {
                    Channel = "channels/DK",
                    Contents = "Hi Kate!",
                    SubmitTime = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString("N"),
                    Sender = "dylan@email.com"
                }, nameof(SubmitChatMessage));

            var json = await client.GetStringAsync("channel/DK");
            json = JObject.Parse(json).ToString(Formatting.Indented);
            System.Console.WriteLine(json);
            System.Console.ReadLine();
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task SubmitCommand(this HttpClient client,
            Command c, string commandName)
        {
            var command = new { Type = commandName, Command = JsonConvert.SerializeObject(c) };
            var json = JsonConvert.SerializeObject(command);
            System.Console.WriteLine($"Submitting {json}");
            var request = await client.PostAsync("command",
                new StringContent(json, Encoding.UTF8, "application/json"));
            System.Console.WriteLine($"Result: {request.StatusCode}");
            if (!request.IsSuccessStatusCode)
            {
                System.Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            var timer = Stopwatch.StartNew();
            var writeVersion = await request.Content.ReadAsStringAsync();
            var readVersion = await client.GetStringAsync("version");
            while (long.Parse(readVersion) <
                   long.Parse(writeVersion))
            {
                readVersion = await client.GetStringAsync("version");
            }

            System.Console.WriteLine($"Read side caught up in {timer.ElapsedMilliseconds} ms");

        }
    }
}
