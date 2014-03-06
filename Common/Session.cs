using System;

namespace Common
{
    public class Session
    {
        public Authentication Authentication { get; set; }
        public DateTime Expiration { get; set; }
    }
}