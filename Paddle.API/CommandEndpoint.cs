using System.Threading.Tasks;
using DomainTactics.Messaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Paddle.API
{
    [Route("command"), ApiController]
    public class CommandController : ControllerBase
    {
        private readonly CommandBus _bus;
        private readonly TypeMapper _types;

        public CommandController(CommandBus bus, TypeMapper types)
        {
            _bus = bus;
            _types = types;
        }

        [HttpPost]
        public async Task<long> Command([FromBody] CommandModel commandModel)
        {
            var type = _types.TypeFor(commandModel.Type);
            //log.LogInformation($"Processing command of type {typeName}.");
            var writeVersion = await _bus.Send(
                (Command) JsonConvert.DeserializeObject(commandModel.Command,
                    type));
            //log.LogInformation($"{typeName} processed successfully.");
            return writeVersion;
        }
    }

    public class CommandModel
    {
        public string Type { get; set; }
        public string Command { get; set; }
    }
}
