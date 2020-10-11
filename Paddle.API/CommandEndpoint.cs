using System.Threading.Tasks;
using DomainTactics.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paddle.API
{
    public class CommandEndpoint
    {
        private readonly CommandBus _bus;
        private readonly TypeMapper _types;

        public CommandEndpoint(CommandBus bus, TypeMapper types)
        {
            _bus = bus;
            _types = types;
        }

        [FunctionName("Command")]
        public async Task<long> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post",
                Route = "command")]
            HttpRequest req,
            [Queue("new-event")] IAsyncCollector<string> version,
            ILogger log)
        {
            var json = await req.ReadAsStringAsync();
            var jo = JObject.Parse(json);
            var typeName = jo["Type"].ToString();
            var type = _types.TypeFor(typeName);
            var command =
                (Command)JsonConvert.DeserializeObject(
                    jo["Command"].ToString(), type);
            log.LogInformation($"Processing command of type {typeName}.");
            var writeVersion = await _bus.Send(command);
            await version.AddAsync(writeVersion.ToString());
            log.LogInformation($"{typeName} processed successfully.");
            return writeVersion;
        }
    }
}
