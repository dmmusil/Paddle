using Microsoft.Azure.Cosmos.Table;

namespace Paddle.API
{
    public class Checkpoint : TableEntity
    {
        public Checkpoint()
            : base("main-reactor", "instance")
        {
        }

        public long Value { get; set; }
    }
}