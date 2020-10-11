using System;

namespace DomainTactics.Messaging
{
    public class Event : Message
    {

    }

    public class Message
    {
        public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");
    }
}