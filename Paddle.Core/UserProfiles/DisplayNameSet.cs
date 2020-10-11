using DomainTactics.Messaging;

namespace Paddle.Core.UserProfiles
{
    public class DisplayNameSet : Event
    {
        public string Name { get; }

        public DisplayNameSet(string name)
        {
            Name = name;
        }
    }
}