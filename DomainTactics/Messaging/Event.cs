using System;
using Newtonsoft.Json;

namespace DomainTactics.Messaging
{
    public class Event : Message
    {
        [JsonIgnore]
        public long Position { get; set; }
    }

    public class Message
    {
        public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");
    }
}