using System;

namespace Common
{
    public class SeacTokenBare : IEquatable<SeacTokenBare>, IComparable<SeacTokenBare>
    {
        public string Token { get; set; }
        public string Class { get; set; }

        public bool Equals(SeacTokenBare other)
        {
            return Token.Equals(other.Token) && Class.Equals(other.Class);
        }

        public int CompareTo(SeacTokenBare other)
        {
            return Equals(other) ? 0 : 1;
        }
    }

    public class SeacToken : SeacTokenBare
    {
        public DateTime Expiration { get; set; }
        public bool SingleUse { get; set; }
    }
}