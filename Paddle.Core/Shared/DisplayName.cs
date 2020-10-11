namespace Paddle.Core.Shared
{
    public class DisplayName
    {
        private readonly string _name;

        public DisplayName(string name)
        {
            _name = name;
        }

        public static implicit operator string(DisplayName d) => d._name;
    }
}