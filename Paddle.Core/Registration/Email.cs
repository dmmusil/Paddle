using System;
using System.Net.Mail;

namespace Paddle.Core.Registration
{
    public class Email : IEquatable<Email>
    {
        public bool Equals(Email other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _emailAddress == other._emailAddress;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Email)obj);
        }

        public override int GetHashCode() => _emailAddress.GetHashCode();

        public static bool operator ==(Email left, Email right) => Equals(left, right);

        public static bool operator !=(Email left, Email right) => !Equals(left, right);
        public static implicit operator string(Email email) => email._emailAddress;

        private readonly string _emailAddress;

        public Email(string emailAddress)
        {
            var _ = new MailAddress(emailAddress);
            _emailAddress = emailAddress;
        }
    }
}