using System;

namespace Paddle.Core.Shared
{
    public class Instant : IEquatable<Instant>
    {
        private readonly DateTime _now;

        public Instant(DateTime now)
        {
            _now = now;
        }

        public bool Equals(Instant other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _now.Equals(other._now);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Instant)obj);
        }

        public override int GetHashCode()
        {
            return _now.GetHashCode();
        }

        public static bool operator ==(Instant left, Instant right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Instant left, Instant right)
        {
            return !Equals(left, right);
        }

        public static implicit operator DateTime(Instant i)
        {
            return i._now;
        }
    }
}